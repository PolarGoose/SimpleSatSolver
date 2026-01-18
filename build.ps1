Function Info($msg) {
    Write-Host -ForegroundColor DarkGreen "`nINFO: $msg`n"
}

Function Error($msg) {
    Write-Host `n`n
    Write-Error $msg
    exit 1
}

Function CheckReturnCodeOfPreviousCommand($msg) {
    if(-Not $?) {
        Error "${msg}. Error code: $LastExitCode"
    }
}

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"
$ProgressPreference = "SilentlyContinue"

$root = Resolve-Path "$PSScriptRoot"

Info "Build project"
dotnet build `
  --nologo `
  --configuration Release `
  -verbosity:minimal `
  /property:DebugType=None `
  $root/SimpleSatSolver.slnx
CheckReturnCodeOfPreviousCommand "build failed"

Info "Run tests"
dotnet test `
  --nologo `
  --no-build `
  --configuration Release `
  -verbosity:minimal `
  --logger:"console;verbosity=normal" `
  $root/SimpleSatSolver.slnx
CheckReturnCodeOfPreviousCommand "tests failed"
