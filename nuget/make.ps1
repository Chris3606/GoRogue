# Change working directory to GoRogue project dir
pushd $PSScriptRoot\..\GoRogue

# Compile release build
msbuild /t:pack /p:Configuration=Release


# Backup and load csproj file
$fullCsProjPath = Join-Path $(Get-Location) "GoRogue.csproj"
$fullCsprojBackupPath = Join-Path $(Get-Location) "GoRogue.csproj.bak"
Copy-Item "$fullCsProjPath" -Destination "$fullCsprojBackupPath"

[xml]$xml = Get-Content "$fullCsProjPath"
$node = $(Select-Xml -Xml $xml -XPath '//Project/PropertyGroup/PackageVersion').Node

# Add "-debug" to end of version tag
$oldVersion = $node.'#text'
$node.'#text' = $oldVersion + "-debug"
$xml.Save($fullCsProjPath)

# Compile debug build
msbuild /t:pack /p:Configuration=Debug

# Revert to backup of csproj
Remove-Item "$fullCsProjPath"
Copy-Item "$fullCsprojBackupPath" -Destination "$fullCsProjPath"

# Revert working directory to previous
popd