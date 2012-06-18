using System;

using MonoTouch.UIKit;
using MonoTouch.Foundation;

namespace KinskyTouch
{
    public partial class CellPlaylistItemFactory : NSObject
    {
        public CellPlaylistItemFactory()
        {
        }

        public CellPlaylistItemFactory(IntPtr aInstance)
            : base(aInstance)
        {
        }

        public CellPlaylistItem Cell
        {
            get
            {
                return cellPlaylistItem;
            }
        }
    }

    public partial class CellPlaylistItem : CellPlaylist
    {
        public CellPlaylistItem(IntPtr aInstance)
            : base(aInstance)
        {
        }
    }
}

