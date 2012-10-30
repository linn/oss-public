
#import "Preferences.h"


@implementation PrefReceiver

@synthesize udn;
@synthesize room;
@synthesize group;
@synthesize name;
@synthesize status;


- (id) initWithDict:(NSDictionary*)aDict
{
    self = [super init];

    self.udn = [aDict objectForKey:@"Udn"];
    self.room = [aDict objectForKey:@"Room"];
    self.group = [aDict objectForKey:@"Group"];
    self.name = [aDict objectForKey:@"Name"];
    self.status = [[aDict objectForKey:@"Status"] intValue];
    
    return self;
}


- (NSDictionary*) convertToDict
{
    return [NSDictionary dictionaryWithObjectsAndKeys:
            udn, @"Udn",
            room, @"Room",
            group, @"Group",
            name, @"Name",
            [NSNumber numberWithInt:status], @"Status", nil];
}


- (void) dealloc
{
    [udn release];
    [room release];
    [group release];
    [name release];
    [super dealloc];
}

@end



@implementation PrefSubnet

@synthesize address;
@synthesize name;


- (id) initWithDict:(NSDictionary*)aDict
{
    self = [super init];

    self.address = [[aDict objectForKey:@"Address"] unsignedIntValue];
    self.name = [aDict objectForKey:@"Name"];

    return self;
}


- (NSDictionary*) convertToDict
{
    return [NSDictionary dictionaryWithObjectsAndKeys:
            [NSNumber numberWithUnsignedInt:address], @"Address",
            name, @"Name",
            nil];
}


- (void) dealloc
{
    [name release];
    [super dealloc];
}

@end



static bool gPrefMulticastChannelSeeded = false;

@implementation PrefMulticastChannel


+ (uint32_t) generate
{
    // generate a new random channel - the channel is the last 2 bytes of the
    // multicast IP address 239.253.x.x
    if (!gPrefMulticastChannelSeeded)
    {
        srandom(time(NULL));
        gPrefMulticastChannelSeeded = true;
    }

    // man page for random() state the function returns an integer in range [0, 2^31 - 1]
    uint64_t maxRand = (((uint64_t)1)<<31) - 1;

    // generating a random number between [0,N] is (random() * (N+1) / (maxRand+1)) - if
    // we use (random() * N / maxRand), the random number will only generate N when
    // random() returns maxRand which is a chance of 1 in (2^31 -1)

    // byte1 in range [1,254]
    uint64_t byte1 = random();
    byte1 *= 254;
    byte1 /= maxRand + 1;
    byte1 += 1;

    // byte2 in range [1,254]
    uint64_t byte2 = random();
    byte2 *= 254;
    byte2 /= maxRand + 1;
    byte2 += 1;

    return (byte1 << 8) | byte2;
}

@end



@implementation Preferences


- (CFPropertyListRef) copyPrefValue:(NSString*)aName
{
    CFPreferencesAppSynchronize(CFSTR("uk.co.linn.Songcast"));
    return CFPreferencesCopyAppValue((CFStringRef)aName, CFSTR("uk.co.linn.Songcast"));
}


- (void) setPrefValue:(NSString*)aName value:(CFPropertyListRef)aValue
{
    CFPreferencesSetAppValue((CFStringRef)aName, aValue, CFSTR("uk.co.linn.Songcast"));
    CFPreferencesAppSynchronize(CFSTR("uk.co.linn.Songcast"));
}


- (void) postToApp:(NSString*)aName
{
    // post a notification from this pref app (uk.co.linn.songcast.prefs)
    CFNotificationCenterRef centre = CFNotificationCenterGetDistributedCenter();
    CFNotificationCenterPostNotification(centre, (CFStringRef)aName, CFSTR("uk.co.linn.songcast.prefs"), NULL, TRUE);
}


- (void) addAppObserver:(id)aObserver selector:(SEL)aSelector name:(NSString*)aName
{
    // listen for notification from the main app (uk.co.linn.songcast.app)
    NSDistributedNotificationCenter* centre = [NSDistributedNotificationCenter defaultCenter];
    [centre addObserver:aObserver selector:aSelector name:aName object:@"uk.co.linn.songcast.app"];
}


- (bool) getBoolPreference:(NSString*)aName default:(bool)aDefault
{
    CFPropertyListRef pref = [self copyPrefValue:aName];

    bool ret = aDefault;

    if (pref)
    {
        if (CFGetTypeID(pref) == CFBooleanGetTypeID())
        {
            ret = ((CFBooleanRef)pref == kCFBooleanTrue);
        }

        CFRelease(pref);
    }

    return ret;
}


- (void) setBoolPreference:(NSString*)aName value:(bool)aValue notification:(NSString*)aNotification
{
    // set the new preference value
    if (aValue)
    {
        [self setPrefValue:aName value:kCFBooleanTrue];
    }
    else
    {
        [self setPrefValue:aName value:kCFBooleanFalse];
    }

    // send notification that this has changed
    if (aNotification != nil)
    {
        [self postToApp:aNotification];
    }
}


- (int64_t) getIntegerPreference:(NSString*)aName default:(int64_t)aDefault
{
    CFPropertyListRef pref = [self copyPrefValue:aName];

    int64_t ret = aDefault;

    if (pref)
    {
        if (CFGetTypeID(pref) == CFNumberGetTypeID())
        {
            if (!CFNumberGetValue((CFNumberRef)pref, kCFNumberSInt64Type, &ret))
            {
                ret = aDefault;
            }
        }
        
        CFRelease(pref);
    }
    
    return ret;
}


- (void) setIntegerPreference:(NSString*)aName value:(int64_t)aValue notification:(NSString*)aNotification
{
    // set the new preference value
    CFNumberRef pref = CFNumberCreate(NULL, kCFNumberSInt64Type, &aValue);
    [self setPrefValue:aName value:pref];
    CFRelease(pref);

    // send notification that this has changed
    if (aNotification != nil)
    {
        [self postToApp:aNotification];
    }
}


- (NSArray*) getListPreference:(NSString*)aName itemType:(CFTypeID)aItemType
{
    // create a temporary mutable list to build the list from the preference
    NSMutableArray* list = [NSMutableArray arrayWithCapacity:0];

    CFPropertyListRef pref = [self copyPrefValue:aName];

    if (pref)
    {
        if (CFGetTypeID(pref) == CFArrayGetTypeID())
        {
            CFArrayRef prefList = (CFArrayRef)pref;

            for (CFIndex i=0 ; i<CFArrayGetCount(prefList) ; i++)
            {
                const void* item = CFArrayGetValueAtIndex(prefList, i);

                if (item && CFGetTypeID(item) == aItemType)
                {
                    [list addObject:(NSObject*)item];
                }
            }
        }

        CFRelease(pref);
    }

    // return a new immutable array of items
    return [NSArray arrayWithArray:list];
}


// Enabled

- (bool) enabled
{
    return [self getBoolPreference:@"Enabled" default:false];
}

- (void) addObserverEnabled:(id)aObserver selector:(SEL)aSelector
{
    [self addAppObserver:aObserver selector:aSelector name:@"PreferenceEnabledChanged"];
}


// MulticastEnabled

- (bool) multicastEnabled
{
    return [self getBoolPreference:@"MulticastEnabled" default:false];
}

- (void) setMulticastEnabled:(bool)aEnabled
{
    [self setBoolPreference:@"MulticastEnabled" value:aEnabled notification:@"PreferenceMulticastEnabledChanged"];
}


// MulticastChannel

- (uint32_t) multicastChannel
{
    uint32_t defaultChannel = [PrefMulticastChannel generate];
    uint32_t channel = (uint32_t)[self getIntegerPreference:@"MulticastChannel" default:defaultChannel];

    if (channel == defaultChannel)
    {
        // channel is not current stored - the default value was returned - so set the value in the
        // preferences file - if the channel is stored and is coincidentally the same as the generated
        // default, setting it again will do no harm
        [self setMulticastChannel:channel];
    }

    return channel;
}

- (void) setMulticastChannel:(uint32_t)aChannel
{
    [self setIntegerPreference:@"MulticastChannel" value:(int64_t)aChannel notification:@"PreferenceMulticastChannelChanged"];
}


// MusicLatencyMs

- (uint32_t) defaultMusicLatencyMs
{
    return 300;
}

- (uint32_t) musicLatencyMs
{
    return (uint32_t)[self getIntegerPreference:@"MusicLatencyMs" default:[self defaultMusicLatencyMs]];
}

- (void) setMusicLatencyMs:(uint32_t)aLatencyMs
{
    [self setIntegerPreference:@"MusicLatencyMs" value:(int64_t)aLatencyMs notification:@"PreferenceMusicLatencyMsChanged"];
}


// VideoLatencyMs

- (uint32_t) defaultVideoLatencyMs
{
    return 50;
}

- (uint32_t) videoLatencyMs
{
    return (uint32_t)[self getIntegerPreference:@"VideoLatencyMs" default:[self defaultVideoLatencyMs]];
}

- (void) setVideoLatencyMs:(uint32_t)aLatencyMs
{
    [self setIntegerPreference:@"VideoLatencyMs" value:(int64_t)aLatencyMs notification:@"PreferenceVideoLatencyMsChanged"];
}


// RotaryVolumeControl

- (bool) rotaryVolumeControl
{
    return [self getBoolPreference:@"RotaryVolumeControl" default:true];
}

- (void) setRotaryVolumeControl:(bool)aOverride
{
    [self setBoolPreference:@"RotaryVolumeControl" value:aOverride notification:@"PreferenceRotaryVolumeControlChanged"];
}


// AutoUpdatesEnabled

- (bool) autoUpdatesEnabled
{
    return [self getBoolPreference:@"AutoUpdatesEnabled" default:true];
}

- (void) setAutoUpdatesEnabled:(bool)aEnabled
{
    [self setBoolPreference:@"AutoUpdatesEnabled" value:aEnabled notification:@"PreferenceAutoUpdatesEnabledChanged"];
}

- (void) addObserverAutoUpdatesEnabled:(id)aObserver selector:(SEL)aSelector
{
    [self addAppObserver:aObserver selector:aSelector name:@"PreferenceAutoUpdatesEnabledChanged"];
}


// BetaUpdatesEnabled

- (bool) betaUpdatesEnabled
{
    return [self getBoolPreference:@"BetaUpdatesEnabled" default:false];
}


- (void) setBetaUpdatesEnabled:(bool)aEnabled
{
    [self setBoolPreference:@"BetaUpdatesEnabled" value:aEnabled notification:@"PreferenceBetaUpdatesEnabledChanged"];
}

// UsageDataEnabled

- (bool) usageDataEnabled
{
    return [self getBoolPreference:@"UsageDataEnabled" default:true];
}


- (void) setUsageDataEnabled:(bool)aEnabled
{
    [self setBoolPreference:@"UsageDataEnabled" value:aEnabled notification:@"PreferenceUsageDataEnabledChanged"];
}


// ReceiverList

- (NSArray*) receiverList
{
    NSMutableArray* receiverList = [NSMutableArray arrayWithCapacity:0];

    NSArray* dictList = [self getListPreference:@"ReceiverList" itemType:CFDictionaryGetTypeID()];

    for (NSDictionary* dict in dictList)
    {
        PrefReceiver* receiver = [[PrefReceiver alloc] initWithDict:dict];
        [receiverList addObject:receiver];
        [receiver release];
    }

    // return a new immutable array of receivers
    return [NSArray arrayWithArray:receiverList];
}

- (void) addObserverReceiverList:(id)aObserver selector:(SEL)aSelector
{
    [self addAppObserver:aObserver selector:aSelector name:@"PreferenceReceiverListChanged"];
}


// SelectedReceiverUdn

- (NSString*) selectedReceiverUdn
{
    NSString* value = nil;

    CFPropertyListRef pref = [self copyPrefValue:@"SelectedReceiverUdn"];

    if (pref)
    {
        if (CFGetTypeID(pref) == CFStringGetTypeID())
        {
            value = [NSString stringWithString:(NSString*)pref];
        }

        CFRelease(pref);
    }

    if (value != nil) {
        return value;
    }
    else {
        return @"";
    }
}

- (void) setSelectedReceiverUdn:(NSString*)aUdn
{
    // set the preference and flush
    [self setPrefValue:@"SelectedReceiverUdn" value:aUdn];
    
    // send notification that this has changed
    [self postToApp:@"PreferenceSelectedReceiverChanged"];
}

- (void) addObserverSelectedReceiverUdn:(id)aObserver selector:(SEL)aSelector
{
    [self addAppObserver:aObserver selector:aSelector name:@"PreferenceSelectedReceiverChanged"];
}


// SubnetList

- (NSArray*) subnetList
{
    NSMutableArray* subnetList = [NSMutableArray arrayWithCapacity:0];

    NSArray* dictList = [self getListPreference:@"SubnetList" itemType:CFDictionaryGetTypeID()];

    for (NSDictionary* dict in dictList)
    {
        PrefSubnet* subnet = [[PrefSubnet alloc] initWithDict:dict];
        [subnetList addObject:subnet];
        [subnet release];
    }

    // return a new immutable array of subnets
    return [NSArray arrayWithArray:subnetList];
}

- (void) addObserverSubnetList:(id)aObserver selector:(SEL)aSelector
{
    [self addAppObserver:aObserver selector:aSelector name:@"PreferenceSubnetListChanged"];
}


// SelectedSubnetAddress

- (uint32_t) selectedSubnetAddress
{
    return (uint32_t)[self getIntegerPreference:@"SelectedSubnetAddress" default:0];
}

- (void) setSelectedSubnetAddress:(uint32_t)aAddress
{
    [self setIntegerPreference:@"SelectedSubnetAddress" value:(int64_t)aAddress notification:@"PreferenceSelectedSubnetChanged"];
}

- (void) addObserverSelectedSubnet:(id)aObserver selector:(SEL)aSelector
{
    [self addAppObserver:aObserver selector:aSelector name:@"PreferenceSelectedSubnetChanged"];
}


// Event when all changed i.e. when the app starts up

- (void) addObserverAll:(id)aObserver selector:(SEL)aSelector
{
    [self addAppObserver:aObserver selector:aSelector name:@"PreferenceAllChanged"];
}


//


- (void) refreshReceiverList
{
    [self postToApp:@"RefreshReceiverList"];
}

- (void) checkForUpdates
{
    [self postToApp:@"CheckForUpdates"];
}


@end


