#Kinsky Pronto Build Procedure

##Goals
To be able to build new version(s) of `Linn-DS.xcf` & `Linn DS.xgf` given
the existing versions of these two file and updated JavaScript files. These two files 
are the Kinsky Pronto release file (`.xgf`) & a demonstration system file (`.xcf`).

##None Goals
To be able to build Kinsky Pronto *from scratch*.

##Build Procedure

1. Make whatever changes are deemed necessary to the JavaScript source files. Be sure to modify
the source JavaScript files not the *build files* `DS.js` & `ds.main.js`.

2. If necessary edit `buildVersion.js` to change the release number.

3. Run `00-make-xp.cmd` which will execute a number of Python scripts to create a timestamp file &
include the appropiate JavaScript files together to create `DS.js`. `DS.js` will also be copied into
the *include* directory of ProntoEdit Pro.

4. Open `Linn-DS.xcf` with ProntoEditPro.

5. Select `Linn DS` from the left hand folder view (Project Overview).

6. In the right hand folder (Activity Properties) select the `Advanced` tab. This should show the ProntoScript `DS`
with previous Javascript in the `Activity Script` box.

7. Select the `Reload From File` option. The `Activity Script` should change to be the new `DS.js`
built in step 3 - this can be verified as the build timestamp and version will be show.

8. Download to a Pronto & test.

9. Assuming that you are happy & wish to make a release do the following.
Save the project as `Linn-DS.xcf` and copy to the Subversion working folder.

10. Right click on `Linn DS` from the left hand folder view (Project Overview), select `Save to ProntoScript Modules`.
You may be prompted to supply a name if an existing script alreay exists. This will create a `.xgf` file in
the `C:\Documents and Settings\All Users\Application Data\Philips\ProntoEdit Professional 2\ProntoScriptModules\Categories\NewCategory`.
Rename the file to `Linn DS.xgf` & copy it to the Subversion working folder.

11. Job done.
