import re

class BuildInformation(object):
    """BuildInformation."""

    def Update(self, aType='', aVersion='', aRevision='0', aAndroidVersionCode='0'):
        if self._Type == '' and aType != '':
            self._Type = aType
        if self._Version == '' and aVersion != '':
            self._Version = aVersion
        if self._Revision == '0' and aRevision != '0':
            self._Revision = aRevision
        
        formattedType = self._GetTypeString(self._Type)
        family = self._GetFamily(self._Version)
        version = self._Version
        try:
            revision = re.match("[0-9]+", self._Revision).group()
        except:
            revision = '0'
        informationalVersion = version + "." + revision
        
        self.BuildInformation = {}
        self.BuildInformation['BUILD_INFO_TYPE'] = self._Type
        self.BuildInformation['BUILD_INFO_FORMATTED_TYPE'] = formattedType
        self.BuildInformation['BUILD_INFO_FAMILY'] = family
        self.BuildInformation['BUILD_INFO_VERSION'] = version
        self.BuildInformation['BUILD_INFO_SVN_REVISION'] = revision
        self.BuildInformation['BUILD_INFO_INFORMATIONAL_VERSION'] = informationalVersion
        self.BuildInformation['BUILD_INFO_ANDROID_VERSION_CODE'] = aAndroidVersionCode
            
    def _GetFamily(self, aVersion):
        families = ['Auskerry', 'Bute', 'Cara', 'Davaar']
        try:
            num = int(aVersion.split('.')[0])
            return families[num-1]
        except:
            return ''
        
    def _GetTypeString(self, aType):
        if aType == 'release':
            return ''
        elif aType == 'beta':
            return ' (Beta)'
        elif aType == 'development':
            return ' (Development)'
        elif aType == 'nightly':
            return ' (NightlyBuild)'
        elif aType == 'developer':
            return ' (Developer)'
        else:
            return ' (' + aType + ')'
            
    def __init__(self, aType, aVersion, aRevision):
        """BuildInformation constructor."""
        self._Type = aType
        self._Version = aVersion
        self._Revision = aRevision
        self.Update(aType, aVersion, aRevision)