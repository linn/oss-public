using System;
using System.Windows.Forms;
using System.Drawing;
using System.Collections.Generic;
using System.Net;
using System.IO;

using Linn.Control.Ssdp;
using Linn.ControlPoint.Upnp;
using Linn.Kinsky;
using Linn.Topology;
using Upnp;

namespace KinskyClassic
{
    class MediaProviderSupport : IContentDirectorySupportV2
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