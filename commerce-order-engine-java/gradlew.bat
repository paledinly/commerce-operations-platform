@echo off
setlocal
set "PROJECT=%~dp0"
docker run --rm -v "%PROJECT%:/workspace" -w /workspace gradle:8.14-jdk21 gradle --no-daemon %*
exit /b %ERRORLEVEL%

