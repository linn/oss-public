import os
import shutil

def CopyFile(src, dst):
    print 'CopyFile(%s -> %s)' % (src, dst)
    shutil.copyfile(src, dst)


def CopyTree(src, dst):
    print 'CopyTree(%s -> %s)' % (src, dst)
    shutil.copytree(src, dst, ignore=shutil.ignore_patterns('.svn'))


def CopyDictionary(dict, rootDstFolder):
    for (relDstFolder, srcs) in dict.items():
        # create the folder for the items
        absDstFolder = os.path.join(rootDstFolder, relDstFolder)
        try:
            os.mkdir(absDstFolder)
        except OSError:
            pass

        # copy files/folders into the dst folder
        for src in srcs:
            if os.path.isdir(str(src)):
                CopyTree(str(src), os.path.join(absDstFolder, src.name))
            else:
                CopyFile(str(src), os.path.join(absDstFolder, src.name))
