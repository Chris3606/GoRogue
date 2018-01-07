pushd %~dp0..\GoRogue
msbuild /t:pack /p:Configuration=Release
popd