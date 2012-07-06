using System;
using System.Runtime.InteropServices;

namespace Linn
{
    struct SCDynamicStoreContext
    {
        #pragma warning disable 414
        IntPtr version;
        IntPtr info;
        IntPtr retain;
        IntPtr release;
        IntPtr copyDescription;
        #pragma warning restore 414
    }

    public class NetworkChangeWatcher : IDisposable
    {

        private InterfaceChangedObserver iObserver;
        public event EventHandler<EventArgs> EventNetworkChanged;
        

        private void OnEventNetworkChanged()
        {
            EventHandler<EventArgs> del = EventNetworkChanged;
            if (del != null)
            {
                del(null, EventArgs.Empty);
            }
        }


        private IntPtr kAddIPAddressListChangeCallbackSCF = __CFStringMakeConstantString("AddIPAddressListChangeCallbackSCF");
        private IntPtr kSCDynamicStoreDomainState = __CFStringMakeConstantString("State:");
        private IntPtr kSCCompAnyRegex = __CFStringMakeConstantString("[^/]+");
        private IntPtr kSCEntNetIPv4 = __CFStringMakeConstantString("IPv4");
        private IntPtr kCFRunLoopCommonModes = __CFStringMakeConstantString("kCFRunLoopCommonModes");
        private SCDynamicStoreContext iContext;
        private IntPtr iCallout;
        private IntPtr iStore;
        private IntPtr iPattern;
        private IntPtr iPatternList;
        private IntPtr iRunLoopSource;
        private IntPtr iRunLoop;

        private IntPtr iModeString;

        private IntPtr kBundleIdentifier = __CFStringMakeConstantString("com.apple.CoreFoundation");
        private delegate void InterfaceChangedObserver(IntPtr aStoreRef, IntPtr changedKeys, IntPtr info);  

        public NetworkChangeWatcher()
        {
            CreateInterfaceChangedObserver();
        }

        ~NetworkChangeWatcher()
        {
            Dispose(false);
        }

        private void CreateInterfaceChangedObserver()
        {
            try
            {
                unsafe
                {
                    iObserver = InterfaceChanged;
                    iCallout = Marshal.GetFunctionPointerForDelegate(iObserver);
                    iContext = new SCDynamicStoreContext();
                    iStore = SCDynamicStoreCreate(IntPtr.Zero, kAddIPAddressListChangeCallbackSCF, iCallout, iContext);
                    if (iStore == IntPtr.Zero)
                    {
                        throw new ApplicationException("Failed to invoke SCDynamicStoreCreate");
                    }
        
                    iPattern = SCDynamicStoreKeyCreateNetworkServiceEntity(IntPtr.Zero, kSCDynamicStoreDomainState, kSCCompAnyRegex, kSCEntNetIPv4);
                    if (iPattern == IntPtr.Zero)
                    {
                        throw new ApplicationException("Failed to invoke SCDynamicStoreKeyCreateNetworkServiceEntity");
                    }
        
                    iPatternList = CFArrayCreate(IntPtr.Zero, new IntPtr[1] { iPattern }, 1, IntPtr.Zero);
                    if (iPatternList == IntPtr.Zero)
                    {
                        throw new ApplicationException("Failed to invoke CFArrayCreate");
                    }
                    bool setNotificationKeys = SCDynamicStoreSetNotificationKeys(iStore, IntPtr.Zero, iPatternList);
                    if (!setNotificationKeys)
                    {
                        throw new ApplicationException("Failed to invoke SCDynamicStoreSetNotificationKeys");
                    }
                    iRunLoopSource = SCDynamicStoreCreateRunLoopSource(IntPtr.Zero, iStore, 0);
                    if (iRunLoopSource == IntPtr.Zero)
                    {
                        throw new ApplicationException("Failed to invoke SCDynamicStoreCreateRunLoopSource");
                    }
                    iRunLoop = CFRunLoopGetMain();
                    if (iRunLoop == IntPtr.Zero)
                    {
                        throw new ApplicationException("Failed to invoke CFRunLoopGetMain");
                    }
        
                    IntPtr bundle = CFBundleGetBundleWithIdentifier(kBundleIdentifier);
                    IntPtr *stringRef = (IntPtr*)CFBundleGetDataPointerForName(bundle, kCFRunLoopCommonModes);
                    iModeString = *stringRef;
        
                    CFRunLoopAddSource(iRunLoop, iRunLoopSource, iModeString);
                    CFRelease(iPattern);
                    CFRelease(iPatternList);
                }
            }
            catch(Exception ex)
            {
                string message = "Error creating NetworkChangeWatcher: " + ex;
                UserLog.WriteLine(message);            
                Trace.WriteLine(Trace.kCore, message);

                if (iPattern != IntPtr.Zero)
                {
                    CFRelease(iPattern);
                }

                if (iPatternList != IntPtr.Zero) 
                {
                    CFRelease(iPatternList);
                }
                if (iStore != IntPtr.Zero)
                {
                    CFRelease(iStore);
                } 
                if (iRunLoopSource != IntPtr.Zero)
                {
                    CFRelease(iRunLoopSource);
                }
            }
        }

        private void DestroyInterfaceChangedObserver()
        {
            if (iRunLoop != IntPtr.Zero)
            {
                CFRunLoopRemoveSource(iRunLoop, iRunLoopSource, iModeString);
                CFRelease(iStore);
                CFRelease(iRunLoopSource);
                iRunLoop = IntPtr.Zero;
            }
        }
        
        private void InterfaceChanged(IntPtr aStoreRef, IntPtr changedKeys, IntPtr info)
        {
            UserLog.WriteLine("InterfaceChanged");
            Trace.WriteLine(Trace.kCore, "InterfaceChanged");
            OnEventNetworkChanged();
        }

        #region IDisposable implementation
        public void Dispose ()
        {
            Dispose(true);
        }
        #endregion

        protected virtual void Dispose(bool aDisposing)
        {
            DestroyInterfaceChangedObserver();
        }

        [DllImport ("/System/Library/Frameworks/SystemConfiguration.framework/Versions/Current/SystemConfiguration")]
        private static extern IntPtr SCDynamicStoreCreate(IntPtr allocator, IntPtr name, IntPtr callout, SCDynamicStoreContext context);

        [DllImport ("/System/Library/Frameworks/SystemConfiguration.framework/Versions/Current/SystemConfiguration")]
        private static extern IntPtr SCDynamicStoreKeyCreateNetworkServiceEntity(IntPtr allocator, IntPtr domain, IntPtr serviceID, IntPtr entity);

        [DllImport ("/System/Library/Frameworks/SystemConfiguration.framework/Versions/Current/SystemConfiguration")]
        private static extern bool SCDynamicStoreSetNotificationKeys(IntPtr store, IntPtr keys, IntPtr patterns);

        [DllImport ("/System/Library/Frameworks/SystemConfiguration.framework/Versions/Current/SystemConfiguration")]
        private static extern IntPtr SCDynamicStoreCreateRunLoopSource(IntPtr allocator, IntPtr store, int order);

        [DllImport ("/System/Library/Frameworks/ApplicationServices.framework/Versions/Current/ApplicationServices")]
        private static extern IntPtr __CFStringMakeConstantString(string aString);

        [DllImport ("/System/Library/Frameworks/CoreFoundation.framework/Versions/Current/CoreFoundation")]
        private static extern IntPtr CFArrayCreate(IntPtr allocator, IntPtr[] values, int numValues, IntPtr callbacks);

        [DllImport ("/System/Library/Frameworks/CoreFoundation.framework/Versions/Current/CoreFoundation")]
        private static extern IntPtr CFRunLoopGetMain();

        [DllImport ("/System/Library/Frameworks/CoreFoundation.framework/Versions/Current/CoreFoundation")]
        private static extern void CFRunLoopAddSource(IntPtr runLoop, IntPtr runLoopSource, IntPtr mode);

        [DllImport ("/System/Library/Frameworks/CoreFoundation.framework/Versions/Current/CoreFoundation")]
        private static extern void CFRunLoopRemoveSource(IntPtr runLoop, IntPtr runLoopSource, IntPtr mode);

        [DllImport ("/System/Library/Frameworks/CoreFoundation.framework/Versions/Current/CoreFoundation")]
        private static extern IntPtr CFBundleGetBundleWithIdentifier(IntPtr bundleID);

        [DllImport ("/System/Library/Frameworks/CoreFoundation.framework/Versions/Current/CoreFoundation")]
        private static extern IntPtr CFBundleGetDataPointerForName(IntPtr bundle, IntPtr symbolName);

        [DllImport ("/System/Library/Frameworks/CoreFoundation.framework/Versions/Current/CoreFoundation")]
        private static extern void CFRelease(IntPtr aReference);


    }
}
