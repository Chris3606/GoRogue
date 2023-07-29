param (
    [Parameter(Mandatory = $true)]
    [string]$PackageName,
	
    [Parameter(Mandatory = $true)]
    [string]$PackageVersion,

    [Parameter(Mandatory = $true)]
    [string]$OutputFolder
)

$targets = @("netstandard2.1", "netstandard2.0", "netstandard1.0")

# Create a temporary directory we can use for the output
$tempDir = New-Item -ItemType Directory -Path "$($ENV:Temp)\nuget_output_temp"
$tempPath = $tempDir.FullName

try {
    # Turn temp folder into a dotnet package so we can use NuGet
    Write-Output "Creating temporary class library to use for NuGet package install..."
    dotnet new classlib -f netstandard2.1 -o $tempPath | Out-Null

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
        foreach ($target in $targets) {
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