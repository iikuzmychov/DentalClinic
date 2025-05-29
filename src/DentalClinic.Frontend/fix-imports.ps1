# Fix Kiota generated TypeScript imports by removing .js extensions
Get-ChildItem -Path 'src/app/api' -Filter '*.ts' -Recurse | ForEach-Object {
    $content = Get-Content $_.FullName
    $fixed = $content -replace '\.js([''"])', '$1'
    Set-Content -Path $_.FullName -Value $fixed
}
Write-Host "Fixed TypeScript imports in API files" 