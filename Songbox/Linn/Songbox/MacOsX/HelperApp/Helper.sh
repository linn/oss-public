#!/bin/sh


APP_PATH=`echo "$0" | awk -F"/" '{ for(i = 1; i <= NF - 7; i++) { printf("%s/", $i); } }'`
open $APP_PATH


