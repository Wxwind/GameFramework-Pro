cd /d %~dp0

set WORKSPACE=../..
set LUBAN_DLL=$WORKSPACE/Tools/Luban/Source/Luban.dll
set CONF_ROOT=$WORKSPACE/Excel
set DATA_OUTPATH=$CONF_ROOT/output/data
set CODE_OUTPATH=$CONF_ROOT/output/code

dotnet %LUBAN_DLL% ^
    -t all ^
    -c cs-simple-json ^
    -d json ^
    --conf %CONF_ROOT%\luban.conf ^
    -x outputDataDir=%DATA_OUTPATH% ^
    -x outputCodeDir=%CODE_OUTPATH%

pause
