# Configuration
$scriptDir = $PSScriptRoot
$rootDir = Join-Path $scriptDir ".."
$archive = Join-Path $scriptDir archive
$corePackage = "GoRogue"
$nugetKeyPath = Join-Path $scriptDir "nuget.key"

# Delete any NuGet packages not moved to archive
Write-Output "Removing old nupkg files"
Remove-Item "$scriptDir\*.nupkg","$scriptDir\*.snupkg" -Force

# Make sure archive directory exists
if(!(Test-Path $archive))
{
    New-Item -Path $archive -ItemType Directory -Force | Out-Null
}

# Build core package
Write-Output "Building $corePackage Debug and Release"
$output = Invoke-Expression "dotnet build $rootDir\$corePackage\$corePackage.csproj -c Debug --no-cache"; if ($LASTEXITCODE -ne 0) { Write-Error "Failed"; Write-Output $output; throw }
$output = Invoke-Expression "dotnet build $rootDir\$corePackage\$corePackage.csproj -c Release --no-cache"; if ($LASTEXITCODE -ne 0) { Write-Error "Failed"; Write-Output $output; throw }

# Find the version we're using
$version = (Get-Content $rootDir\$corePackage\$corePackage.csproj | Select-String "<Version>(.*)<").Matches[0].Groups[1].Value
$nugetKey = Get-Content $nugetKeyPath
Write-Output "Target $coreProjectVersion version is $version"

# Push packages to nuget
Write-Output "Pushing $corePackage packages"
$corePackages = Get-ChildItem "$corePackage.*.nupkg" | Select-Object -ExpandProperty Name

foreach ($package in $corePackages) {
    $output = Invoke-Expression "dotnet nuget push `"$package`" -s nuget.org -k $nugetKey --skip-duplicate"; if ($LASTEXITCODE -ne 0) { Write-Error "Failed"; Write-Output $output; throw }
}

Write-Output "Query NuGet for 10 minutes to find the new package"

$timeout = New-TimeSpan -Minutes 10
$timer = [Diagnostics.StopWatch]::StartNew()
[Boolean]$foundPackage = $false

# Loop searching for the new package
while ($timer.elapsed -lt $timeout){

    $existingVersions = (Invoke-WebRequest "https://api-v2v3search-0.nuget.org/query?q=PackageId:$corePackage&prerelease=true").Content | ConvertFrom-Json

    if ($existingVersions.totalHits -eq 0) {
        throw "Unable to get any results from NuGet"
    }

    if ($null -eq ($existingVersions.data.versions | Where-Object version -eq $version)) {
        Write-Output "Waiting 30 seconds to retry..."
        Start-Sleep -Seconds 30
		
    }
    else {
        Write-Output "Found package!  Deployment successful."
        $foundPackage = $true
        break
    }
}

if (!$foundPackage) {
    Write-Error "$corePackage didn't appear on NuGet within 10 minutes."
}