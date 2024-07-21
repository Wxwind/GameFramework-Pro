cd /d %~dp0

set WORKSPACE=../..
set LUBAN_DLL=%WORKSPACE%/Tools/Luban/Source/Luban.dll
set CONF_ROOT=%WORKSPACE%/Excel
set UNITY_FOLDER=%WORKSPACE%/Unity
set DATA_OUTPATH=%UNITY_FOLDER%/Assets/GameMain/LubanConfig
set CODE_OUTPATH=%UNITY_FOLDER%/Assets/GameMain/Scripts/LubanConfig


dotnet %LUBAN_DLL% ^
    -t all ^
    -c cs-simple-json ^
    -d json ^
    --conf %CONF_ROOT%/luban.conf ^
    -x outputDataDir=%DATA_OUTPATH% ^
    -x outputCodeDir=%CODE_OUTPATH%

pause
