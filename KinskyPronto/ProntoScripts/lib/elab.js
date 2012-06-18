////
//
// elab.js
//
// Elaboration code
//
// uses: diag
//
// TODO: Need some way to add units which should have ._name, ._initialise & ._start attributes / routines.
//
// Common Code Version.
//
////

var elab = {};

elab.stack = [];
elab.failed = false;
elab.settings = {};


elab.add = function (name, init, start) {

  if ((init == null) && (start == null)) {

    TIMESTAMP("Loaded:"+name);
    return;
  };

  var mod = {_name: name, _init: init, _start:start}

  TIMESTAMP("Loading:"+name);
  elab.stack.push(mod);

};


elab.doInitialise = function() {

  TIMESTAMP("Elaborating" + buildTimestamp);

  for (var i = 0; i < elab.stack.length; i++){
    if (elab.stack[i]._init != null) {

      try {

	TIMESTAMP("Elaborating "+elab.stack[i]._name);
        elab.stack[i]._init();
        TIMESTAMP("Done");

      } catch (e) {
        elab.failed = true;
        TIMESTAMP("Error "+e+" elaborating "+elab.stack[i]._name);
	diag.fatal("Error "+e+" elaborating "+elab.stack[i]._name);
	return;
      };
     
    };
  };
  TIMESTAMP("Elaboration complete");
}

elab.doStart = function() {

  if (elab.failed) {
    TIMESTAMP("Startup cancelled.")
    return;
  };

  for (var i = 0; i < elab.stack.length; i++){
    if (elab.stack[i]._start != null) {

      try {

        TIMESTAMP("Starting "+elab.stack[i]._name);
        elab.stack[i]._start(elab.settings);
        TIMESTAMP("Done");

      } catch (e) {
        elab.failed = true;
        TIMESTAMP("Error "+e+" starting "+elab.stack[i]._name);
	diag.fatal("Error "+e+" starting "+elab.stack[i]._name);
	return;
      };
    };
  };

  TIMESTAMP("Startup complete");

};


elab.go = function() {

  GUI.updateScreen();
  elab.doInitialise();
  elab.doStart();
};

elab.add("elab");
