pushd %~dp0
nuget pack ../GoRogue/GoRogue.csproj -properties Configuration=Release
popd