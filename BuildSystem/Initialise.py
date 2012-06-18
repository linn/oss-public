import os.path
import sys
import os
import glob

try:
    svn_version = os.popen('svnversion %s' % GetLaunchDir()).read()[:-1]
    svn_version = svn_version.split(':')[-1]
except:
    svn_version = ""
if svn_version == "exported" or svn_version == "":
    svn_version = "0"

possibleTargets = [x for x in os.listdir('./Hardware') if x not in ['Bsp', '.svn']]

opts = Variables('./config.py')
opts.Add(EnumVariable('variant', '<HELP variant>', 'release', allowed_values = ('debug', 'trace', 'release', '')))
opts.Add(EnumVariable('hardware', 'hardware target against which to build', 'Windows', allowed_values = possibleTargets))
opts.Add(BoolVariable('installers', '<HELP installers>', 1))
opts.Add(PathVariable('build_dir', '<HELP build_dir>', '../build', PathVariable.PathIsDirCreate))
opts.Add(PathVariable('install_dir', '<HELP install_dir>', '../install', PathVariable.PathIsDirCreate))
opts.Add('svn_rev', '<HELP svn_rev>', svn_version)

_default_env = Environment(options = opts)
_tool_env = Environment(options = opts)

#add envrionment path variable
_default_env['ENV']['PATH'] = os.environ['PATH']
_tool_env['ENV']['PATH'] = os.environ['PATH']

# Generate help options
Help(opts.GenerateHelpText(_default_env))

# General tools
_default_env.Tool('makensis', toolpath=['../BuildSystem/Tool'])
_default_env.Tool('dpkg-deb', toolpath=['../BuildSystem/Tool'])
_default_env.Tool('cabwiz', toolpath=['../BuildSystem/Tool'])
_default_env.Tool('kodegen', toolpath=['../BuildSystem/Tool'])
_default_env.Tool('ibtool', toolpath=['../BuildSystem/Tool'])
_default_env.Tool('doxygen', toolpath=['../BuildSystem/Tool'])

_tool_env.Tool('kodegen', toolpath=['../BuildSystem/Tool'])

_default_env['hardware'] = os.path.basename(_default_env.subst('$hardware'))
_default_env['sconsign_dir'] = os.path.realpath(_default_env.subst('$build_dir'))
_default_env['build_hardware_dir'] = os.path.realpath(_default_env.subst('$build_dir'))
_default_env['build_variant_dir'] = os.path.realpath(_default_env.subst('$build_dir/$hardware'))
_default_env['build_dir'] = os.path.realpath(_default_env.subst('$build_dir/$hardware/$variant'))
_default_env['hardware_dir'] = os.path.realpath(_default_env.subst('$install_dir'))

if _default_env['install_dir'] == '../install':   
    _default_env['variant_dir'] = os.path.realpath(_default_env.subst('$install_dir/$hardware'))
    _default_env['install_dir'] = os.path.realpath(_default_env.subst('$install_dir/$hardware/$variant'))
else:
    _default_env['variant_dir'] = os.path.realpath(_default_env.subst('$install_dir'))
_default_env.SConsignFile('$build_dir/.sconsign.$variant')

_tool_env['hardware'] = _default_env['hardware']
_tool_env['sconsign_dir'] = _default_env['sconsign_dir']
_tool_env['build_dir'] = _default_env['build_dir']
_tool_env['hardware_dir'] = _default_env['hardware_dir']
_tool_env['variant_dir'] = _default_env['variant_dir']
_tool_env['install_dir'] = _default_env['install_dir']
_tool_env.SConsignFile('$build_dir/.sconsign.$hardware-$variant')


SConscript('Hardware/' + _default_env['hardware'], exports = {"env": _default_env})

if sys.platform == "win32":
    SConscript('Hardware/Windows', exports = {"env": _tool_env})
elif sys.platform == "linux2":
    SConscript('Hardware/Linux', exports = {"env": _tool_env})
elif sys.platform == "darwin":
    SConscript('Hardware/MacOsX', exports = {"env": _tool_env})
else:
    SConscript('Hardware/Windows', exports = {"env": _tool_env})

# include all builder definitions (*.py)
# except those 'commented out' with leading '#' character
for f in glob.glob('Builders/[!#]*.py'):
    _default_env = SConscript(f, exports = {"env": _default_env})
    _tool_env = SConscript(f, exports = {"env": _tool_env})

Return('_default_env', '_tool_env')
