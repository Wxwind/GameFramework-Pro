#!/bin/bash

CURRENT_DIR=$(
    cd $(dirname $0)
    pwd
)
cd ${CURRENT_DIR}

WORKSPACE=../..
LUBAN_DLL=$WORKSPACE/Tools/Luban/Source/Luban.dll
CONF_ROOT=$WORKSPACE/Config/Excel/Game
L10N_FILE=$WORKSPACE/Config/Excel/Localization.xlsx
UNITY_FOLDER=$WORKSPACE/Unity
DATA_OUTPATH=$UNITY_FOLDER/Assets/GameMain/LubanConfig
CODE_OUTPATH=$UNITY_FOLDER/Assets/GameMain/Scripts/LubanConfig

dotnet $LUBAN_DLL \
    -t all \
    -c cs-simple-json \
    -d json \
    --conf $CONF_ROOT/luban.conf \
    -x outputDataDir=$DATA_OUTPATH \
    -x outputCodeDir=$CODE_OUTPATH \
    -x l10n.provider=default \
    -x l10n.textFile.path=$L10N_FILE \
    -x l10n.textFile.keyFieldName=key
