param(
    [string]$StartPath = "../",
    [string]$OutputFile = "results.txt"
)

if (Test-Path $OutputFile) {
    Remove-Item $OutputFile
}

$csFiles = Get-ChildItem -Path $StartPath -Filter "*.cs" -Recurse -File | Where-Object {
    $_.DirectoryName -notmatch "\\bin\\|\\obj\\|\\bin$|\\obj$"
}

Write-Host "Found $($csFiles.Count) .cs files"

New-Item -ItemType File -Path $OutputFile -Force | Out-Null

foreach ($file in $csFiles) {
    $relativePath = Resolve-Path -Path $file.FullName -Relative
    
    Write-Host "Processing: $relativePath"
    
    Add-Content -Path $OutputFile -Value $relativePath -Encoding UTF8
    Add-Content -Path $OutputFile -Value "" -Encoding UTF8
    
    $content = Get-Content -Path $file.FullName -Raw -Encoding UTF8
    Add-Content -Path $OutputFile -Value $content -Encoding UTF8
    Add-Content -Path $OutputFile -Value "" -Encoding UTF8
}

Write-Host "Done! Results saved to $OutputFile"