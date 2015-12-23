SET SolutionDir=%~dp0..\..\
SET ProjectDir=%~dp0..\

msbuild %ProjectDir%SuperMFLib.csproj -p:SolutionDir=%SolutionDir% -t:rebuild -p:ApplicationVersion=%APPVEYOR_BUILD_VERSION% -v:minimal /p:Configuration=Release

%ProjectDir%tools\nuget.exe pack %ProjectDir%SuperMFLib.nuspec -Prop Configuration=release -Verbosity detail -Version %APPVEYOR_BUILD_VERSION%