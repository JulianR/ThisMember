mkdir lib > nul

copy /y ..\ThisMember.Core\bin\Release\ThisMember.Core.dll .\lib\

nuget pack
