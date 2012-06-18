#!/bin/bash

set -e #exit immediately on a build error

mono --debug Test/TestGuiLayout.exe
#mono --debug Test/TestNodeList.exe
mono --debug Test/TestPackage.exe
mono --debug Test/TestPackageManager.exe
mono --debug Test/TestSceneGraph.exe
mono --debug Test/TestSceneGraphEvent.exe
mono --debug Test/TestSrt.exe
mono --debug Test/TestTextureManager.exe
