
#import <Cocoa/Cocoa.h>


// Enum for the receiver state
typedef enum
{
    eReceiverStateOffline,
    eReceiverStateDisconnected,
    eReceiverStateConnecting,
    eReceiverStateConnected
    
} EReceiverState;



@interface PrefReceiver : NSObject
{
    NSString* udn;
    NSString* room;
    NSString* group;
    NSString* name;
    EReceiverState status;
}

@property (copy) NSString* udn;
@property (copy) NSString* room;
@property (copy) NSString* group;
@property (copy) NSString* name;
@property (assign) EReceiverState status;

- (id) initWithDict:(NSDictionary*)aDict;
- (NSDictionary*) convertToDict;

@end



@interface PrefSubnet : NSObject
{
    uint32_t address;
    NSString* name;
}

@property (assign) uint32_t address;
@property (copy) NSString* name;

- (id) initWithDict:(NSDictionary*)aDict;
- (NSDictionary*) convertToDict;

@end



@interface PrefMulticastChannel : NSObject
{
}

+ (uint32_t) generate;

@end



@interface Preferences : NSObject
{
}

// functions to get current preferences
- (bool) enabled;
- (bool) multicastEnabled;
- (uint32_t) multicastChannel;
- (uint32_t) musicLatencyMs;
- (uint32_t) videoLatencyMs;
- (uint32_t) defaultMusicLatencyMs;
- (uint32_t) defaultVideoLatencyMs;
- (bool) rotaryVolumeControl;
- (bool) autoUpdatesEnabled;
- (bool) betaUpdatesEnabled;
- (bool) usageDataEnabled;
- (NSArray*) receiverList;
- (NSString*) selectedReceiverUdn;
- (NSArray*) subnetList;
- (uint32_t) selectedSubnetAddress;

// functions to set preferences - not all preferences can be set by the pref pane app
- (void) setMulticastEnabled:(bool)aEnabled;
- (void) setMulticastChannel:(uint32_t)aChannel;
- (void) setMusicLatencyMs:(uint32_t)aLatencyMs;
- (void) setVideoLatencyMs:(uint32_t)aLatencyMs;
- (void) setRotaryVolumeControl:(bool)aRotaryVolumeControl;
- (void) setAutoUpdatesEnabled:(bool)aEnabled;
- (void) setBetaUpdatesEnabled:(bool)aEnabled;
- (void) setUsageDataEnabled:(bool)aEnabled;
- (void) setSelectedReceiverUdn:(NSString*)aUdn;
- (void) setSelectedSubnetAddress:(uint32_t)aAddress;

// functions to register for notifications i.e. these can be changed by the main songcast app
- (void) addObserverEnabled:(id)aObserver selector:(SEL)aSelector;
- (void) addObserverAutoUpdatesEnabled:(id)aObserver selector:(SEL)aSelector;
- (void) addObserverReceiverList:(id)aObserver selector:(SEL)aSelector;
- (void) addObserverSelectedReceiverUdn:(id)aObserver selector:(SEL)aSelector;
- (void) addObserverSubnetList:(id)aObserver selector:(SEL)aSelector;
- (void) addObserverSelectedSubnet:(id)aObserver selector:(SEL)aSelector;
- (void) addObserverAll:(id)aObserver selector:(SEL)aSelector;

// functions to send button press notifications to the main songcast app
- (void) refreshReceiverList;
- (void) checkForUpdates;

@end



