using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System;
using System.Collections.ObjectModel;
using System.Collections;
using System.Linq;
using System.Windows.Threading;
using Linn;
using Linn.Kinsky;

namespace KinskyDesktopWpf
{

    public abstract class LazyLoadingList<ViewModelType, ContentType>
        : IList<ViewModelType>,
        INotifyCollectionChanged,
        INotifyPropertyChanged,
        ICollectionViewFactory,
        ICollectionView,
        IDisposable
        where ViewModelType : class
    {

        private ViewModelType[] iCachedItems;
        private const int kChunkSize = 250;
        private object iCurrentItem;
        private int iCurrentPosition;
        private int iDeferredRefreshCount;
        private CultureInfo iCulture;
        private int iCount;
        private ViewModelType iPlaceholderItem;
        private IContentCollector<ContentType> iContentCollector;
        private Dispatcher iDispatcher;
        private bool iDisposed;

        public LazyLoadingList(IContentCollector<ContentType> aContentCollector, Dispatcher aDispatcher)
        {
            iCachedItems = new ViewModelType[0];
            iCount = 0;
            iDispatcher = aDispatcher;
            iContentCollector = aContentCollector;
            iContentCollector.EventOpened += iContentCollector_EventOpened;
            iContentCollector.EventItemsLoaded += iContentCollector_EventItemsLoaded;
            iPlaceholderItem = CreateViewModel(default(ContentType));
        }

        public abstract ViewModelType CreateViewModel(ContentType aItem);

        void iContentCollector_EventItemsLoaded(object sender, EventArgsItemsLoaded<ContentType> e)
        {
            iDispatcher.BeginInvoke((Action)(() =>
            {
                if (!iDisposed)
                {
                    Assert.Check(iCachedItems != null);
                    for (int i = 0; i < e.Items.Count; i++)
                    {
                        int index = e.StartIndex + i;
                        ViewModelType previous = iCachedItems[index];
                        if (previous == null || previous == iPlaceholderItem)
                        {
                            ViewModelType wrappedItem = CreateViewModel(e.Items[i]);
                            this[index] = wrappedItem;
                        }
                    }
                }
            }), DispatcherPriority.SystemIdle);
        }

        void iContentCollector_EventOpened(object sender, EventArgs e)
        {
            iDispatcher.BeginInvoke((Action)(() =>
            {
                if (!iDisposed)
                {
                    iCount = (int)iContentCollector.Count;
                    iCachedItems = new ViewModelType[iCount];
                    for (int i = 0; i < iCount; i++)
                    {
                        iCachedItems[i] = iPlaceholderItem;
                    }
                    OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
                    OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));
                    OnPropertyChanged(new PropertyChangedEventArgs("Count"));
                }
            }));
        }

        #region IList<T> Members

        public int IndexOf(ViewModelType item)
        {
            int counter = 0;
            foreach (ViewModelType t in iCachedItems)
            {
                if (item == t)
                {
                    return counter;
                }
                counter++;
            }
            return -1;
        }

        public void Insert(int index, ViewModelType item)
        {
            throw new NotImplementedException();
        }

        public void RemoveAt(int index)
        {
            throw new NotImplementedException();
        }

        public ViewModelType this[int index]
        {
            get
            {
                Assert.Check(index < iCachedItems.Length);
                if (iCachedItems[index] != iPlaceholderItem)
                {
                    return iCachedItems[index];
                }
                ContentType cachedItem = iContentCollector.Item(index, ERequestPriority.Foreground);
                if (cachedItem != null)
                {
                    ViewModelType wrappedItem = CreateViewModel(cachedItem);
                    iCachedItems[index] = wrappedItem;
                    OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, wrappedItem, iPlaceholderItem, index));
                    return wrappedItem;
                }
                return iPlaceholderItem;
            }
            set
            {
                iCachedItems[index] = value;
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, value, iPlaceholderItem, index));
            }
        }

        #endregion

        #region ICollection<T> Members

        public void Add(ViewModelType item)
        {
            throw new NotImplementedException();
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }

        public bool Contains(ViewModelType item)
        {
            return this.IndexOf(item) != -1;
        }

        public void CopyTo(ViewModelType[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public int Count
        {
            get { return iCount; }
        }

        public bool IsReadOnly
        {
            get { throw new NotImplementedException(); }
        }

        public bool Remove(ViewModelType item)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IEnumerable<T> Members

        public IEnumerator<ViewModelType> GetEnumerator()
        {
            for (int i = 0; i < Count; i++)
            {
                yield return iCachedItems[i];
            }
        }

        #endregion

        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        #region INotifyCollectionChanged Members

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        private void OnCollectionChanged(NotifyCollectionChangedEventArgs aArgs)
        {
            if (CollectionChanged != null)
            {
                CollectionChanged(this, aArgs);
            }
        }
        #endregion

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(PropertyChangedEventArgs args)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, args);
            }
        }
        #endregion

        #region ICollectionViewFactory Members

        public ICollectionView CreateView()
        {
            return this;
        }

        #endregion

        #region ICollectionView Members

        public bool CanFilter
        {
            get { return false; }
        }

        public bool CanGroup
        {
            get { return false; }
        }

        public bool CanSort
        {
            get { return false; }
        }

        public bool Contains(object item)
        {
            return Contains((ViewModelType)item);
        }

        public CultureInfo Culture
        {
            get
            {
                return iCulture;
            }
            set
            {
                iCulture = value;
                OnPropertyChanged(new PropertyChangedEventArgs("Culture"));
            }
        }

        public event EventHandler CurrentChanged;
        public void OnCurrentChanged()
        {
            if (CurrentChanged != null)
            {
                CurrentChanged(this, EventArgs.Empty);
            }
        }
        public event CurrentChangingEventHandler CurrentChanging;
        public bool OnCurrentChanging()
        {
            CurrentChangingEventArgs args = new CurrentChangingEventArgs(true);
            if (CurrentChanging != null)
            {
                CurrentChanging(this, args);
            }
            return args.Cancel;
        }

        public object CurrentItem
        {
            get { return iCurrentItem; }
        }

        public int CurrentPosition
        {
            get { return iCurrentPosition; }
        }

        public IDisposable DeferRefresh()
        {
            iDeferredRefreshCount += 1;
            return new DeferredRefreshHandler(this);
        }

        public Predicate<object> Filter
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public ObservableCollection<GroupDescription> GroupDescriptions
        {
            get { return null; }
        }

        public ReadOnlyObservableCollection<object> Groups
        {
            get { return null; }
        }

        public bool IsCurrentAfterLast
        {
            get { return iCurrentPosition >= Count; }
        }

        public bool IsCurrentBeforeFirst
        {
            get { return iCurrentPosition < 0; }
        }

        public bool IsEmpty
        {
            get { return Count == 0; }
        }

        public bool MoveCurrentTo(object item)
        {

            if (iCurrentItem == item && (item != null || iCurrentPosition >= 0 && iCurrentPosition < Count))
            {
                return iCurrentPosition >= 0 && iCurrentPosition < Count;
            }
            return MoveCurrentToPosition(IndexOf((ViewModelType)item));
        }

        public bool MoveCurrentToFirst()
        {
            return MoveCurrentToPosition(0);
        }

        public bool MoveCurrentToLast()
        {
            return MoveCurrentToPosition(Count - 1);
        }

        public bool MoveCurrentToNext()
        {
            return iCurrentPosition <= Count && MoveCurrentToPosition(iCurrentPosition + 1);
        }

        public bool MoveCurrentToPosition(int position)
        {
            if (position < -1 || position > Count)
                throw new ArgumentOutOfRangeException("position");
            if (position != iCurrentPosition && OnCurrentChanging())
            {
                bool prevCurrentBeforeFirst = IsCurrentBeforeFirst;
                bool prevCurrentAfterLast = IsCurrentAfterLast;
                if (position < 0)
                {
                    iCurrentItem = null;
                    iCurrentPosition = -1;
                }
                else if (position >= Count)
                {
                    iCurrentItem = null;
                    iCurrentPosition = Count;
                }
                else
                {
                    iCurrentPosition = position;
                    iCurrentItem = this[position];
                }
                OnCurrentChanged();
                if (prevCurrentBeforeFirst != IsCurrentBeforeFirst)
                {
                    OnPropertyChanged(new PropertyChangedEventArgs("IsCurrentBeforeFirst"));
                }
                if (prevCurrentAfterLast != IsCurrentAfterLast)
                {
                    OnPropertyChanged(new PropertyChangedEventArgs("IsCurrentAfterLast"));
                }
                OnPropertyChanged(new PropertyChangedEventArgs("CurrentPosition"));
                OnPropertyChanged(new PropertyChangedEventArgs("CurrentItem"));
            }
            return iCurrentPosition >= 0 && iCurrentPosition < Count;
        }

        public bool MoveCurrentToPrevious()
        {
            return iCurrentPosition >= 0 && MoveCurrentToPosition(iCurrentPosition - 1);
        }

        public void Refresh()
        {
            OnCurrentChanging();
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            OnCurrentChanged();
        }

        public SortDescriptionCollection SortDescriptions
        {
            get { return new SortDescriptionCollection(); }
        }

        public IEnumerable SourceCollection
        {
            get { return this; }
        }

        #endregion



        private class DeferredRefreshHandler : IDisposable
        {
            private LazyLoadingList<ViewModelType, ContentType> iList;

            public DeferredRefreshHandler(LazyLoadingList<ViewModelType, ContentType> aList)
            {
                iList = aList;
            }

            #region IDisposable Members

            public void Dispose()
            {
                if (iList != null)
                {
                    iList.DeferredRefreshDisposed();
                    iList = null;
                }
            }

            #endregion
        }

        private void DeferredRefreshDisposed()
        {
            iDeferredRefreshCount -= 1;
            if (iDeferredRefreshCount == 0)
            {
                Refresh();
            }
        }


        #region IDisposable Members

        public void Dispose()
        {
            iDisposed = true;
            iContentCollector.EventOpened -= iContentCollector_EventOpened;
            iContentCollector.EventItemsLoaded -= iContentCollector_EventItemsLoaded;
            iCount = 0;
        }

        #endregion

        public int Find(Predicate<ViewModelType> aPredicate)
        {
            for (int i = 0; i < iCachedItems.Length; i++)
            {
                if (aPredicate(iCachedItems[i]))
                {
                    return i;
                }
            }
            return -1;
        }
    }

}