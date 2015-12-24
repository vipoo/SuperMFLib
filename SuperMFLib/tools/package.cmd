SET SolutionDir=%~dp0..\..\
SET ProjectDir=%~dp0..\

msbuild %ProjectDir%SuperMFLib.csproj -p:SolutionDir=%SolutionDir% -t:rebuild -p:ApplicationVersion=%APPVEYOR_BUILD_VERSION% -v:minimal /p:Configuration=Release

copy %ProjectDir%bin\x64\Release\MediaFoundation.Net.dll %ProjectDir%bin\x64\Release\MediaFoundation.Net.unmerged.dll 

%SolutionDir%packages\ILMerge.2.14.1208\tools\ILMerge.exe /targetplatform:v2,C:\Windows\Microsoft.NET\Framework\v2.0.50727 /out:%ProjectDir%bin\x64\Release\MediaFoundation.Net.dll %ProjectDir%bin\x64\Release\MediaFoundation.Net.unmerged.dll %ProjectDir%bin\x64\Release\MediaFoundation.dll

%ProjectDir%tools\nuget.exe pack %ProjectDir%SuperMFLib.nuspec -Prop Configuration=release -Verbosity detail -Version %APPVEYOR_BUILD_VERSION%