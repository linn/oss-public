Import('env')

def cabinf(target, source, env):
    f = open(str(source[0]), 'r')
    s = f.read()
    f.close()

    s = s.replace('$install_dir', env['install_dir'])
    #s = s.replace('$hardware', '')#env['hardware'].replace('/', '\\'))
    s = s.replace('$variant', env['variant'])

    f = open(str(target[0]), 'w')
    f.write(s)
    f.close()

    return 0

env['BUILDERS']['CabInf'] = Builder(action = cabinf, suffix = '.inf', single_source = 1)

Return('env')
