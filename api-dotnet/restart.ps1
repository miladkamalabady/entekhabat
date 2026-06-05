# restart.ps1 - Build و Restart سرور
# اجرا: .\restart.ps1

Write-Host "Stopping server..." -ForegroundColor Yellow
Get-Process | Where-Object { $_.ProcessName -match "dotnet|EntekhabatApi" } | ForEach-Object { $_.Kill() }
Start-Sleep 3

Write-Host "Building..." -ForegroundColor Cyan
$build = & dotnet build -q 2>&1
if ($LASTEXITCODE -ne 0) {
    # اگر EXE هنوز قفل بود، DLL را دستی کپی کن
    Write-Host "Copying DLL manually..." -ForegroundColor Yellow
    $src = ".\obj\Debug\net8.0"
    $dst = ".\bin\Debug\net8.0"
    Copy-Item "$src\EntekhabatApi.dll" $dst -Force -ErrorAction SilentlyContinue
    Copy-Item "$src\EntekhabatApi.pdb" $dst -Force -ErrorAction SilentlyContinue
    Copy-Item "$src\apphost.exe" "$dst\EntekhabatApi.exe" -Force -ErrorAction SilentlyContinue
}

Write-Host "Starting server on http://localhost:5050 ..." -ForegroundColor Green
dotnet run --no-build
