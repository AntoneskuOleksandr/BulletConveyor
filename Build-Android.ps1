param(
    [Parameter(Mandatory = $true)]
    [string]$UnityPath,
    [string]$OutputPath = 'Builds\Android\BulletConveyor.apk',
    [ValidateRange(1, 16)]
    [int]$Jobs = 4
)

$ErrorActionPreference = 'Stop'
$projectPath = $PSScriptRoot
if (!(Test-Path -LiteralPath $UnityPath -PathType Leaf)) { throw "Unity editor not found: $UnityPath" }
if (Test-Path -LiteralPath (Join-Path $projectPath 'Temp\UnityLockfile')) {
    throw 'This checkout is open in Unity. Close it, or run the build from a separate checkout.'
}
if (![IO.Path]::IsPathRooted($OutputPath)) { $OutputPath = Join-Path $projectPath $OutputPath }
$OutputPath = [IO.Path]::GetFullPath($OutputPath)
$logPath = Join-Path $projectPath 'Logs\android-build.log'
New-Item -ItemType Directory -Force -Path ([IO.Path]::GetDirectoryName($logPath)) | Out-Null

$oldProcessorCount = $env:DOTNET_PROCESSOR_COUNT
$oldApkPath = $env:BULLET_APK_PATH
try {
    $env:DOTNET_PROCESSOR_COUNT = $Jobs.ToString()
    $env:BULLET_APK_PATH = $OutputPath
    $arguments = @('-batchmode', '-nographics', '-buildTarget', 'Android', '-projectPath', $projectPath,
        '-executeMethod', 'AndroidBuild.Apk', '-quit', '-logFile', $logPath, '-job-worker-count', $Jobs.ToString())
    $process = Start-Process -FilePath $UnityPath -ArgumentList ($arguments | ForEach-Object { '"' + $_ + '"' }) `
        -WindowStyle Hidden -PassThru -Wait
    if ($process.ExitCode -ne 0) { throw "Android build failed. See $logPath" }
    if (!(Test-Path -LiteralPath $OutputPath)) { throw 'Unity exited without producing the requested APK.' }
    Write-Output "APK ready: $OutputPath"
    Write-Output "Build log: $logPath"
}
finally {
    $env:DOTNET_PROCESSOR_COUNT = $oldProcessorCount
    $env:BULLET_APK_PATH = $oldApkPath
}
