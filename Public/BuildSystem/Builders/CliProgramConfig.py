Import('env')

import SCons.Node
import os
import os.path

def CliProgramConfigBuilder(target, source, env):
    
    config = """<configuration>
  <system.net>
    <settings>
      <socket alwaysUseCompletionPortsForConnect="true" alwaysUseCompletionPortsForAccept="true"/>
      <servicePointManager expect100Continue="false" useNagleAlgorithm="false"/>
    </settings>
  </system.net>
</configuration>
"""

    file = open(str(target[0]), 'wb')
    file.write(config)
    file.close()
    return 0

def CliProgramConfigBuilderString(target, source, env):
    return 'Generating config file: "%s"' % target[0]

cliProgramConfigAction = Action(CliProgramConfigBuilder, CliProgramConfigBuilderString)
env['BUILDERS']['CliProgramConfig'] = Builder(action = cliProgramConfigAction, suffix = '.exe.config')

Return('env')
