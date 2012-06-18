Import('env')

def CliLibraryLinn(target, source, **kw):
    import os
    library = env._CliLibrary(target, source, **kw)
    hack = env.Install('$install_dir/bin/' + os.path.split(target)[0], library)
    hack += env.Install('$install_dir/bin/Test/' + os.path.split(target)[0], library)
    install = env.Install('$install_dir/lib/Linn/' + os.path.split(target)[0], library)    
    env.Depends(install, hack)

    if(env['variant'] == 'debug'):
        if env['hardware'] != 'PocketPc':
            install_dbg = env.Install('$install_dir/lib/' + os.path.split(target)[0], library[0].abspath + '.mdb')
            install_dbg += env.Install('$install_dir/bin/' + os.path.split(target)[0], library[0].abspath + '.mdb')
            install_dbg += env.Install('$install_dir/bin/Test/' + os.path.split(target)[0], library[0].abspath + '.mdb')
        else:
            install_dbg = env.Install('$install_dir/lib/' + os.path.split(target)[0], library[0].abspath.replace('dll', 'pdb'))
            install_dbg += env.Install('$install_dir/bin/' + os.path.split(target)[0], library[0].abspath.replace('dll', 'pdb'))
            install_dbg += env.Install('$install_dir/bin/Test/' + os.path.split(target)[0], library[0].abspath.replace('dll', 'pdb'))
        env.Depends(install, install_dbg) 
        
    env.Alias(os.path.split(target)[1], install)
    return install

def CliProgramLinn(target, source, **kw):
    import os
    program = env._CliProgram(target, source, **kw)
    install = env.Install('$install_dir/bin/' + os.path.split(target)[0], program)
    #config file is not needed and is giving me issues
    #config = env.CliProgramConfig(target, program)
    #install += env.Install('$install_dir/bin/' + os.path.split(target)[0], config)
    if(env['variant'] == 'debug'):
        if env['hardware'] != 'PocketPc':
            install_dbg = env.Install('$install_dir/bin/' + os.path.split(target)[0], program[0].abspath + '.mdb')
        else:
            install_dbg = env.Install('$install_dir/bin', program[0].abspath.replace('exe', 'pdb'))
        env.Depends(install, install_dbg)
    env.Alias(os.path.split(target)[1], install)
    return install
  
def MSBuildExeBuilderLinn(target, source, **kw):
    import os
    program = env._MSBuildExeBuilder(target, source, **kw)
    install = env.Install('$install_dir/bin/' + os.path.split(target)[0], program)
    if(env['variant'] == 'debug'):
        install_dbg = env.Install('$install_dir/bin', program[0].abspath.replace('exe', 'pdb'))
        env.Depends(install, install_dbg)
    env.Alias(os.path.split(target)[1], install)
    return install  

def MSBuildLibBuilderLinn(target, source, **kw):
    import os
    library = env._MSBuildLibBuilder(target, source, **kw)
    hack = env.Install('$install_dir/bin/' + os.path.split(target)[0], library)
    hack += env.Install('$install_dir/bin/Test/' + os.path.split(target)[0], library)
    install = env.Install('$install_dir/lib/Linn/' + os.path.split(target)[0], library)    
    env.Depends(install, hack)

    if(env['variant'] == 'debug'):
        install_dbg = env.Install('$install_dir/lib/' + os.path.split(target)[0], library[0].abspath.replace('dll', 'pdb'))
        install_dbg += env.Install('$install_dir/bin/' + os.path.split(target)[0], library[0].abspath.replace('dll', 'pdb'))
        install_dbg += env.Install('$install_dir/bin/Test/' + os.path.split(target)[0], library[0].abspath.replace('dll', 'pdb'))
        env.Depends(install, install_dbg) 
        
    env.Alias(os.path.split(target)[1], install)
    return install
  
def MSBuildApkBuilderLinn(target, source, **kw):
    import os
    program = env._MSBuildApkBuilder(target, source, **kw)
    programSigned = program[0].abspath.replace(".apk", "-Signed.apk")
    install = env.Install('$install_dir/bin/' + os.path.split(target)[0], program)
    install += env.Install('$install_dir/bin/' + os.path.split(target)[0], programSigned)
    env.Alias(os.path.split(target)[1], install)
    return install  
    
try:
    # Tag original builder with a _
    # Override original builder with the new Linn added functionality
    env._CliLibrary = env.CliLibrary
    env.CliLibrary = CliLibraryLinn
    env._CliProgram = env.CliProgram
    env.CliProgram = CliProgramLinn
    if "MSBuildExeBuilder" in env["BUILDERS"]:
        env._MSBuildExeBuilder = env.MSBuildExeBuilder
        env.MSBuildExeBuilder = MSBuildExeBuilderLinn
        env._MSBuildLibBuilder = env.MSBuildLibBuilder
        env.MSBuildLibBuilder = MSBuildLibBuilderLinn
        env._MSBuildApkBuilder = env.MSBuildApkBuilder
        env.MSBuildApkBuilder = MSBuildApkBuilderLinn

    # Set various environment variables up
    env.AppendUnique(CLILIBPATH = ['$install_dir/lib/Linn'])
    env.AppendUnique(CLILIBPATH = ['$install_dir/lib'])
except AttributeError, e:
    print e

Return('env')
