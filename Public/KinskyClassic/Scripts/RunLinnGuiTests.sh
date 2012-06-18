#!/bin/bash

set -e #exit immediately on a build error

mono --debug Test/TestXmlParsing.exe
mono --debug Test/TestTime.exe
mono --debug Test/TestKeyBindings.exe
mono --debug Test/TestAvTransportEventParser.exe
mono --debug Test/TestModelRoom.exe