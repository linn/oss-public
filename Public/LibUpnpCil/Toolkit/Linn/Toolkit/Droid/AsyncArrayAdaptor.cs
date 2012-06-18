using Android.Widget;
using Android.Content;
using System.Collections.Generic;
using Android.Views;
using System;
using System.Linq;
using Linn;
namespace OssToolkitDroid
{
    public abstract class AsyncArrayAdapter<ItemType, SectionHeaderType> : BaseAdapter
    {
        public AsyncArrayAdapter(Context aContext, IAsyncLoader<ItemType> aLoader, IInvoker aInvoker)
            : base()
        {
            iContext = aContext;
            iSectionHeaders = new List<SectionHeader<SectionHeaderType>>();
            iLoader = aLoader;
            iLoader.EventDataChanged += iLoader_DataChanged;
            iDisconnectedSectionHeaderViews = new List<View>();
            iDisconnectedItemViews = new List<View>();
            iDisconnectedPlaceholderViews = new List<View>();
            iInvoker = aInvoker;
        }


        protected virtual void AppendSectionHeaderView(Context aContext, SectionHeader<SectionHeaderType> aSectionHeader, ViewCache aViewCache, ViewGroup aRoot)
        {
            View view = new View(aContext);
            aRoot.AddView(view);
        }
        protected virtual void AppendItemView(Context aContext, ItemType aItem, ViewCache aViewCache, ViewGroup aRoot)
        {
            View view = new View(aContext);
            aRoot.AddView(view);
        }
        protected virtual void AppendPlaceholderView(Context aContext, ViewCache aViewCache, ViewGroup aRoot)
        {
            View view = new View(aContext);
            aRoot.AddView(view);
        }
        protected virtual void RecycleSectionHeaderView(Context aContext, SectionHeader<SectionHeaderType> aSectionHeader, ViewCache aViewCache) { }
        protected virtual void RecycleItemView(Context aContext, ItemType aItem, ViewCache aViewCache) { }
        protected virtual void RecyclePlaceholderView(Context aContext, ViewCache aViewCache) { }
        protected virtual void DestroySectionHeaderView(Context aContext, ViewCache aViewCache) { }
        protected virtual void DestroyItemView(Context aContext, ViewCache aViewCache) { }
        protected virtual void DestroyPlaceholderView(Context aContext, ViewCache aViewCache) { }

        public ItemType this[int aPosition]
        {
            get
            {
                SectionHeader<SectionHeaderType> header = GetSectionHeader(aPosition);
                if (header != null)
                {
                    // user has clicked on a header, return null
                    return default(ItemType);
                }
                // user has clicked on an item, return it if loaded, or null if not
                int loaderPosition = GetLoaderPosition(aPosition);
                return iLoader.Item(loaderPosition);
            }
        }

        void iLoader_DataChanged(object sender, EventArgs e)
        {
            iInvoker.BeginInvoke(((Action)(() =>
            {
                NotifyDataSetChanged();
            })));
        }

        public override Java.Lang.Object GetItem(int position)
        {
            return position;
        }

        public override long GetItemId(int position)
        {
            return position;
        }

        public void SetSectionHeaders(List<SectionHeader<SectionHeaderType>> aSectionHeaders)
        {
            if (aSectionHeaders == null) throw new ArgumentNullException();
            iSectionHeaders = aSectionHeaders;
            this.NotifyDataSetChanged();
        }

        public override int Count
        {
            get
            {
                return iLoader.Count + iSectionHeaders.Count;
            }
        }

        public override View GetView(int aPosition, View aConvertView, ViewGroup aParent)
        {
            RecyclingContainerView root = aConvertView as RecyclingContainerView;
            if (root == null)
            {
                root = CreateContainerView(iContext);
            }
            SectionHeader<SectionHeaderType> header = GetSectionHeader(aPosition);
            if (header != null)
            {
                SetHeader(root, header);
            }
            else
            {
                int loaderPosition = GetLoaderPosition(aPosition);
                ItemType item = iLoader.Item(loaderPosition);

                if (item != null)
                {
                    SetItem(root, item);
                }
                else
                {
                    SetPlaceholder(root);
                }
            }

            return root;
        }

        private void SetPlaceholder(RecyclingContainerView aRoot)
        {
            ViewCache cache = aRoot.Tag as ViewCache;
            Assert.Check(cache != null);
            if (cache.ChildType == EChildViewType.Placeholder)
            {
                RecyclePlaceholderView(iContext, cache);
            }
            else
            {
                DisconnectContainer(aRoot);
                if (iDisconnectedPlaceholderViews.Count > 0)
                {
                    View recycledView = iDisconnectedPlaceholderViews[0];
                    iDisconnectedPlaceholderViews.RemoveAt(0);
                    aRoot.AddView(recycledView);
                    RecyclePlaceholderView(iContext, cache);
                }
                else
                {
                    AppendPlaceholderView(iContext, cache, aRoot);
                }
                cache.ChildType = EChildViewType.Placeholder;
            }
        }

        private void SetHeader(RecyclingContainerView aRoot, SectionHeader<SectionHeaderType> aHeader)
        {
            ViewCache cache = aRoot.Tag as ViewCache;
            Assert.Check(cache != null);
            if (cache.ChildType == EChildViewType.Header)
            {
                RecycleSectionHeaderView(iContext, aHeader, cache);
            }
            else
            {
                DisconnectContainer(aRoot);
                if (iDisconnectedSectionHeaderViews.Count > 0)
                {
                    View recycledView = iDisconnectedSectionHeaderViews[0];
                    iDisconnectedSectionHeaderViews.RemoveAt(0);
                    aRoot.AddView(recycledView);
                    RecycleSectionHeaderView(iContext, aHeader, cache);
                }
                else
                {
                    AppendSectionHeaderView(iContext, aHeader, cache, aRoot);
                }
                cache.ChildType = EChildViewType.Header;
            }
        }

        private void SetItem(RecyclingContainerView aRoot, ItemType aItem)
        {
            ViewCache cache = aRoot.Tag as ViewCache;
            Assert.Check(cache != null);
            if (cache.ChildType == EChildViewType.Item)
            {
                RecycleItemView(iContext, aItem, cache);
            }
            else
            {
                DisconnectContainer(aRoot);
                if (iDisconnectedItemViews.Count > 0)
                {
                    View recycledView = iDisconnectedItemViews[0];
                    iDisconnectedItemViews.RemoveAt(0);
                    aRoot.AddView(recycledView);
                    RecycleItemView(iContext, aItem, cache);
                }
                else
                {
                    AppendItemView(iContext, aItem, cache, aRoot);
                }
                cache.ChildType = EChildViewType.Item;
            }
        }

        private void DisconnectContainer(RecyclingContainerView aRoot)
        {
            ViewCache cache = aRoot.Tag as ViewCache;
            Assert.Check(cache != null);
            View disconnectedChild = aRoot.GetChildAt(0);
            switch (cache.ChildType)
            {
                case EChildViewType.Placeholder:
                    {
                        if (iDisconnectedPlaceholderViews.Count < kMaxDisconnectedViews)
                        {
                            iDisconnectedPlaceholderViews.Add(disconnectedChild);
                        }
                        else
                        {
                            DestroyPlaceholderView(iContext, cache);
                        }
                        break;
                    }
                case EChildViewType.Header:
                    {
                        if (iDisconnectedSectionHeaderViews.Count < kMaxDisconnectedViews)
                        {
                            iDisconnectedSectionHeaderViews.Add(disconnectedChild);
                        }
                        else
                        {
                            DestroySectionHeaderView(iContext, cache);
                        }
                        break;
                    }
                case EChildViewType.Item:
                    {
                        if (iDisconnectedItemViews.Count < kMaxDisconnectedViews)
                        {
                            iDisconnectedItemViews.Add(disconnectedChild);
                        }
                        else
                        {
                            DestroyItemView(iContext, cache);
                        }
                        break;
                    }
                default:
                    {
                        Assert.Check(false);
                        break;
                    }
            }
            if (aRoot.ChildCount > 0)
            {
                aRoot.RemoveViewAt(0);
            }
            cache.Clear();
        }

        private RecyclingContainerView CreateContainerView(Context aContext)
        {
            RecyclingContainerView result = new RecyclingContainerView(aContext);
            ViewCache cache = new ViewCache(result);
            result.Tag = cache;
            AppendPlaceholderView(aContext, cache, result);
            return result;
        }

        private SectionHeader<SectionHeaderType> GetSectionHeader(int aPosition)
        {
            Assert.Check(aPosition < Count);
            int counter = 0;
            foreach (SectionHeader<SectionHeaderType> idx in iSectionHeaders)
            {
                if (idx.Index + counter == aPosition)
                {
                    return idx;
                }
                else if (idx.Index + counter < aPosition)
                {
                    counter++;
                }
                else
                {
                    break;
                }
            }
            return null;
        }

        private int GetLoaderPosition(int aPosition)
        {
            Assert.Check(aPosition < Count);
            int counter = 0;
            foreach (SectionHeader<SectionHeaderType> idx in iSectionHeaders)
            {
                if (idx.Index + counter == aPosition)
                {
                    Assert.Check(false);
                }
                else if (idx.Index + counter < aPosition)
                {
                    counter++;
                }
                else
                {
                    break;
                }
            }
            return aPosition - counter;
        }

        protected LayoutInflater LayoutInflater
        {
            get
            {
                return (LayoutInflater)iContext.GetSystemService(Context.LayoutInflaterService);
            }
        }

        private Context iContext;
        private IAsyncLoader<ItemType> iLoader;
        private List<SectionHeader<SectionHeaderType>> iSectionHeaders;
        private List<View> iDisconnectedSectionHeaderViews;
        private List<View> iDisconnectedItemViews;
        private List<View> iDisconnectedPlaceholderViews;
        private const int kMaxDisconnectedViews = 20;
        private IInvoker iInvoker;
    }

    public class RecyclingContainerView : LinearLayout
    {
        public RecyclingContainerView(Context aContext) : base(aContext) { }
    }

    public class ViewCache : Java.Lang.Object
    {

        public ViewCache(View aBaseView)
        {
            iBaseView = aBaseView;
            iChildViews = new Dictionary<int, View>();
            ChildType = EChildViewType.Placeholder;
        }

        protected View BaseView
        {
            get
            {
                return iBaseView;
            }
        }

        public T FindViewById<T>(int aViewId) where T : View
        {
            if (!iChildViews.ContainsKey(aViewId))
            {
                iChildViews[aViewId] = iBaseView.FindViewById<T>(aViewId);
            }
            T result = iChildViews[aViewId] as T;
            Assert.Check(result != null);
            return result;
        }

        internal void Clear()
        {
            iChildViews.Clear();
        }

        internal EChildViewType ChildType { get; set; }

        private View iBaseView;
        private Dictionary<int, View> iChildViews;
    }

    public enum EChildViewType
    {
        Placeholder,
        Header,
        Item
    }

    public interface IAsyncLoader<T>
    {
        event EventHandler<EventArgs> EventDataChanged;
        T Item(int aIndex);
        int Count { get; }
    }

    public class SectionHeader<SectionHeaderType>
    {
        public SectionHeader(int aIndex, SectionHeaderType aHeader)
        {
            iIndex = aIndex;
            iHeader = aHeader;
        }

        public int Index
        {
            get
            {
                return iIndex;
            }
        }

        public SectionHeaderType Header
        {
            get
            {
                return iHeader;
            }
        }

        private int iIndex;
        private SectionHeaderType iHeader;
    }

}