Import('env')

import re
import SCons.Node.Python


def TagReplaceBuilder(target, source, env): 
    # read the source file
    f = open(str(source[0]), 'rb')
    contents = f.read()
    f.close()

    # replace instances of given tags
    for (tag, value) in env.get('TAGS', {}).items():
        r = re.compile('\$' + tag)
        contents = r.sub(value, contents)

    # write the target file
    f = open(str(target[0]), 'wb')
    f.write(contents)
    f.close()

    env.Depends(target, Value(env['TAGS']))
    return 0


def TagReplaceBuilderString(target, source, env):
    return 'Replacing tags in file: "%s" -> "%s"' % (source[0], target[0])


tagReplaceAction = Action(TagReplaceBuilder, TagReplaceBuilderString)

env['BUILDERS']['TagReplace'] = Builder(action = tagReplaceAction, suffix = '')

Return('env')

