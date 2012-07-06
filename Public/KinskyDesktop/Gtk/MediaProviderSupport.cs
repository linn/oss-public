using System;

using Linn.Kinsky;

namespace KinskyDesktopGtk
{
    public class MediaProviderSupport : IContentDirectorySupportV2
    {
        public MediaProviderSupport(IVirtualFileSystem aVirtualFileSystem)
        {
            iVirtualFileSystem = aVirtualFileSystem;
        }
        
        public IVirtualFileSystem VirtualFileSystem
        {
            get
            {
                return iVirtualFileSystem;
            }
        }
        
        private IVirtualFileSystem iVirtualFileSystem;
    }
}

