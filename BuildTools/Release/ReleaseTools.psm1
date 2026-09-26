Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

function Invoke-ReleaseTool {
    param([string]$File, [string[]]$Arguments, [string]$InputText)
    $errorFile = [IO.Path]::GetTempFileName()
    $previousEncoding = [Console]::OutputEncoding
    try {
        # Git and gh emit UTF-8. Windows PowerShell's default OEM code page corrupts Chinese notes.
        [Console]::OutputEncoding = [Text.UTF8Encoding]::new($false)
        # Windows PowerShell represents native stderr as ErrorRecord even on success (e.g. git worktree).
        $previousPreference = $ErrorActionPreference
        $ErrorActionPreference = 'Continue'
        try {
            if ($PSBoundParameters.ContainsKey('InputText')) { $output = $InputText | & $File @Arguments 2> $errorFile }
            else { $output = & $File @Arguments 2> $errorFile }
        }
        finally { $ErrorActionPreference = $previousPreference }
        $code = $LASTEXITCODE
        if ($code -ne 0) {
            $detail = [IO.File]::ReadAllText($errorFile).Trim()
            throw "$(Split-Path $File -Leaf) failed (exit $code): $detail"
        }
        return ($output -join "`n")
    }
    finally {
        [Console]::OutputEncoding = $previousEncoding
        Remove-Item -LiteralPath $errorFile -ErrorAction SilentlyContinue
    }
}

function Write-ReleaseJson {
    param([string]$Path, $Value)
    [IO.File]::WriteAllText($Path, ($Value | ConvertTo-Json -Depth 12), [Text.UTF8Encoding]::new($false))
}

function Get-ReleaseVersion {
    param([string]$Tag)
    if ($Tag -cnotmatch '^v(0|[1-9]\d*)\.(0|[1-9]\d*)\.(0|[1-9]\d*)$') {
        throw 'Tag must be vMAJOR.MINOR.PATCH, for example v1.0.2.'
    }
    return $Tag.Substring(1)
}

function Get-ReleaseLocalTag {
    param([string]$Git, [string]$Tag, [string]$Remote)
    $null = Get-ReleaseVersion $Tag
    $tagRef = "refs/tags/$Tag"
    $existing = Invoke-ReleaseTool $Git @('tag', '--list', $Tag)
    if ([string]::IsNullOrWhiteSpace($existing)) {
        $available = Invoke-ReleaseTool $Git @('tag', '--list', 'v*', '--sort=-version:refname')
        $available = if ($available) { (($available -split "`n" | Select-Object -First 10) -join ', ') } else { '(none)' }
        throw @"
Local tag '$Tag' does not exist. Existing local version tags: $available.
- If the tag already exists on the remote, fetch it: git fetch $Remote tag $Tag
- For a new release, commit the release tools, matching Unity version/build number and notes first; then create and push the tag:
  git tag -a $Tag -m "Release $Tag"
  git push $Remote $Tag
The -Tag parameter selects an existing tag; it does not create one. See BuildTools/Release/README.md.
"@
    }
    $tagObject = Invoke-ReleaseTool $Git @('rev-parse', '--verify', $tagRef)
    try { $commit = Invoke-ReleaseTool $Git @('rev-parse', '--verify', "${tagRef}^{commit}") }
    catch { throw "Local tag '$Tag' exists but does not reference a commit. Select a version tag that points to a source commit." }
    return [pscustomobject]@{ commit = $commit; tagObject = $tagObject }
}

function Get-ReleaseSettings {
    param([string]$Text, [string]$Version)
    $matchVersion = [regex]::Match($Text, '(?m)^\s*bundleVersion:\s*([^\r\n]+)')
    $matchBuild = [regex]::Match($Text, '(?m)^\s*AndroidBundleVersionCode:\s*(\d+)\s*$')
    if (!$matchVersion.Success -or $matchVersion.Groups[1].Value.Trim().Trim('"', "'") -cne $Version) {
        $actual = if ($matchVersion.Success) { $matchVersion.Groups[1].Value.Trim().Trim('"', "'") } else { '(missing)' }
        throw "The tag's bundleVersion does not match ${Version}: the tagged commit contains '$actual'. Changes in the current working directory are not part of the tag. Commit the version/build number, release tools and notes before tagging. If this tag was already pushed, first verify it has not been published before repairing it."
    }
    if (!$matchBuild.Success) { throw 'AndroidBundleVersionCode is missing.' }
    $build = [long]$matchBuild.Groups[1].Value
    if ($build -lt 1 -or $build -gt 2100000000) { throw 'Android build number is out of range.' }
    return [int]$build
}

function Initialize-ReleaseGitHubAuth {
    param([string]$Gh, [string]$Git)
    try {
        $null = Invoke-ReleaseTool $Gh @('auth', 'status', '--hostname', 'github.com')
        return $false
    }
    catch { }

    $previousToken = [Environment]::GetEnvironmentVariable('GH_TOKEN', 'Process')
    $previousPrompt = [Environment]::GetEnvironmentVariable('GIT_TERMINAL_PROMPT', 'Process')
    $previousInteractive = [Environment]::GetEnvironmentVariable('GCM_INTERACTIVE', 'Process')
    $credentialText = $null
    $password = $null
    try {
        $env:GIT_TERMINAL_PROMPT = '0'
        $env:GCM_INTERACTIVE = 'Never'
        # Capture credential output privately. Do not log it or include it in a report.
        $credentialText = Invoke-ReleaseTool $Git @('credential', 'fill') -InputText "protocol=https`nhost=github.com`n`n"
        foreach ($line in ($credentialText -split "`n")) {
            if ($line.StartsWith('password=', [StringComparison]::Ordinal)) { $password = $line.Substring(9).TrimEnd("`r") }
        }
        if ([string]::IsNullOrWhiteSpace($password)) { throw 'No cached credential.' }
        $env:GH_TOKEN = $password
        $null = Invoke-ReleaseTool $Gh @('auth', 'status', '--hostname', 'github.com')
        Write-Host '[Release] GitHub authentication uses cached Git credentials for this process.'
        return $true
    }
    catch {
        [Environment]::SetEnvironmentVariable('GH_TOKEN', $previousToken, 'Process')
        throw 'GitHub authentication is unavailable. Run gh auth login, then retry. Cached Git credentials could not be used.'
    }
    finally {
        [Environment]::SetEnvironmentVariable('GIT_TERMINAL_PROMPT', $previousPrompt, 'Process')
        [Environment]::SetEnvironmentVariable('GCM_INTERACTIVE', $previousInteractive, 'Process')
        $credentialText = $null
        $password = $null
    }
}

function Assert-ReleaseRemoteTag {
    param([string]$Git, [string]$Remote, [string]$Tag, [string]$Commit, [string]$TagObject)
    $text = Invoke-ReleaseTool $Git @('ls-remote', '--tags', $Remote, "refs/tags/$Tag", "refs/tags/$Tag^{}")
    $refs = @{}
    foreach ($line in ($text -split "`n")) {
        if ($line -match '^([0-9a-f]{40,64})\s+(.+)$') { $refs[$Matches[2]] = $Matches[1] }
    }
    if (!$refs.ContainsKey("refs/tags/$Tag")) { throw "Push tag $Tag to $Remote before publishing." }
    $remoteCommit = $refs["refs/tags/$Tag"]
    if ($refs.ContainsKey("refs/tags/$Tag^{}")) { $remoteCommit = $refs["refs/tags/$Tag^{}"] }
    if ($remoteCommit -cne $Commit -or $refs["refs/tags/$Tag"] -cne $TagObject) {
        throw 'Local and remote tags differ. Refusing to release a different source revision.'
    }
}

function Resolve-ReleaseExecutable {
    param([string]$Explicit, [string[]]$Candidates, [string]$Name)
    if ($Explicit) {
        if (!(Test-Path -LiteralPath $Explicit -PathType Leaf)) { throw "$Name not found: $Explicit" }
        return (Resolve-Path -LiteralPath $Explicit).Path
    }
    foreach ($candidate in $Candidates) {
        if ($candidate -and (Test-Path -LiteralPath $candidate -PathType Leaf)) { return [IO.Path]::GetFullPath($candidate) }
    }
    $command = Get-Command $Name -CommandType Application -ErrorAction SilentlyContinue | Select-Object -First 1
    if ($command) { return $command.Source }
    throw "$Name was not found. Pass its executable path explicitly."
}

function ConvertTo-ReleaseProcessArgument {
    param([string]$Value)
    # Windows CommandLineToArgvW quoting, including backslashes before a quote or closing quote.
    return '"' + [regex]::Replace([regex]::Replace($Value, '(\\*)"', '$1$1\"'), '(\\+)$', '$1$1') + '"'
}

function Invoke-ReleaseUnity {
    param([string]$Unity, [string]$Source, [string]$Platform, [string]$Version, [int]$BuildNumber,
          [string]$UnityVersion, [string]$Output, [string]$Report, [string]$Log, [int]$TimeoutMinutes)
    $target = if ($Platform -eq 'Windows') { 'Win64' } else { 'Android' }
    $arguments = @('-batchmode', '-nographics', '-quit', '-projectPath', $Source, '-buildTarget', $target,
        '-executeMethod', 'MutinyReleaseBuild.Build', '-mutinyPlatform', $Platform, '-mutinyVersion', $Version,
        '-mutinyBuildNumber', "$BuildNumber", '-mutinyOutput', $Output, '-mutinyReport', $Report, '-logFile', $Log)
    $start = [Diagnostics.ProcessStartInfo]::new()
    $start.FileName = $Unity
    $start.Arguments = ($arguments | ForEach-Object { ConvertTo-ReleaseProcessArgument $_ }) -join ' '
    $start.WorkingDirectory = $Source
    $start.UseShellExecute = $false
    $start.CreateNoWindow = $true
    $start.WindowStyle = [Diagnostics.ProcessWindowStyle]::Hidden
    $start.RedirectStandardOutput = $true
    $start.RedirectStandardError = $true
    $process = [Diagnostics.Process]::Start($start)
    $stdout = $process.StandardOutput.ReadToEndAsync()
    $stderr = $process.StandardError.ReadToEndAsync()
    $watch = [Diagnostics.Stopwatch]::StartNew()
    $nextUpdate = 30
    try {
        while (!$process.WaitForExit(1000)) {
            if ($watch.Elapsed.TotalMinutes -ge $TimeoutMinutes) {
                $process.Kill()
                $process.WaitForExit()
                throw "$Platform exceeded the $TimeoutMinutes minute timeout. See $Log"
            }
            if ($watch.Elapsed.TotalSeconds -ge $nextUpdate) {
                Write-Host "[Release] $Platform building ($([int]$watch.Elapsed.TotalSeconds)s). Log: $Log"
                $nextUpdate += 30
            }
        }
        # Do not echo Unity startup output: it can contain local license/account details.
        $null = $stdout.GetAwaiter().GetResult()
        $null = $stderr.GetAwaiter().GetResult()
        if ($process.ExitCode -ne 0) { throw "$Platform Unity build failed (exit $($process.ExitCode)). See $Log" }
    }
    finally { $process.Dispose() }
    if (!(Test-Path -LiteralPath $Report -PathType Leaf)) { throw "$Platform build report is missing. See $Log" }
    $result = [IO.File]::ReadAllText($Report) | ConvertFrom-Json
    if (!$result.success -or $result.platform -cne $Platform -or $result.version -cne $Version -or
        $result.buildNumber -ne $BuildNumber -or $result.unityVersion -cne $UnityVersion -or
        [IO.Path]::GetFullPath($result.outputPath) -ine [IO.Path]::GetFullPath($Output)) {
        throw "$Platform build report does not match the requested release. See $Report"
    }
    if (!(Test-Path -LiteralPath $Output -PathType Leaf) -or (Get-Item -LiteralPath $Output).Length -eq 0) {
        throw "$Platform output file is missing or empty."
    }
}

function Test-ReleaseShippingPath {
    param([string]$RelativePath)
    return $RelativePath -notmatch '(?i)(^|[\\/])[^\\/]*(?:_BackUpThisFolder_ButDontShipItWithYourGame|_BurstDebugInformation_DoNotShip)([\\/]|$)' -and
        $RelativePath -notmatch '(?i)\.pdb$'
}

function New-ReleaseWindowsPackage {
    param([string]$Raw, [string]$Stage, [string]$Assets, [string]$Version, [int]$BuildNumber,
          [string]$Compiler, [string]$InstallerScript)
    $null = New-Item -ItemType Directory -Path $Stage -Force
    $root = [IO.Path]::GetFullPath($Raw).TrimEnd('\', '/') + [IO.Path]::DirectorySeparatorChar
    foreach ($file in Get-ChildItem -LiteralPath $Raw -File -Recurse -Force) {
        $relative = $file.FullName.Substring($root.Length)
        if (Test-ReleaseShippingPath $relative) {
            $destination = Join-Path $Stage $relative
            $null = New-Item -ItemType Directory -Path (Split-Path $destination -Parent) -Force
            Copy-Item -LiteralPath $file.FullName -Destination $destination
        }
    }
    if (!(Test-Path -LiteralPath (Join-Path $Stage 'Mutiny X.exe'))) { throw 'Windows staging is missing the player executable.' }
    Add-Type -AssemblyName System.IO.Compression.FileSystem
    $zip = Join-Path $Assets "MutinyX-Windows-$Version+$BuildNumber.zip"
    [IO.Compression.ZipFile]::CreateFromDirectory($Stage, $zip, [IO.Compression.CompressionLevel]::Optimal, $false)
    $archive = [IO.Compression.ZipFile]::OpenRead($zip)
    try {
        foreach ($entry in $archive.Entries) {
            if (!(Test-ReleaseShippingPath $entry.FullName)) { throw "ZIP includes non-shipping content: $($entry.FullName)" }
        }
    }
    finally { $archive.Dispose() }
    $keys = @('MUTINY_APP_VERSION', 'MUTINY_BUILD_NUMBER', 'MUTINY_SOURCE_DIR', 'MUTINY_OUTPUT_DIR')
    $previous = @{}
    foreach ($key in $keys) { $previous[$key] = [Environment]::GetEnvironmentVariable($key, 'Process') }
    try {
        $env:MUTINY_APP_VERSION = $Version
        $env:MUTINY_BUILD_NUMBER = "$BuildNumber"
        $env:MUTINY_SOURCE_DIR = $Stage
        $env:MUTINY_OUTPUT_DIR = $Assets
        $log = Invoke-ReleaseTool $Compiler @($InstallerScript)
        [IO.File]::WriteAllText((Join-Path (Split-Path $Assets -Parent) 'inno.log'), $log)
    }
    finally {
        foreach ($key in $keys) { [Environment]::SetEnvironmentVariable($key, $previous[$key], 'Process') }
    }
    $installer = Join-Path $Assets "MutinyX-Setup-$Version+$BuildNumber.exe"
    if (!(Test-Path -LiteralPath $installer) -or (Get-Item -LiteralPath $installer).Length -eq 0) { throw 'Installer output is missing or empty.' }
    return @($zip, $installer)
}

function Get-ReleaseAsset {
    param([string]$Path)
    $file = Get-Item -LiteralPath $Path
    if ($file.Length -eq 0) { throw "Empty release asset: $Path" }
    return [pscustomobject]@{ name = $file.Name; path = $file.FullName; size = $file.Length;
        sha256 = (Get-FileHash -LiteralPath $Path -Algorithm SHA256).Hash.ToLowerInvariant() }
}

function Assert-ReleaseManifest {
    param($Manifest, [string]$Tag, [string]$Commit, [string]$TagObject, [string]$Repo,
          [string[]]$Platforms, [string]$Version, [int]$BuildNumber)
    if ($Manifest.schema -ne 1 -or $Manifest.tag -cne $Tag -or $Manifest.commit -cne $Commit -or
        $Manifest.tagObject -cne $TagObject -or $Manifest.repo -ine $Repo -or
        $Manifest.version -cne $Version -or $Manifest.buildNumber -ne $BuildNumber -or
        (($Manifest.platforms | Sort-Object) -join ',') -cne (($Platforms | Sort-Object) -join ',')) {
        throw 'Manifest identity does not match this tag, repository or platform selection.'
    }
    $expected = @('SHA256SUMS.txt')
    if ($Platforms -contains 'Windows') { $expected += "MutinyX-Windows-$Version+$BuildNumber.zip", "MutinyX-Setup-$Version+$BuildNumber.exe" }
    if ($Platforms -contains 'Android') { $expected += "MutinyX-Android-$Version+$BuildNumber.apk" }
    if ((@($Manifest.assets).Count -ne $expected.Count) -or
        (($Manifest.assets.name | Sort-Object) -join ',') -cne (($expected | Sort-Object) -join ',')) {
        throw 'Manifest asset names do not match the requested platforms.'
    }
    foreach ($asset in $Manifest.assets) {
        $actual = Get-ReleaseAsset $asset.path
        if ($actual.name -cne $asset.name -or $actual.size -ne $asset.size -or $actual.sha256 -cne $asset.sha256) {
            throw "Local release asset changed: $($asset.name). Refusing to upload."
        }
    }
}

function Invoke-ReleaseGhJson {
    param([string]$Gh, [string[]]$Arguments)
    return (Invoke-ReleaseTool $Gh $Arguments | ConvertFrom-Json)
}

function Get-ReleaseOnGitHub {
    param([string]$Gh, [string]$Repo, [string]$Tag)
    # Listing also finds drafts; the tags REST endpoint can return 404 for a draft.
    $pages = Invoke-ReleaseGhJson $Gh @('api', "repos/$Repo/releases?per_page=100", '--paginate', '--slurp')
    $matches = @($pages | ForEach-Object { $_ } | Where-Object { $null -ne $_ -and $_.tag_name -ceq $Tag })
    if ($matches.Count -gt 1) { throw 'Multiple releases have the same tag.' }
    if ($matches.Count -eq 1) { return $matches[0] }
    return $null
}

function Assert-ReleaseGitHubIdentity {
    param($Release, $Manifest)
    $body = ([string]$Release.body).Replace("`r`n", "`n").TrimEnd("`n")
    $notes = ([string]$Manifest.notes).Replace("`r`n", "`n").TrimEnd("`n")
    if ($Release.tag_name -cne $Manifest.tag -or $Release.target_commitish -cne $Manifest.commit -or
        $Release.name -cne "Mutiny X $($Manifest.tag)" -or $body -cne $notes -or $Release.prerelease) {
        throw 'Existing GitHub release identity or notes do not match the manifest. Refusing to modify it.'
    }
}

function Test-ReleaseRemoteAsset {
    param($RemoteAsset, $LocalAsset)
    return $RemoteAsset.state -ceq 'uploaded' -and $RemoteAsset.name -ceq $LocalAsset.name -and
        $RemoteAsset.size -eq $LocalAsset.size -and $RemoteAsset.digest -ceq "sha256:$($LocalAsset.sha256)"
}

function Assert-ReleaseGitHubAssets {
    param($Release, $Manifest)
    $remote = @($Release.assets)
    if ($remote.Count -ne @($Manifest.assets).Count) { throw 'GitHub asset count is incomplete or contains unexpected assets.' }
    foreach ($asset in $Manifest.assets) {
        $match = @($remote | Where-Object { $_.name -ceq $asset.name })
        if ($match.Count -ne 1 -or !(Test-ReleaseRemoteAsset $match[0] $asset)) {
            throw "GitHub size/SHA256 verification failed: $($asset.name)"
        }
    }
}

function Publish-ReleaseManifest {
    param([string]$Gh, $Manifest, [string]$NotesPath, [string]$VerificationPath, [switch]$Resume)
    if ([string]::IsNullOrWhiteSpace($Manifest.notes)) { throw 'Release notes must not be empty.' }
    $repo = $Manifest.repo
    $tag = $Manifest.tag
    $release = Get-ReleaseOnGitHub $Gh $repo $tag
    if ($null -eq $release) {
        $null = Invoke-ReleaseTool $Gh @('release', 'create', $tag, '--repo', $repo, '--target', $Manifest.commit,
            '--title', "Mutiny X $tag", '--notes-file', $NotesPath, '--draft', '--verify-tag')
        $release = Get-ReleaseOnGitHub $Gh $repo $tag
        if ($null -eq $release) { throw 'Draft creation did not return a release.' }
    }
    Assert-ReleaseGitHubIdentity $release $Manifest
    if (!$release.draft) {
        if (!$Resume) { throw 'This tag is already published. Use a new version tag.' }
        Assert-ReleaseGitHubAssets $release $Manifest
        Write-ReleaseJson $VerificationPath $release
        return $release.html_url
    }

    foreach ($remote in $release.assets) {
        if ($Manifest.assets.name -cnotcontains $remote.name) { throw 'Draft contains an unexpected asset. Refusing to publish.' }
    }
    foreach ($asset in $Manifest.assets) {
        $uploaded = $false
        for ($attempt = 1; $attempt -le 3; $attempt++) {
            $release = Invoke-ReleaseGhJson $Gh @('api', "repos/$repo/releases/$($release.id)")
            Assert-ReleaseGitHubIdentity $release $Manifest
            if (!$release.draft) { throw 'Release was published by another process during upload.' }
            $match = @($release.assets | Where-Object { $_.name -ceq $asset.name })
            if ($match.Count -eq 1 -and (Test-ReleaseRemoteAsset $match[0] $asset)) { $uploaded = $true; break }
            try {
                Write-Host "[Release] Upload $($asset.name) (attempt $attempt/3)"
                $null = Invoke-ReleaseTool $Gh @('release', 'upload', $tag, $asset.path, '--repo', $repo, '--clobber')
            }
            catch {
                if ($attempt -eq 3) { throw }
                Write-Warning "Upload interrupted; verifying server state before retry: $($asset.name)"
            }
            $release = Invoke-ReleaseGhJson $Gh @('api', "repos/$repo/releases/$($release.id)")
            $match = @($release.assets | Where-Object { $_.name -ceq $asset.name })
            if ($match.Count -eq 1 -and (Test-ReleaseRemoteAsset $match[0] $asset)) { $uploaded = $true; break }
        }
        if (!$uploaded) { throw "Upload verification failed after retries: $($asset.name)" }
    }

    $release = Invoke-ReleaseGhJson $Gh @('api', "repos/$repo/releases/$($release.id)")
    Assert-ReleaseGitHubIdentity $release $Manifest
    Assert-ReleaseGitHubAssets $release $Manifest
    if (!$release.draft) { throw 'Draft state changed before publication.' }
    # Persist evidence before the final write. A failed final response can be safely verified with -Resume.
    Write-ReleaseJson $VerificationPath $release
    $null = Invoke-ReleaseTool $Gh @('release', 'edit', $tag, '--repo', $repo, '--draft=false', '--latest')
    $release = Invoke-ReleaseGhJson $Gh @('api', "repos/$repo/releases/$($release.id)")
    Assert-ReleaseGitHubIdentity $release $Manifest
    Assert-ReleaseGitHubAssets $release $Manifest
    if ($release.draft -or !$release.published_at) { throw 'GitHub did not confirm publication. Re-run with -Resume.' }
    Write-ReleaseJson $VerificationPath $release
    return $release.html_url
}

Export-ModuleMember -Function *-Release*
