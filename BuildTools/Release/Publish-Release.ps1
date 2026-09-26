<#
.SYNOPSIS
Build a fixed local version tag and publish verified Windows/Android assets to GitHub.
.EXAMPLE
.\BuildTools\Release\Publish-Release.ps1 -Tag v1.0.2
.EXAMPLE
.\BuildTools\Release\Publish-Release.ps1 -Tag v1.0.2 -BuildOnly
.EXAMPLE
.\BuildTools\Release\Publish-Release.ps1 -Tag v1.0.2 -Resume
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)][string]$Tag,
    [ValidateSet('Windows', 'Android')][string[]]$Platforms = @('Windows', 'Android'),
    [string]$NotesFile,
    [string]$Remote = 'origin',
    [string]$UnityPath,
    [string]$InnoSetupPath,
    [string]$GhPath,
    [ValidateRange(1, 240)][int]$BuildTimeoutMinutes = 60,
    [switch]$BuildOnly,
    [switch]$Resume,
    [switch]$KeepWorktree
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
Import-Module (Join-Path $PSScriptRoot 'ReleaseTools.psm1') -Force -DisableNameChecking
$project = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..\..'))
$notesArgument = if ($NotesFile) { (Resolve-Path -LiteralPath $NotesFile).Path } else { $null }
$lock = $null
$source = $null
$buildComplete = $false
$previousGhToken = [Environment]::GetEnvironmentVariable('GH_TOKEN', 'Process')
Push-Location $project
try {
    $version = Get-ReleaseVersion $Tag
    $Platforms = @($Platforms | ForEach-Object { if ($_ -ieq 'Windows') { 'Windows' } else { 'Android' } } | Sort-Object -Unique)
    if ($Platforms.Count -eq 0) { throw 'Select at least one platform.' }
    $git = Resolve-ReleaseExecutable '' @() 'git.exe'
    $gitRoot = Invoke-ReleaseTool $git @('rev-parse', '--show-toplevel')
    if ([IO.Path]::GetFullPath($gitRoot) -ine $project) { throw 'The script must be in the Unity Git repository root.' }
    $localTag = Get-ReleaseLocalTag $git $Tag $Remote
    $commit = $localTag.commit
    $tagObject = $localTag.tagObject
    $remoteUrl = Invoke-ReleaseTool $git @('remote', 'get-url', $Remote)
    if ($remoteUrl -notmatch '^(?:https://github\.com/|git@github\.com:|ssh://git@github\.com/)([A-Za-z0-9_.-]+/[A-Za-z0-9_.-]+?)(?:\.git)?/?$') {
        throw 'The selected remote must be a github.com repository.'
    }
    $repo = $Matches[1]
    $settings = Invoke-ReleaseTool $git @('show', "${commit}:ProjectSettings/ProjectSettings.asset")
    $buildNumber = Get-ReleaseSettings $settings $version
    $editorSettings = Invoke-ReleaseTool $git @('show', "${commit}:ProjectSettings/ProjectVersion.txt")
    $editorMatch = [regex]::Match($editorSettings, '(?m)^m_EditorVersion:\s*(\S+)')
    if (!$editorMatch.Success) { throw 'Unity editor version is missing from the tagged source.' }
    $editorVersion = $editorMatch.Groups[1].Value
    if (!$Resume) {
        $entry = Invoke-ReleaseTool $git @('ls-tree', '--name-only', $commit, 'Assets/Editor/MutinyReleaseBuild.cs')
        if (!$entry) { throw "Tag '$Tag' predates this pipeline and does not contain Assets/Editor/MutinyReleaseBuild.cs. Commit the release tools, version settings and notes before creating a NEW release tag; existing published tags must stay fixed. See BuildTools/Release/README.md." }
        $notes = if ($notesArgument) { [IO.File]::ReadAllText($notesArgument) } else {
            $tagNotesPath = "BuildTools/Release/Notes/$Tag.md"
            $notesEntry = Invoke-ReleaseTool $git @('ls-tree', '--name-only', $commit, $tagNotesPath)
            if (!$notesEntry) { throw "Tag '$Tag' has no $tagNotesPath. Pass -NotesFile with a UTF-8 notes file, or commit notes before creating the next tag." }
            Invoke-ReleaseTool $git @('show', "${commit}:$tagNotesPath")
        }
        $notes = $notes.Replace("`r`n", "`n").TrimEnd("`n")
        if ([string]::IsNullOrWhiteSpace($notes)) { throw 'Release notes must not be empty.' }
    }
    $root = Join-Path $project "Builds\Release\$Tag"
    $null = New-Item -ItemType Directory -Path $root -Force
    try { $lock = [IO.File]::Open((Join-Path $root 'publish.lock'), 'OpenOrCreate', 'ReadWrite', 'None') }
    catch { throw "Another release process is using $Tag." }
    $manifestPath = Join-Path $root 'manifest.json'
    $savedNotes = Join-Path $root 'release-notes.md'
    $gh = $null
    if (!$BuildOnly) {
        $gh = Resolve-ReleaseExecutable $GhPath @("$env:ProgramFiles\GitHub CLI\gh.exe") 'gh.exe'
        $null = Initialize-ReleaseGitHubAuth $gh $git
        Assert-ReleaseRemoteTag $git $Remote $Tag $commit $tagObject
    }

    if ($Resume) {
        if (!(Test-Path -LiteralPath $manifestPath)) { throw 'No complete manifest to resume. Re-run without -Resume to rebuild.' }
        $manifest = [IO.File]::ReadAllText($manifestPath) | ConvertFrom-Json
        Assert-ReleaseManifest $manifest $Tag $commit $tagObject $repo $Platforms $version $buildNumber
        if ($notesArgument -and [IO.File]::ReadAllText($notesArgument).Replace("`r`n", "`n").TrimEnd("`n") -cne $manifest.notes) {
            throw 'Release notes changed. Resume uses the notes saved with the build.'
        }
        [IO.File]::WriteAllText($savedNotes, $manifest.notes, [Text.UTF8Encoding]::new($false))
    }
    else {
        if (Test-Path -LiteralPath $manifestPath) { throw "A complete build already exists for $Tag. Use -Resume or a new tag." }
        if (!$BuildOnly -and $null -ne (Get-ReleaseOnGitHub $gh $repo $Tag)) {
            throw 'A GitHub release already exists for this tag. Resume its original manifest or use a new tag.'
        }
        $unityCandidates = @($env:UNITY_EDITOR_PATH, "$env:ProgramFiles\Unity\Hub\Editor\$editorVersion\Editor\Unity.exe")
        # Do not resolve the 'unity' automation CLI; it is different from the Unity Editor executable.
        $unity = Resolve-ReleaseExecutable $UnityPath $unityCandidates 'UnityEditor.exe'
        $compiler = $null
        if ($Platforms -contains 'Windows') {
            $compilerCandidates = @($env:INNO_SETUP_COMPILER)
            foreach ($base in @($env:ProgramFiles, ${env:ProgramFiles(x86)}, "$env:LOCALAPPDATA\Programs")) {
                foreach ($innoVersion in @(7, 6)) { $compilerCandidates += Join-Path $base "Inno Setup $innoVersion\ISCC.exe" }
            }
            $compiler = Resolve-ReleaseExecutable $InnoSetupPath $compilerCandidates 'ISCC.exe'
        }
        $run = Join-Path $root ('runs\' + [DateTime]::UtcNow.ToString('yyyyMMdd-HHmmss') + '-' + [guid]::NewGuid().ToString('N').Substring(0, 8))
        $assetsPath = Join-Path $run 'assets'
        $null = New-Item -ItemType Directory -Path $assetsPath -Force
        # Keep Unity/Gradle source paths short; evidence files include deeply nested original AS2 paths.
        $source = Join-Path (Split-Path $project -Parent) ('_release-' + $Tag + '-' + [guid]::NewGuid().ToString('N').Substring(0, 8))
        [IO.File]::WriteAllText((Join-Path $run 'source-path.txt'), $source)
        Write-Host "[Release] $Tag / $commit / $version+$buildNumber / $($Platforms -join ', ')"
        $null = Invoke-ReleaseTool $git @('-c', 'core.longpaths=true', 'worktree', 'add', '--detach', $source, $commit)
        $builtAssets = @()
        foreach ($platform in $Platforms) {
            $output = if ($platform -eq 'Windows') { Join-Path $run 'raw\Windows\Mutiny X.exe' } else { Join-Path $run "raw\MutinyX-Android-$version+$buildNumber.apk" }
            $report = Join-Path $run "$platform-report.json"
            $log = Join-Path $run "$platform.log"
            Write-Host "[Release] Build $platform with Unity $editorVersion"
            Invoke-ReleaseUnity $unity $source $platform $version $buildNumber $editorVersion $output $report $log $BuildTimeoutMinutes
            if ($platform -eq 'Windows') {
                $builtAssets += New-ReleaseWindowsPackage (Split-Path $output -Parent) (Join-Path $run 'Windows') $assetsPath $version $buildNumber $compiler (Join-Path $source 'BuildTools\WindowsInstaller\MutinyX.iss')
            }
            else {
                $apk = Join-Path $assetsPath (Split-Path $output -Leaf)
                Copy-Item -LiteralPath $output -Destination $apk
                $builtAssets += $apk
            }
        }
        $assets = @($builtAssets | ForEach-Object { Get-ReleaseAsset $_ } | Sort-Object name)
        $sums = Join-Path $assetsPath 'SHA256SUMS.txt'
        [IO.File]::WriteAllText($sums, (($assets | ForEach-Object { "$($_.sha256)  $($_.name)" }) -join "`n") + "`n", [Text.UTF8Encoding]::new($false))
        $assets += Get-ReleaseAsset $sums
        $manifest = [pscustomobject]@{ schema = 1; tag = $Tag; tagObject = $tagObject; commit = $commit; repo = $repo;
            version = $version; buildNumber = $buildNumber; platforms = $Platforms; unityVersion = $editorVersion;
            createdAt = [DateTime]::UtcNow.ToString('o'); notes = $notes; assets = $assets }
        Assert-ReleaseManifest $manifest $Tag $commit $tagObject $repo $Platforms $version $buildNumber
        [IO.File]::WriteAllText($savedNotes, $notes, [Text.UTF8Encoding]::new($false))
        Write-ReleaseJson $manifestPath $manifest
        $buildComplete = $true
    }

    if ($BuildOnly) { Write-Host "[Release] Local build verified. Manifest: $manifestPath" }
    else {
        # Recheck after a long build, immediately before any GitHub writes.
        Assert-ReleaseRemoteTag $git $Remote $Tag $commit $tagObject
        Assert-ReleaseManifest $manifest $Tag $commit $tagObject $repo $Platforms $version $buildNumber
        $url = Publish-ReleaseManifest $gh $manifest $savedNotes (Join-Path $root 'github-verified.json') -Resume:$Resume
        Write-Host "[Release] Published and verified: $url"
    }
}
finally {
    if ($source -and $buildComplete -and !$KeepWorktree) {
        # Only delete this invocation's sibling worktree after verifying its Git root.
        $allowedParent = [IO.Path]::GetFullPath((Split-Path $project -Parent))
        $resolvedSource = [IO.Path]::GetFullPath($source)
        if ([IO.Path]::GetDirectoryName($resolvedSource) -ieq $allowedParent -and
            [IO.Path]::GetFileName($resolvedSource) -cmatch '^_release-v\d+\.\d+\.\d+-[0-9a-f]{8}$') {
            try {
                $worktreeRoot = Invoke-ReleaseTool $git @('-C', $resolvedSource, 'rev-parse', '--show-toplevel')
                if ([IO.Path]::GetFullPath($worktreeRoot) -ine $resolvedSource) { throw 'Worktree root mismatch.' }
                $null = Invoke-ReleaseTool $git @('-c', 'core.longpaths=true', 'worktree', 'remove', '--force', $resolvedSource)
            }
            catch { Write-Warning "Could not remove the temporary release worktree: $resolvedSource" }
        }
    }
    if ($lock) { $lock.Dispose() }
    [Environment]::SetEnvironmentVariable('GH_TOKEN', $previousGhToken, 'Process')
    Pop-Location
}
