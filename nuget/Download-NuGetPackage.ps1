<#
.SYNOPSIS
    Downloads the specified package from NuGet, along with all of its dependencies, then copies
    the DLL and XML files for that package and all of its dependencies to the specified output folder.

.DESCRIPTION
    The purpose of this script is to use NuGet to resolve a package and its dependencies, and take
    the DLLs and XML files for that package and its dependencies, and copy them to an output directory.

.PARAMETER PackageName
The name of the NuGet package to download.

.PARAMETER PackageVersion
The version of the NuGet package to download.

.PARAMETER OutputFolder
The folder where the DLLs and XML documentation files will be placed.

.PARAMETER Targets
    Targets to use to resolve packages (in order).  The first target for which each package has a DLL will be used.

.EXAMPLE
.\Download-NuGetPackage.ps1 -PackageName "Newtonsoft.Json" -PackageVersion "13.0.1" -OutputFolder "C:\Temp\Newtonsoft.Json" -Targets netstandard2.1,netstandard2.0

.NOTES
This script requires the dotnet CLI to be installed.  Additionally, it will only work for packages
which support .NET Standard 2.1.
#>
param (
    [Parameter(Mandatory = $true)]
    [string]$PackageName,
	
    [Parameter(Mandatory = $true)]
    [string]$PackageVersion,

    [Parameter(Mandatory = $true)]
    [string]$OutputFolder,

    [Parameter(Mandatory = $true)]
    [string[]]$Targets
)

# Create a temporary directory we can use for the output
$tempDir = New-Item -ItemType Directory -Path "$($ENV:Temp)\nuget_output_temp"
$tempPath = $tempDir.FullName

try {
    # Turn temp folder into a dotnet package so we can use NuGet
    Write-Output "Creating temporary class library to use for NuGet package install..."
    dotnet new classlib -f $netstandard2.1 -o $tempPath | Out-Null

    # Get package and all its dependencies and place them in the temp directory
    Write-Output "Installing NuGet package and all dependencies..."
    dotnet add $tempPath package $PackageName --version $PackageVersion --package-directory $tempPath\Deps | Out-Null

    # Go through the package format, and extract the DLLs and XML files from each package.
    $packageDirs = Get-ChildItem "$tempPath\Deps\" -Directory
    foreach ($packageDir in $packageDirs) {
        $path = Join-Path $packageDir.FullName "*.dll"
        $dlls = Get-ChildItem -Recurse -File $path
        
        # Find one that matches the target chain and copy it
        $foundTarget = $false
        foreach ($target in $Targets) {
            foreach ($dll in $dlls) {     
                if (-not ($dll.FullName.Contains($target))) { continue }

                Write-Output "Found $target DLL for $($packageDir.Name)."
                $foundTarget = $true

                $destinationPath = Join-Path $OutputFolder $dll.Name
                Copy-Item -Path $dll.FullName -Destination $destinationPath -Force

                # Grab the corresponding XML documentation file as well
                $xmlName = $dll.BaseName + ".xml"
                $xmlPath = Join-Path $dll.Directory.FullName $xmlName
                $destinationPath = Join-Path $OutputFolder $xmlName
                Copy-Item -Path $xmlPath -Destination $destinationPath -Force
                    
                break
            }

            if ($foundTarget) { break }
        }

        if (-not $foundTarget) {
            Write-Error "ERROR: Couldn't find target for dependency $($packageDir.Name).  Available paths were: $dlls"
        }
    }
}
finally {
    # Remove directory we used for build
    Remove-Item -Path $tempDir -Recurse -Force
}