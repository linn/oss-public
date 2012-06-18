# NSIS installer script for KinskyPronto
#
# NSIS installer to install XCF, XGF, Lib and ProntoScripts in the default
# locations on the destination PC.


name "Kinsky Pronto"
outfile "build\KinskyPronto.exe"


!define XCF "$DOCUMENTS\Pronto Projects"
!define XGF "$APPDATA\Philips\ProntoEdit Professional 2\ProntoScriptModules\Categories\NewCategory"
!define LIB "$APPDATA\Philips\ProntoEdit Professional 2\Libraries"
!define PS  "$APPDATA\Philips\ProntoEdit Professional 2\ProntoScripts"


section

    setShellVarContext current
    setOutPath "${XCF}\TSU9400"
    file "..\Binaries\KinskyPronto9400.xcf"
    setOutPath "${XCF}\TSU9600"
    file "..\Binaries\KinskyPronto9600.xcf"
    
    setShellVarContext all
    setOutPath "${XGF}"
    file "C:\Documents and Settings\All Users\Application Data\Philips\ProntoEdit Professional 2\ProntoScriptModules\Categories\NewCategory\Linn DS-9400-DS.xgf"
    file "C:\Documents and Settings\All Users\Application Data\Philips\ProntoEdit Professional 2\ProntoScriptModules\Categories\NewCategory\Linn DS-9400-PREAMP.xgf"
    file "C:\Documents and Settings\All Users\Application Data\Philips\ProntoEdit Professional 2\ProntoScriptModules\Categories\NewCategory\Linn DS-9400-RADIO.xgf"
    file "C:\Documents and Settings\All Users\Application Data\Philips\ProntoEdit Professional 2\ProntoScriptModules\Categories\NewCategory\Linn DS-9600-DS.xgf"
    file "C:\Documents and Settings\All Users\Application Data\Philips\ProntoEdit Professional 2\ProntoScriptModules\Categories\NewCategory\Linn DS-9600-PREAMP.xgf"
    file "C:\Documents and Settings\All Users\Application Data\Philips\ProntoEdit Professional 2\ProntoScriptModules\Categories\NewCategory\Linn DS-9600-RADIO.xgf"

    setOutPath "${LIB}"
    file "C:\Documents and Settings\All Users\Application Data\Philips\ProntoEdit Professional 2\Libraries\linnControl.js"
    
    setOutPath "${PS}"
    file "C:\Documents and Settings\All Users\Application Data\Philips\ProntoEdit Professional 2\ProntoScripts\DS_9400_DS.js"
    file "C:\Documents and Settings\All Users\Application Data\Philips\ProntoEdit Professional 2\ProntoScripts\DS_9400_PREAMP.js"
    file "C:\Documents and Settings\All Users\Application Data\Philips\ProntoEdit Professional 2\ProntoScripts\DS_9400_RADIO.js"
    file "C:\Documents and Settings\All Users\Application Data\Philips\ProntoEdit Professional 2\ProntoScripts\DS_9600_DS.js"
    file "C:\Documents and Settings\All Users\Application Data\Philips\ProntoEdit Professional 2\ProntoScripts\DS_9600_PREAMP.js"
    file "C:\Documents and Settings\All Users\Application Data\Philips\ProntoEdit Professional 2\ProntoScripts\DS_9600_RADIO.js"
    
sectionEnd
