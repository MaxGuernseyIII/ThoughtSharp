dotnet tool install -g dotnet-train

Get-ChildItem -Recurse -Filter *.sln | ForEach-Object {
    Write-Host "Checking $($_.FullName)"
    dotnet outdated $_.FullName --upgrade
}