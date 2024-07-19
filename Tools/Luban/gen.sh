#!/bin/bash

CURRENT_DIR=$(
    cd $(dirname $0)
    pwd
)
cd ${CURRENT_DIR}

WORKSPACE=../..
LUBAN_DLL=$WORKSPACE/Tools/Luban/Source/Luban.dll
CONF_ROOT=$WORKSPACE/Excel
DATA_OUTPATH=$CONF_ROOT/output/data
CODE_OUTPATH=$CONF_ROOT/output/code

dotnet $LUBAN_DLL \
    -t all \
    -c cs-simple-json \
    -d json \
    --conf $CONF_ROOT/luban.conf \
    -x outputDataDir=$DATA_OUTPATH \
    -x outputCodeDir=$CODE_OUTPATH
