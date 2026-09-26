[CmdletBinding()]
param()
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$releaseTools = Split-Path $PSScriptRoot -Parent
$project = [IO.Path]::GetFullPath((Join-Path $releaseTools '..\..'))
Import-Module (Join-Path $releaseTools 'ReleaseTools.psm1') -Force -DisableNameChecking
$root = Join-Path $project ('Builds\ReleaseTests\' + [guid]::NewGuid().ToString('N'))
$null = New-Item -ItemType Directory -Path $root -Force
$git = (Get-Command git.exe).Source
$script:passed = 0
$previousPath = $env:PATH
$previousGhToken = [Environment]::GetEnvironmentVariable('GH_TOKEN', 'Process')

function Assert-Case([bool]$Condition, [string]$Name) {
    if (!$Condition) { throw "FAIL: $Name" }
    $script:passed++
    Write-Host "PASS: $Name"
}
function Assert-Throws([scriptblock]$Action, [string]$Pattern, [string]$Name) {
    $message = $null
    try { & $Action | Out-Null } catch { $message = $_.Exception.Message }
    Assert-Case ($null -ne $message -and $message -match $Pattern) "$Name ($message)"
}
function Write-TestFile([string]$Path, [string]$Text) {
    $absolute = [IO.Path]::GetFullPath($Path)
    if ($absolute.Length -gt 240) { $absolute = '\\?\' + $absolute }
    $null = [IO.Directory]::CreateDirectory([IO.Path]::GetDirectoryName($absolute))
    [IO.File]::WriteAllText($absolute, $Text, [Text.UTF8Encoding]::new($false))
}

try {
    # A real child executable exercises argument quoting, exit status and JSON reports.
    $fakeUnitySource = Join-Path $root 'FakeUnity.cs'
    Write-TestFile $fakeUnitySource @'
using System;
using System.IO;
using System.Collections.Generic;
using System.Web.Script.Serialization;
public static class FakeUnity {
    static string Arg(string[] a, string n) { return a[Array.IndexOf(a,n)+1]; }
    public static int Main(string[] a) {
        string mode = Environment.GetEnvironmentVariable("MUTINY_TEST_UNITY_MODE");
        string source = Arg(a,"-projectPath");
        string version = Arg(a,"-mutinyVersion");
        string platform = Arg(a,"-mutinyPlatform");
        string output = Arg(a,"-mutinyOutput");
        string report = Arg(a,"-mutinyReport");
        File.WriteAllText(Arg(a,"-logFile"),"Test Unity process\n");
        if (!File.ReadAllText(Path.Combine(source,"ProjectSettings/ProjectSettings.asset")).Contains("bundleVersion: " + version)) return 9;
        if (mode=="exit") return 7;
        Directory.CreateDirectory(Path.GetDirectoryName(output));
        File.WriteAllText(output,"fixture player data");
        if (platform=="Windows") {
            string raw=Path.GetDirectoryName(output);
            File.WriteAllText(Path.Combine(raw,"UnityPlayer.dll"),"fixture dll");
            File.WriteAllText(Path.Combine(raw,"ignore.pdb"),"debug symbols");
            string debug=Path.Combine(raw,"Mutiny X_BurstDebugInformation_DoNotShip");
            Directory.CreateDirectory(debug); File.WriteAllText(Path.Combine(debug,"debug.txt"),"debug");
            string backup=Path.Combine(raw,"Mutiny X_BackUpThisFolder_ButDontShipItWithYourGame");
            Directory.CreateDirectory(backup); File.WriteAllText(Path.Combine(backup,"backup.txt"),"backup");
        }
        if (mode=="missing") return 0;
        var result=new Dictionary<string,object> {
            {"success", mode!="false"}, {"platform",platform}, {"version",mode=="mismatch"?"0.0.0":version},
            {"buildNumber",int.Parse(Arg(a,"-mutinyBuildNumber"))}, {"unityVersion","6000.6.0f1"},
            {"outputPath",output}, {"error","fixture"}
        };
        File.WriteAllText(report,new JavaScriptSerializer().Serialize(result));
        return 0;
    }
}
'@
    $fakeUnity = Join-Path $root 'Fake Unity.exe'
    $csc = Join-Path $env:WINDIR 'Microsoft.NET\Framework64\v4.0.30319\csc.exe'
    $null = Invoke-ReleaseTool $csc @('/nologo', '/r:System.Web.Extensions.dll', "/out:$fakeUnity", $fakeUnitySource)
    $fakeInno = Join-Path $root 'Fake Inno.ps1'
    Write-TestFile $fakeInno @'
if (Get-ChildItem -LiteralPath $env:MUTINY_SOURCE_DIR -File -Recurse | Where-Object { $_.Name -like '*.pdb' -or $_.FullName -like '*DontShip*' -or $_.FullName -like '*DoNotShip*' }) {
    $global:LASTEXITCODE = 8
    Write-Error 'Installer staging includes debug files' -ErrorAction Continue
    return
}
[IO.File]::WriteAllText((Join-Path $env:MUTINY_OUTPUT_DIR "MutinyX-Setup-$env:MUTINY_APP_VERSION+$env:MUTINY_BUILD_NUMBER.exe"), 'fixture installer')
Write-Output 'Fixture Inno used filtered staging'
$global:LASTEXITCODE = 0
'@

    $fixture = Join-Path $root 'Tag Project'
    $null = New-Item -ItemType Directory -Path $fixture -Force
    $null = New-Item -ItemType Directory -Path (Join-Path $fixture 'BuildTools') -Force
    Copy-Item -LiteralPath $releaseTools -Destination (Join-Path $fixture 'BuildTools') -Recurse
    Copy-Item -LiteralPath (Join-Path $project 'BuildTools\WindowsInstaller') -Destination (Join-Path $fixture 'BuildTools') -Recurse
    Write-TestFile (Join-Path $fixture 'Assets\Editor\MutinyReleaseBuild.cs') '// fixture marker'
    Write-TestFile (Join-Path $fixture 'ProjectSettings\ProjectSettings.asset') "PlayerSettings:`n  bundleVersion: 1.2.3`n  AndroidBundleVersionCode: 42`n"
    Write-TestFile (Join-Path $fixture 'ProjectSettings\ProjectVersion.txt') "m_EditorVersion: 6000.6.0f1`n"
    $fixtureNotes = '1 ' + [char]0x52a0 + [char]0x5165 + [char]0x5934 + [char]0x50cf + "`n2 gm aitakeover 1"
    Write-TestFile (Join-Path $fixture 'BuildTools\Release\Notes\v1.2.3.md') ($fixtureNotes + "`n")
    $longRelative = 'Docs\' + (('DeepOriginalEvidence\' * 9)) + 'long-source.as'
    Write-TestFile (Join-Path $fixture $longRelative) '// long path regression'
    Push-Location $fixture
    try {
        $null = Invoke-ReleaseTool $git @('init', '--quiet')
        $null = Invoke-ReleaseTool $git @('config', 'user.name', 'Release fixture')
        $null = Invoke-ReleaseTool $git @('config', 'user.email', 'fixture@example.invalid')
        $null = Invoke-ReleaseTool $git @('config', 'core.longpaths', 'true')
        $null = Invoke-ReleaseTool $git @('remote', 'add', 'origin', 'https://github.com/RichardXue123/Mutiny-X.git')
        $null = Invoke-ReleaseTool $git @('add', '.')
        $null = Invoke-ReleaseTool $git @('commit', '--quiet', '-m', 'Fixture tagged source')
        $null = Invoke-ReleaseTool $git @('tag', '-a', 'v1.2.3', '-m', 'Fixture')
        $null = Invoke-ReleaseTool $git @('tag', 'v1.2.4')
        $tree = Invoke-ReleaseTool $git @('rev-parse', 'HEAD^{tree}')
        $null = Invoke-ReleaseTool $git @('tag', 'v1.2.6', $tree)
    }
    finally { Pop-Location }
    $launcher = Join-Path $fixture 'BuildTools\Release\Publish-Release.ps1'
    # An uncommitted version must not leak into the build.
    Write-TestFile (Join-Path $fixture 'ProjectSettings\ProjectSettings.asset') "bundleVersion: 99.0.0`nAndroidBundleVersionCode: 999`n"
    $originalConsoleEncoding = [Console]::OutputEncoding
    try {
        [Console]::OutputEncoding = [Text.Encoding]::GetEncoding(936)
        & $launcher -Tag v1.2.3 -BuildOnly -UnityPath $fakeUnity -InnoSetupPath $fakeInno
    }
    finally { [Console]::OutputEncoding = $originalConsoleEncoding }
    $manifestPath = Join-Path $fixture 'Builds\Release\v1.2.3\manifest.json'
    $manifest = [IO.File]::ReadAllText($manifestPath) | ConvertFrom-Json
    Assert-Case ($manifest.version -ceq '1.2.3' -and $manifest.buildNumber -eq 42 -and @($manifest.assets).Count -eq 4) 'Tag source isolation and four default assets'
    Assert-Case ((Get-Content -LiteralPath (Join-Path $fixture 'ProjectSettings\ProjectSettings.asset') -Raw) -match '99.0.0') 'Current working tree is preserved'
    Assert-Case $true 'Long original evidence paths are checked out successfully'
    Assert-Case ($manifest.notes -ceq $fixtureNotes) 'Chinese release notes survive a non-UTF8 console'
    $zip = @($manifest.assets | Where-Object { $_.name -like '*.zip' })[0]
    $archive = [IO.Compression.ZipFile]::OpenRead($zip.path)
    try { Assert-Case ($archive.Entries.Count -eq 2 -and @($archive.Entries | Where-Object { !(Test-ReleaseShippingPath $_.FullName) }).Count -eq 0) 'ZIP and installer staging filter debug/backup content' }
    finally { $archive.Dispose() }
    & $launcher -Tag v1.2.3 -BuildOnly -Resume
    Assert-Case $true 'BuildOnly resume needs no Unity/Inno/GitHub authentication'
    Assert-Throws { & $launcher -Tag v1.2.3 -BuildOnly } 'complete build' 'Existing build is not silently replaced'
    Assert-Throws { & $launcher -Tag 'v1.2.3/invalid' -BuildOnly } 'Tag must' 'Invalid tag rejected'
    Assert-Throws { & $launcher -Tag v1.2.4 -BuildOnly } 'bundleVersion' 'Tag version mismatch rejected before building'
    $missingTagError = $null
    try { & $launcher -Tag v1.2.5 -BuildOnly | Out-Null } catch { $missingTagError = $_.Exception.Message }
    Assert-Case ($null -ne $missingTagError -and $missingTagError -match "Local tag 'v1.2.5' does not exist" -and
        $missingTagError -match 'v1.2.3' -and $missingTagError -notmatch 'Needed a single revision|NativeCommandError') 'Missing local tag has actionable diagnostics without Git 128 stack'
    Assert-Case (!(Test-Path -LiteralPath (Join-Path $fixture 'Builds\Release\v1.2.5'))) 'Missing local tag stops before creating build output'
    Assert-Throws { & $launcher -Tag v1.2.6 -BuildOnly } 'does not reference a commit' 'Non-commit tag has a separate diagnostic'
    $assetPath = $manifest.assets[0].path
    $originalBytes = [IO.File]::ReadAllBytes($assetPath)
    [IO.File]::AppendAllText($assetPath, 'tamper')
    Assert-Throws { & $launcher -Tag v1.2.3 -BuildOnly -Resume } 'asset changed' 'Tampered local asset rejected'
    [IO.File]::WriteAllBytes($assetPath, $originalBytes)
    $held = [IO.File]::Open((Join-Path $fixture 'Builds\Release\v1.2.3\publish.lock'), 'Open', 'ReadWrite', 'None')
    try { Assert-Throws { & $launcher -Tag v1.2.3 -BuildOnly -Resume } 'Another release process' 'Concurrent tag publication blocked' }
    finally { $held.Dispose() }

    $failedFixture = Join-Path $root 'Failure Project'
    $null = Invoke-ReleaseTool $git @('-c', 'core.longpaths=true', 'clone', '--quiet', $fixture, $failedFixture)
    Push-Location $failedFixture
    try { $null = Invoke-ReleaseTool $git @('remote', 'set-url', 'origin', 'https://github.com/RichardXue123/Mutiny-X.git') }
    finally { Pop-Location }
    $failedLauncher = Join-Path $failedFixture 'BuildTools\Release\Publish-Release.ps1'
    foreach ($mode in @('exit', 'missing', 'false', 'mismatch')) {
        $env:MUTINY_TEST_UNITY_MODE = $mode
        Assert-Throws { & $failedLauncher -Tag v1.2.3 -BuildOnly -Platforms Android -UnityPath $fakeUnity } 'failed|missing|does not match' "Build gate: $mode"
        Assert-Case (!(Test-Path -LiteralPath (Join-Path $failedFixture 'Builds\Release\v1.2.3\manifest.json'))) "No publishable manifest after $mode"
    }
    Remove-Item Env:\MUTINY_TEST_UNITY_MODE

    # Exercise the real publishing function with a stateful CLI double. No online writes.
    $fakeGh = Join-Path $root 'Fake GitHub.ps1'
    Write-TestFile $fakeGh @'
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$a = @($args)
$state = [IO.File]::ReadAllText($env:MUTINY_TEST_GH_STATE) | ConvertFrom-Json
function Save { [IO.File]::WriteAllText($env:MUTINY_TEST_GH_STATE, ($state | ConvertTo-Json -Depth 12)) }
function Value([string]$flag) { return $a[[Array]::IndexOf($a, $flag) + 1] }
$state.events = @($state.events) + ($a[0..([Math]::Min(1,$a.Count-1))] -join ' ')
$global:LASTEXITCODE = 0
if ($a[0] -eq 'api') {
    if ($a -contains '--slurp') {
        if ($null -eq $state.release) { Write-Output '[[]]' }
        else { Write-Output ('[[' + ($state.release | ConvertTo-Json -Depth 12 -Compress) + ']]') }
    } else { Write-Output ($state.release | ConvertTo-Json -Depth 12 -Compress) }
}
elseif ($a[0] -eq 'auth' -and $a[1] -eq 'status') {
    if ($state.mode -eq 'auth-fallback' -and $env:GH_TOKEN -cne 'fixture-cached-token') {
        $global:LASTEXITCODE=1; Save; Write-Error 'Fixture keyring credential unavailable' -ErrorAction Continue; return
    }
}
elseif ($a[0] -eq 'release' -and $a[1] -eq 'create') {
    $state.release = [pscustomobject]@{ id=101; tag_name=$a[2]; target_commitish=(Value '--target'); name=(Value '--title');
        body=[IO.File]::ReadAllText((Value '--notes-file')); draft=$true; prerelease=$false; published_at=$null;
        html_url='https://github.com/fixture/repo/releases/tag/v1.2.3'; assets=@() }
}
elseif ($a[0] -eq 'release' -and $a[1] -eq 'upload') {
    $file = Get-Item -LiteralPath $a[3]
    if ($state.mode -eq 'fail-once' -and !$state.failedOnce) {
        $state.failedOnce=$true; $global:LASTEXITCODE=1; Save; Write-Error 'Fixture network interrupted' -ErrorAction Continue; return
    }
    $hash=(Get-FileHash -LiteralPath $file.FullName -Algorithm SHA256).Hash.ToLowerInvariant()
    if ($state.mode -eq 'bad-digest') { $hash = '0' * 64 }
    $state.release.assets = @($state.release.assets | Where-Object { $_.name -cne $file.Name }) +
        [pscustomobject]@{name=$file.Name; size=$file.Length; state='uploaded'; digest="sha256:$hash"}
}
elseif ($a[0] -eq 'release' -and $a[1] -eq 'edit') {
    $state.release.draft=$false; $state.release.published_at='2026-09-26T00:00:00Z'
    if ($state.mode -eq 'lost-edit-response') { $global:LASTEXITCODE=1; Save; Write-Error 'Fixture response lost' -ErrorAction Continue; return }
}
else { throw 'Unexpected fake GitHub command' }
Save
'@
    $ghState = Join-Path $root 'github-state.json'
    $env:MUTINY_TEST_GH_STATE = $ghState
    $notesPath = Join-Path $root 'notes.md'
    Write-TestFile $notesPath $manifest.notes
    $verified = Join-Path $root 'verified.json'
    function Reset-Gh([string]$Mode) { Write-ReleaseJson $ghState ([pscustomobject]@{mode=$Mode;failedOnce=$false;release=$null;events=@()}) }
    Reset-Gh 'fail-once'
    $url = Publish-ReleaseManifest $fakeGh $manifest $notesPath $verified
    $state = [IO.File]::ReadAllText($ghState) | ConvertFrom-Json
    Assert-Case (!$state.release.draft -and @($state.release.assets).Count -eq 4 -and $state.failedOnce -and $url -like 'https://*') 'Draft/upload/retry/hash verification/publication sequence'
    $editsBefore = @($state.events | Where-Object { $_ -ceq 'release edit' }).Count
    $null = Publish-ReleaseManifest $fakeGh $manifest $notesPath $verified -Resume
    $state = [IO.File]::ReadAllText($ghState) | ConvertFrom-Json
    Assert-Case (@($state.events | Where-Object { $_ -ceq 'release edit' }).Count -eq $editsBefore) 'Published resume verifies without changing release'
    Assert-Throws { Publish-ReleaseManifest $fakeGh $manifest $notesPath $verified } 'already published' 'Published release cannot be overwritten'

    Reset-Gh 'bad-digest'
    Assert-Throws { Publish-ReleaseManifest $fakeGh $manifest $notesPath $verified } 'verification failed' 'Remote SHA256 mismatch blocks publication'
    $state = [IO.File]::ReadAllText($ghState) | ConvertFrom-Json
    Assert-Case ($state.release.draft -and @($state.events | Where-Object { $_ -ceq 'release edit' }).Count -eq 0) 'Invalid upload remains draft'
    $state.mode = 'normal'
    Write-ReleaseJson $ghState $state
    $null = Publish-ReleaseManifest $fakeGh $manifest $notesPath $verified -Resume
    Assert-Case (!([IO.File]::ReadAllText($ghState) | ConvertFrom-Json).release.draft) 'Interrupted draft upload resumes successfully'

    Reset-Gh 'lost-edit-response'
    Assert-Throws { Publish-ReleaseManifest $fakeGh $manifest $notesPath $verified } 'failed' 'Lost final publish response is reported'
    $null = Publish-ReleaseManifest $fakeGh $manifest $notesPath $verified -Resume
    Assert-Case $true 'Lost final publish response is recoverable without rebuilding'

    $state = [IO.File]::ReadAllText($ghState) | ConvertFrom-Json
    $state.release.draft = $true
    $state.release.body = 'different notes'
    Write-ReleaseJson $ghState $state
    Assert-Throws { Publish-ReleaseManifest $fakeGh $manifest $notesPath $verified -Resume } 'identity or notes' 'Foreign draft identity is rejected'
    $state.release.body = $manifest.notes
    $state.release.assets += [pscustomobject]@{ name='unexpected-ios.zip';size=10;state='uploaded';digest='sha256:bad' }
    Write-ReleaseJson $ghState $state
    Assert-Throws { Publish-ReleaseManifest $fakeGh $manifest $notesPath $verified -Resume } 'unexpected asset' 'Extra platform assets block publication'
    Assert-Throws { Assert-ReleaseRemoteTag $git $fixture v1.2.3 ('0'*40) ('0'*40) } 'tags differ' 'Remote tag mismatch rejected'
    Assert-Throws { Assert-ReleaseRemoteTag $git $fixture v9.0.0 ('0'*40) ('0'*40) } 'Push tag' 'Missing remote tag rejected'
    Assert-ReleaseRemoteTag $git $fixture $manifest.tag $manifest.commit $manifest.tagObject
    Assert-Case $true 'Matching remote annotated tag accepted'

    Assert-Throws { Get-ReleaseSettings "bundleVersion: 1.0.1`nAndroidBundleVersionCode: 5" '1.0.2' } "does not match 1\.0\.2.*contains '1\.0\.1'" 'Version mismatch reports the tagged version and requested version'

    # Authentication fallback and cleanup are exercised through the actual publishing launcher.
    $shimDir = Join-Path $root 'Git Auth Shim'
    $null = New-Item -ItemType Directory -Path $shimDir -Force
    $shimSource = Join-Path $shimDir 'GitShim.cs'
    Write-TestFile $shimSource @'
using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
public static class GitShim {
    public static int Main(string[] a) {
        Console.OutputEncoding=new UTF8Encoding(false);
        if (a.Length>1 && a[0]=="credential" && a[1]=="fill") {
            Console.In.ReadToEnd();
            if (Environment.GetEnvironmentVariable("MUTINY_TEST_NO_CREDENTIAL")=="1") return 1;
            Console.WriteLine("protocol=https\nhost=github.com\nusername=fixture\npassword=fixture-cached-token");
            return 0;
        }
        if (a.Length>0 && a[0]=="ls-remote") {
            Console.WriteLine(Environment.GetEnvironmentVariable("MUTINY_TEST_TAG_OBJECT")+"\trefs/tags/v1.2.3");
            Console.WriteLine(Environment.GetEnvironmentVariable("MUTINY_TEST_COMMIT")+"\trefs/tags/v1.2.3^{}");
            return 0;
        }
        var start=new ProcessStartInfo {
            FileName=Environment.GetEnvironmentVariable("MUTINY_TEST_REAL_GIT"),
            Arguments=string.Join(" ",a.Select(v=>"\""+v.Replace("\"","\\\"")+"\"")),
            UseShellExecute=false, CreateNoWindow=true, RedirectStandardOutput=true, RedirectStandardError=true,
            StandardOutputEncoding=Encoding.UTF8, StandardErrorEncoding=Encoding.UTF8
        };
        using(var p=Process.Start(start)) {
            var stdout=p.StandardOutput.ReadToEndAsync(); var stderr=p.StandardError.ReadToEndAsync();
            p.WaitForExit(); Console.Write(stdout.Result); Console.Error.Write(stderr.Result); return p.ExitCode;
        }
    }
}
'@
    $shimGit = Join-Path $shimDir 'git.exe'
    $null = Invoke-ReleaseTool $csc @('/nologo', "/out:$shimGit", $shimSource)
    $env:MUTINY_TEST_REAL_GIT=$git
    $env:MUTINY_TEST_TAG_OBJECT=$manifest.tagObject
    $env:MUTINY_TEST_COMMIT=$manifest.commit
    $env:GH_TOKEN='fixture-original-token'
    Reset-Gh 'normal'
    $usedFallback=Initialize-ReleaseGitHubAuth $fakeGh $shimGit
    Assert-Case (!$usedFallback -and $env:GH_TOKEN -ceq 'fixture-original-token') 'Valid gh auth keeps original credentials'
    Reset-Gh 'auth-fallback'
    $env:PATH=$shimDir + [IO.Path]::PathSeparator + $previousPath
    & $launcher -Tag v1.2.3 -Resume -GhPath $fakeGh
    $state=[IO.File]::ReadAllText($ghState) | ConvertFrom-Json
    Assert-Case (!$state.release.draft -and $env:GH_TOKEN -ceq 'fixture-original-token') 'Cached Git auth publishes through launcher and restores original GH_TOKEN'
    Reset-Gh 'auth-fallback'
    $env:MUTINY_TEST_NO_CREDENTIAL='1'
    Assert-Throws { & $launcher -Tag v1.2.3 -Resume -GhPath $fakeGh } 'authentication is unavailable' 'Missing cached auth stops with login instructions'
    Assert-Case ($env:GH_TOKEN -ceq 'fixture-original-token') 'Authentication failure restores original GH_TOKEN'
    Remove-Item Env:\MUTINY_TEST_NO_CREDENTIAL
    $env:PATH=$previousPath
    Write-ReleaseJson (Join-Path $root 'results.json') ([pscustomobject]@{passed=$script:passed;date=[DateTime]::UtcNow.ToString('o');onlineWrites=$false})
    Write-Host "Release regression: $script:passed cases passed. Evidence: $root"
}
finally {
    $env:PATH=$previousPath
    [Environment]::SetEnvironmentVariable('GH_TOKEN',$previousGhToken,'Process')
    Remove-Item Env:\MUTINY_TEST_UNITY_MODE -ErrorAction SilentlyContinue
    Remove-Item Env:\MUTINY_TEST_GH_STATE -ErrorAction SilentlyContinue
    Remove-Item Env:\MUTINY_TEST_NO_CREDENTIAL,Env:\MUTINY_TEST_REAL_GIT,Env:\MUTINY_TEST_TAG_OBJECT,Env:\MUTINY_TEST_COMMIT -ErrorAction SilentlyContinue
}
