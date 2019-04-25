# Change working directory to GoRogue project dir
pushd $PSScriptRoot\..\GoRogue

# Compile release build
msbuild /t:pack /p:Configuration=Release

# Load csproj file
$fullCsProjPath = Join-Path $(Get-Location) "GoRogue.csproj"
[xml]$xml = Get-Content "$fullCsProjPath"
$node = $(Select-Xml -Xml $xml -XPath '//Project/PropertyGroup/PackageVersion').Node

# Add "-debug" to end of version tag
$oldVersion = $node.'#text'
$node.'#text' = $oldVersion + "-debug"
$xml.Save($fullCsProjPath)

# Compile debug build
msbuild /t:pack /p:Configuration=Debug

# Revert version tag to release build
$node.'#text' = $oldVersion
$xml.Save($fullCsProjPath)

# Revert working directory to previous
popd