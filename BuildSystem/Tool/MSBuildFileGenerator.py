import os.path
import shutil
import SCons.Builder
import SCons.Node.FS
import SCons.Util
import CsProg
from xml.dom.minidom import parseString
import uuid 



CsProgramScanner = CsProg.CsProgramScanner()

def MSBuildFileGeneratorFn(target, source, env):
    builddir, filename = os.path.split(target[0].abspath)
    # filename has extension .csproj, strip this out
    filename = os.path.splitext(filename)[0]
    
    if 'OUTPUTTYPE' not in env:
        outputType = 'Library' if env['hardware'] == 'Android' else 'WinExe'
    else:
        outputType = env['OUTPUTTYPE']
        if SCons.Util.is_List(outputType):
            outputType = outputType[0]
    if outputType not in ['Library', 'WinExe', 'Exe']:
        raise Exception("OutputType must be one of 'Library', 'WinExe' or 'Exe'")	

    # ensure key info is provided for android release mode signing
    if env['hardware'] == "Android" and env['variant'] == 'release':
        for key in ['ANDROIDKEYSTORE', 'ANDROIDKEYSTOREPASS', 'ANDROIDKEYALIAS', 'ANDROIDKEYPASS']:
            if key not in env: raise Exception("Missing %s in environment." % key)

    msbuildfile = """<?xml version="1.0" encoding="utf-8"?>
    <Project ToolsVersion="%(TOOLSVERSION)s" DefaultTargets="%(DEFAULTTARGETS)s" xmlns="http://schemas.microsoft.com/developer/msbuild/2003">
      <PropertyGroup>
        <AlwaysCompileMarkupFilesInSeparateDomain>true</AlwaysCompileMarkupFilesInSeparateDomain>
        <Configuration Condition=" '$(Configuration)' == '' ">Debug</Configuration>
        <Platform Condition=" '$(Platform)' == '' ">%(CLIPLATFORMTARGET)s</Platform>
        <PlatformTarget>%(CLIPLATFORMTARGET)s</PlatformTarget>
        <ProductVersion>%(PRODUCTVERSION)s</ProductVersion>
        <SchemaVersion>%(SCHEMAVERSION)s</SchemaVersion>
        <ProjectGuid>{%(UUID)s}</ProjectGuid>
        <OutputType>%(OUTPUTTYPE)s</OutputType>
        <AppDesignerFolder>Properties</AppDesignerFolder>
        <RootNamespace>%(ROOTNAMESPACE)s</RootNamespace>
        <AssemblyName>%(ASSEMBLYNAME)s</AssemblyName>
        <TargetFrameworkVersion>v%(TARGETFRAMEWORKVERSION)s</TargetFrameworkVersion>
        <FileAlignment>512</FileAlignment>
        <ProjectTypeGuids>%(PROJECTTYPEGUIDS)s</ProjectTypeGuids>
        <WarningLevel>4</WarningLevel>
        <AndroidResgenClass>Resource</AndroidResgenClass>
        <MonoDroidResourcePrefix>Resources</MonoDroidResourcePrefix>
        <AndroidApplication>True</AndroidApplication>
        <AndroidSupportedAbis>%(ANDROIDSUPPORTEDABIS)s</AndroidSupportedAbis>
        <AndroidStoreUncompressedFileExtensions />
        <MandroidI18n />
        <AndroidResgenFile>%(ANDROIDRESGENFILE)s</AndroidResgenFile>
        <AndroidManifest>%(ANDROIDMANIFEST)s</AndroidManifest>
        %(APPLICATIONICON)s
      </PropertyGroup>
      <PropertyGroup Condition=" '$(Configuration)|$(Platform)' == 'Trace|%(CLIPLATFORMTARGET)s' ">
        <DebugSymbols>true</DebugSymbols>
        <DebugType>full</DebugType>
        <Optimize>false</Optimize>
        <OutputPath>%(OUTPUTPATH)s</OutputPath>
        <DefineConstants>DEBUG;TRACE</DefineConstants>
        <ErrorReport>prompt</ErrorReport>
        <WarningLevel>4</WarningLevel>
        <ConsolePause>false</ConsolePause>
        <AndroidUseSharedRuntime>false</AndroidUseSharedRuntime>
        <AndroidLinkMode>SdkOnly</AndroidLinkMode>
      </PropertyGroup>
      <PropertyGroup Condition=" '$(Configuration)|$(Platform)' == 'Debug|%(CLIPLATFORMTARGET)s' ">
        <DebugSymbols>true</DebugSymbols>
        <DebugType>full</DebugType>
        <Optimize>true</Optimize>
        <OutputPath>%(OUTPUTPATH)s</OutputPath>
        <DefineConstants>TRACE</DefineConstants>
        <ErrorReport>prompt</ErrorReport>
        <WarningLevel>4</WarningLevel>
        <ConsolePause>false</ConsolePause>
        <AndroidUseSharedRuntime>true</AndroidUseSharedRuntime>
        <AndroidLinkMode>None</AndroidLinkMode>
      </PropertyGroup>
      <PropertyGroup Condition=" '$(Configuration)|$(Platform)' == 'Release|%(CLIPLATFORMTARGET)s' ">
        <DebugType>pdbonly</DebugType>
        <Optimize>true</Optimize>
        <OutputPath>%(OUTPUTPATH)s</OutputPath>
        <ErrorReport>prompt</ErrorReport>
        <WarningLevel>4</WarningLevel>
        <ConsolePause>false</ConsolePause>
        <AndroidUseSharedRuntime>false</AndroidUseSharedRuntime>
        <AndroidLinkMode>SdkOnly</AndroidLinkMode>
      </PropertyGroup>
      <PropertyGroup Condition="'$(Configuration)' == 'Release'">  
        <AndroidKeyStore>True</AndroidKeyStore>  
        <AndroidSigningKeyStore>%(ANDROIDKEYSTORE)s</AndroidSigningKeyStore>  
        <AndroidSigningStorePass>%(ANDROIDKEYSTOREPASS)s</AndroidSigningStorePass>  
        <AndroidSigningKeyAlias>%(ANDROIDKEYALIAS)s</AndroidSigningKeyAlias>  
        <AndroidSigningKeyPass>%(ANDROIDKEYPASS)s</AndroidSigningKeyPass>  
      </PropertyGroup>
      %(IMPORTS)s
      <Target Name="BeforeResolveReferences">  
        <CreateProperty Value="%(CLILIBPATH)s$(AssemblySearchPaths)">  
        <Output TaskParameter="Value" PropertyName="AssemblySearchPaths" />  
        </CreateProperty>  
      </Target>
    </Project>
    """ % {
              'TOOLSVERSION' : "4.0" if 'TOOLSVERSION' not in env else env['TOOLSVERSION'],
              'PRODUCTVERSION' : "10.0.0" if 'PRODUCTVERSION' not in env else env['PRODUCTVERSION'],
              'SCHEMAVERSION' : "2.0" if 'SCHEMAVERSION' not in env else env['SCHEMAVERSION'],
              'TARGETFRAMEWORKVERSION' : "2.2" if 'TARGETFRAMEWORKVERSION' not in env and env['hardware'] == "Android" else "4.0" if 'TARGETFRAMEWORKVERSION' not in env else env['TARGETFRAMEWORKVERSION'],
              'CLIPLATFORMTARGET' : env['CLIPLATFORMTARGET'],
              'PROJECTTYPEGUIDS' : "{EFBA0AD7-5A72-4C68-AF49-83D382785DCF};{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}" if env['hardware'] == 'Android' else "{60dc8134-eba5-43b8-bcc9-bb4bc16c2548};{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}",
              'ROOTNAMESPACE' : filename if 'ROOTNAMESPACE' not in env else env['ROOTNAMESPACE'],
              'ASSEMBLYNAME' : filename,
              'OUTPUTTYPE' : outputType,
              'APPLICATIONICON' : "" if 'ICON' not in env else "<ApplicationIcon>%(ICON)s</ApplicationIcon>" % { 'ICON' : env['ICON'] },
              'OUTPUTPATH' : builddir if 'OUTPUTPATH' not in env else env['OUTPUTPATH'],
              'IMPORTS' : '<Import Project="$(MSBuildExtensionsPath)\Novell\Novell.MonoDroid.CSharp.targets" />' if env['hardware'] == 'Android' else '<Import Project="$(MSBuildToolsPath)\Microsoft.CSharp.targets" />',
              'ANDROIDRESGENFILE' : "" if 'ANDROIDRESGENFILE' not in env else env['ANDROIDRESGENFILE'],
              'ANDROIDMANIFEST' : "" if 'ANDROIDMANIFEST' not in env else env['ANDROIDMANIFEST'],
              'CLILIBPATH' : "" if 'CLILIBPATH' not in env else expandCliLibs(env),
              'DEFAULTTARGETS' : "Build" if 'DEFAULTTARGETS' not in env else env['DEFAULTTARGETS'],
              'ANDROIDKEYSTORE' : "" if 'ANDROIDKEYSTORE' not in env else env['ANDROIDKEYSTORE'],
              'ANDROIDKEYSTOREPASS' : "" if 'ANDROIDKEYSTOREPASS' not in env else env['ANDROIDKEYSTOREPASS'],
              'ANDROIDKEYALIAS' : "" if 'ANDROIDKEYALIAS' not in env else env['ANDROIDKEYALIAS'],
              'ANDROIDKEYPASS' : "" if 'ANDROIDKEYPASS' not in env else env['ANDROIDKEYPASS'],
              'ANDROIDSUPPORTEDABIS' : "armeabi%3barmeabi-v7a" if 'ANDROIDSUPPORTEDABIS' not in env else env['ANDROIDSUPPORTEDABIS'],
              'UUID' : uuid.uuid1(),
          }          
        
    doc = parseString(msbuildfile)
    topElement = doc.documentElement

    refsContainer = doc.createElement("ItemGroup")
    topElement.appendChild(refsContainer)
    try:
        libs = env['CLILIBS']
        if not SCons.Util.is_List(libs):
            libs = [ libs ]
    except KeyError:
        libs = []
        
    for ref in libs:
        refsContainer.appendChild(createReferenceElement(ref, doc))
    
    srcContainer = doc.createElement("ItemGroup")
    topElement.appendChild(srcContainer)
    links = source if 'LINKS' not in env else SCons.Util.flatten(env["LINKS"])
    i = 0
    for src in source:
        link = links[i]
        if isinstance(link, SCons.Node.FS.Base):
            link = link.abspath
        srcContainer.appendChild(createSourceElement(src, doc, link, env))
        i+=1    
    # add compilation element for the generated resources.designer.cs if present
    if 'ANDROIDRESGENFILE' in env:
        srcElement = doc.createElement("Compile")
        srcElement.setAttribute("Include", env['ANDROIDRESGENFILE'])
        linkElement = doc.createElement("Link")
        linkElement.appendChild(doc.createTextNode(env['ANDROIDRESGENFILE'].replace("/","\\")))
        srcElement.appendChild(linkElement)
        srcContainer.appendChild(srcElement)
    
    msbuild = open(target[0].abspath,"w")
    msbuild.write(doc.toxml())
    msbuild.close()    
    
    return None

def expandCliLibs(env):
    result = ""
    for lib in env['CLILIBPATH']:
        result += env.subst(lib) + ";"
    return result

def createSourceElement(src, doc, link, env):
    file, extension = os.path.splitext(src.abspath)   
    extension = extension[1:]
    filename = os.path.split(src.abspath)[-1]
    elementName = getElementName(extension)
    # override elementName if filename is in an array env variable called 'ANDROIDRESOURCES'
    if 'ANDROIDRESOURCES' in env and link in env['ANDROIDRESOURCES']:
        elementName = 'AndroidResource'
    # override elementName if it is defined as a xaml app definition
    if ('APPLICATIONDEFINITION' in env):
        if (os.path.split(file)[-1] == env['APPLICATIONDEFINITION']):
            elementName='ApplicationDefinition'
    srcElement = doc.createElement(elementName)
    srcElement.setAttribute("Include", src.abspath)
    linkElement = doc.createElement("Link")
    linkElement.appendChild(doc.createTextNode(link.replace("/","\\")))
    srcElement.appendChild(linkElement)
    generator = getGenerator(extension)
    if (generator != ""):
        generatorElem = doc.createElement("Generator")
        generatorElem.appendChild(doc.createTextNode(generator))
        srcElement.appendChild(generatorElem)
    return srcElement

def getElementName(ext):
    names = {
        "cs":"Compile",
        "xaml":"Page",
        "settings":"None",
        "resx":"EmbeddedResource",
        "jar":"AndroidJavaLibrary"
    }
    try:
        return names[ext.lower()]
    except KeyError:
        return ""

def getGenerator(ext):
    generators = {
        "xaml":"MSBuild:Compile",
        "resx":"PublicResXFileCodeGenerator",
        "settings":"SettingsSingleFileGenerator"
    }
    try:
        return generators[ext.lower()]
    except KeyError:
        return ""

def createReferenceElement(ref, doc):
    refElement = doc.createElement("Reference")
    if (isinstance(ref, str)):
        refElement.setAttribute("Include", ref)
    else:
        refElement.setAttribute("Include", ref[0])
        hintPathElem = doc.createElement("HintPath")
        hintPathElem.appendChild(doc.createTextNode(ref[1]))
        refElement.appendChild(hintPathElem)
    return refElement

MSBuildFileGenerator = SCons.Builder.Builder(action = MSBuildFileGeneratorFn, 
                                  suffix = '$MSBUILDSUFFIX',
                                  target_scanner = CsProgramScanner)
def generate(env):
    env['BUILDERS']['MSBuildFileGenerator'] = MSBuildFileGenerator
    env["MSBUILDSUFFIX"] = ".csproj"
    env["CLIPLATFORMTARGET"] = "AnyCPU"
    env['CLIPROGPREFIX']   = ''
    env['CLIPROGSUFFIX']   = '.exe'
    env['CLILIBPREFIX']   = ''
    env['CLILIBSUFFIX']   = '.dll'
    env['CLILIBPREFIXES'] = [ '$CLILIBPREFIX' ]
    env['CLILIBSUFFIXES'] = [ '$CLILIBSUFFIX' ]

def exists(env):
    return env.Detect('MSBuild')

    
