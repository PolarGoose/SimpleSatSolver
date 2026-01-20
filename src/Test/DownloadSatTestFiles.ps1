param([Parameter(Mandatory = $true)] [string] $BuildDir,
      [Parameter(Mandatory = $true)] [string] $OutputDir)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"
$ProgressPreference = "SilentlyContinue"

$BuildDir = Resolve-Path $BuildDir
$OutputDir = Resolve-Path $OutputDir

function DownloadFile($Uri, $OutFile) {
    if (Test-Path $OutFile) {
        return
    }
    Write-Host "Download '$Uri' -> '$OutFile'"
    Invoke-WebRequest -Uri $Uri -OutFile $OutFile
}

function DownloadAndUnpackSatFiles($Uri, $OutFile, $OutDir) {
    DownloadFile -Uri $Uri -OutFile $OutFile
    New-Item -ItemType Directory `
             -Path $OutDir `
             -Force > $null
    tar -xzf $OutFile -C $OutDir
}

# More test files can be found at https://www.cs.ubc.ca/~hoos/SATLIB/benchm.html

DownloadAndUnpackSatFiles -Uri https://www.cs.ubc.ca/~hoos/SATLIB/Benchmarks/SAT/RND3SAT/uf50-218.tar.gz `
                          -OutFile $BuildDir/uf50-218.tar.gz `
                          -OutDir $OutputDir/test_files/sat
DownloadAndUnpackSatFiles -Uri https://www.cs.ubc.ca/~hoos/SATLIB/Benchmarks/SAT/RND3SAT/uuf50-218.tar.gz `
                          -OutFile $BuildDir/uuf50-218.tar.gz `
                          -OutDir $OutputDir/test_files/unsat

DownloadAndUnpackSatFiles -Uri https://www.cs.ubc.ca/~hoos/SATLIB/Benchmarks/SAT/RND3SAT/uf100-430.tar.gz `
                          -OutFile $BuildDir/uf100-430.tar.gz `
                          -OutDir $OutputDir/test_files/sat
DownloadAndUnpackSatFiles -Uri https://www.cs.ubc.ca/~hoos/SATLIB/Benchmarks/SAT/RND3SAT/uuf100-430.tar.gz `
                          -OutFile $BuildDir/uuf100-430.tar.gz `
                          -OutDir $OutputDir/test_files/unsat
