@echo off

set NETFOLDER=
For /D %%D in ("C:\Windows\Microsoft.NET\Framework64\v*") do (
   set NETFOLDER=%%~fD
)

if exist "%NETFOLDER%\csc.exe" (
	"%NETFOLDER%/csc.exe" /nologo ExeProxy.cs IniFile.cs
	"%NETFOLDER%/csc.exe" /nologo MultiPHP.cs IniFile.cs

	echo Compilation ended
) else (
	echo Compiler not found
)

@pause
