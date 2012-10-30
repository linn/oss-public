
#import "SongcastPrefPane.h"


// Class for storing static resources used by table cell
@interface CellResources : NSObject
{
}

+ (void) loadResources:(NSBundle*)aBundle;
+ (NSImage*) imageConnected;
+ (NSImage*) imageDisconnected;
+ (NSString*) textConnected;
+ (NSString*) textConnecting;
+ (NSString*) textDisconnected;
+ (NSString*) textUnavailable;
+ (NSString*) textSongcastOff;

@end



// Implementation of preference pane
@implementation SongcastPrefPane

@synthesize buttonAutoUpdates;
@synthesize buttonBeta;
@synthesize buttonUsageData;
@synthesize buttonNetworkAdapter;
@synthesize buttonReceiver;
@synthesize buttonVolumeControl;
@synthesize buttonSongcastMode;
@synthesize imageReceiverStatus;
@synthesize textAbout;
@synthesize textDescription;
@synthesize textMulticastChannel;
@synthesize textMusicLatencyMs;
@synthesize textReceiverStatus;
@synthesize textVideoLatencyMs;


- (void) mainViewDidLoad
{
    // get the bundle name from the info.plist
    NSString* appName = [[[self bundle] infoDictionary] objectForKey:@"CFBundleName"];

    [CellResources loadResources:[self bundle]];

    // initialise the text for the UI elements
    [textDescription setStringValue:[NSString stringWithFormat:[textDescription stringValue], appName]];

    // initialise the about text
    NSString* aboutFormat = [[[self bundle] infoDictionary] objectForKey:@"SongcastAboutText"];
    NSString* version = [[[self bundle] infoDictionary] objectForKey:@"CFBundleShortVersionString"];
    NSString* copyright = [[[self bundle] infoDictionary] objectForKey:@"NSHumanReadableCopyright"];
    [textAbout setStringValue:[NSString stringWithFormat:aboutFormat, appName, version, copyright]];
    
    // create the preferences object
    iPreferences = [[Preferences alloc] init];    
    
    // register for notifications from app
    [iPreferences addObserverEnabled:self selector:@selector(preferenceEnabledChanged:)];
    [iPreferences addObserverAutoUpdatesEnabled:self selector:@selector(preferenceAutoUpdatesEnabledChanged:)];
    [iPreferences addObserverReceiverList:self selector:@selector(preferenceReceiverListChanged:)];
    [iPreferences addObserverSelectedReceiverUdn:self selector:@selector(preferenceReceiverListChanged:)];
    [iPreferences addObserverSubnetList:self selector:@selector(preferenceSubnetListChanged:)];
    [iPreferences addObserverSelectedSubnet:self selector:@selector(preferenceSubnetListChanged:)];
    [iPreferences addObserverAll:self selector:@selector(preferenceAllChanged:)];

    // initialise all elements of the UI
    [self preferenceAllChanged:nil];
}


- (IBAction) buttonAutoUpdatesClicked:(id)aSender
{
    [iPreferences setAutoUpdatesEnabled:([buttonAutoUpdates state] == NSOnState)];
}


- (IBAction) buttonBetaClicked:(id)aSender
{
    [iPreferences setBetaUpdatesEnabled:([buttonBeta state] == NSOnState)];
}

- (IBAction) buttonUsageDataClicked:(id)aSender
{
    [iPreferences setUsageDataEnabled:([buttonUsageData state] == NSOnState)];
}


- (IBAction) buttonCheckForUpdatesClicked:(id)aSender
{
    [iPreferences checkForUpdates];
}


- (IBAction) buttonHelpClicked:(id)aSender
{
    NSURL* manualUrl = [NSURL URLWithString:[[[self bundle] infoDictionary] objectForKey:@"SongcastManualUrl"]];
    [[NSWorkspace sharedWorkspace] openURL:manualUrl];
}


- (IBAction) buttonMulticastChannelClicked:(id)aSender
{
    // generate a new random channel - the channel is the last 2 bytes of the
    // multicast IP address 239.253.x.x
    uint32_t channel = [PrefMulticastChannel generate];

    // set preference and update UI
    [iPreferences setMulticastChannel:channel];
    [textMulticastChannel setIntegerValue:channel];
}


- (IBAction) buttonMusicLatencyDefaultClicked:(id)aSender
{
    // set the preference value
    [iPreferences setMusicLatencyMs:[iPreferences defaultMusicLatencyMs]];

    // update the UI
    [textMusicLatencyMs setIntegerValue:[iPreferences musicLatencyMs]];
}


- (IBAction) buttonNetworkAdapterClicked:(id)aSender
{
    // get index of selected item
    NSInteger selected = [buttonNetworkAdapter indexOfSelectedItem];

    if (selected >= 0)
    {
        PrefSubnet* subnet = [iSubnetList objectAtIndex:selected];
        [iPreferences setSelectedSubnetAddress:[subnet address]];
    }
}


- (IBAction) buttonReceiverClicked:(id)aSender
{
    // get index of selected item
    iSelectedReceiverIdx = [buttonReceiver indexOfSelectedItem];

    // update the preferences and the UI
    if (iSelectedReceiverIdx != -1)
    {
        Receiver* receiver = [iReceiverList objectAtIndex:iSelectedReceiverIdx];
        [iPreferences setSelectedReceiverUdn:[receiver udn]];
        [self updateReceiverStatus:receiver];
    }
    else
    {
        [iPreferences setSelectedReceiverUdn:@""];
        [self updateReceiverStatus:nil];
    }
}


- (IBAction) buttonRefreshClicked:(id)aSender
{
    [iPreferences refreshReceiverList];
}


- (IBAction) buttonSongcastModeClicked:(id)aSender
{
    // get the selected radio button coordinates
    NSInteger row, column;
    [buttonSongcastMode getRow:&row column:&column ofCell:[buttonSongcastMode selectedCell]];

    // set the preference
    [iPreferences setMulticastEnabled:(column == 1)];
}


- (IBAction) buttonVideoLatencyDefaultClicked:(id)aSender
{
    // set the preference value
    [iPreferences setVideoLatencyMs:[iPreferences defaultVideoLatencyMs]];

    // update the UI
    [textVideoLatencyMs setIntegerValue:[iPreferences videoLatencyMs]];
}


- (IBAction) buttonVolumeControlClicked:(id)aSender
{
    // get the selected radio button coordinates
    NSInteger row, column;
    [buttonVolumeControl getRow:&row column:&column ofCell:[buttonVolumeControl selectedCell]];
    
    // set the preference
    [iPreferences setRotaryVolumeControl:(column == 0)];
}


- (IBAction) textMusicLatencyChanged:(id)aSender
{
    // get value
    uint64_t latencyMs = [textMusicLatencyMs integerValue];

    // set preference
    [iPreferences setMusicLatencyMs:latencyMs];
}


- (IBAction) textVideoLatencyChanged:(id)aSender
{
    // get value
    uint64_t latencyMs = [textVideoLatencyMs integerValue];

    // set preference
    [iPreferences setVideoLatencyMs:latencyMs];
}


- (void) preferenceEnabledChanged:(NSNotification*)aNotification
{
    // update the UI
    Receiver* receiver = (iSelectedReceiverIdx != -1) ? [iReceiverList objectAtIndex:iSelectedReceiverIdx] : nil;
    [self updateReceiverStatus:receiver];
}


- (void) preferenceAutoUpdatesEnabledChanged:(NSNotification*)aNotification
{
    [buttonAutoUpdates setState:([iPreferences autoUpdatesEnabled] ? NSOnState : NSOffState)];
}


- (void) preferenceReceiverListChanged:(NSNotification*)aNotification
{
    // sort the list of receivers by room, group order
    NSSortDescriptor* roomSorter = [NSSortDescriptor sortDescriptorWithKey:@"room" ascending:YES selector:@selector(caseInsensitiveCompare:)];
    NSSortDescriptor* groupSorter = [NSSortDescriptor sortDescriptorWithKey:@"group" ascending:YES selector:@selector(caseInsensitiveCompare:)];
    NSArray* sorters = [NSArray arrayWithObjects:roomSorter, groupSorter, nil];
    NSArray* sorted = [[iPreferences receiverList] sortedArrayUsingDescriptors:sorters];

    // create the list of receiver objects to display
    NSMutableArray* receivers = [NSMutableArray arrayWithCapacity:0];

    // loop over all receivers to find blocks of receivers that are in the same room
    uint roomStart = 0;
    while (roomStart < [sorted count])
    {
        // calculate the start and end indices of the next contiguous block of receivers in the same room
        NSString* thisRoom = [[sorted objectAtIndex:roomStart] room];

        uint roomEnd = roomStart + 1;

        while (roomEnd < [sorted count] && [[[sorted objectAtIndex:roomEnd] room] compare:thisRoom] == NSOrderedSame)
        {
            roomEnd++;
        }

        // receivers in index range [roomStart, roomEnd-1] have the same room
        if (roomEnd - roomStart == 1)
        {
            // this is the only receiver in this room
            PrefReceiver* prefRecv = [sorted objectAtIndex:roomStart];
            Receiver* recv = [[Receiver alloc] initWithPref:prefRecv title:[prefRecv room]];
            [receivers addObject:recv];
            [recv release];
            roomStart = roomEnd;
            continue;
        }

        // now loop over all receivers in this room to find blocks of receivers with the same group
        uint groupStart = roomStart;
        while (groupStart < roomEnd)
        {
            // calculate the start and end indices of the next contiguous block of receivers in the same room and group
            NSString* thisGroup = [[sorted objectAtIndex:groupStart] group];

            uint groupEnd = groupStart + 1;

            while (groupEnd < roomEnd && [[[sorted objectAtIndex:groupEnd] group] compare:thisGroup] == NSOrderedSame)
            {
                groupEnd++;
            }

            // the first receiver in a block of receivers with the same room and group is displayed as "room (group)" and
            // subsequent ones are displayed as "room (group) 2" etc...
            for (uint i=groupStart ; i<groupEnd ; i++)
            {
                PrefReceiver* prefRecv = [sorted objectAtIndex:i];

                Receiver* recv = nil;

                if (i == groupStart)
                {
                    recv = [[Receiver alloc] initWithPref:prefRecv title:[NSString stringWithFormat:@"%@ (%@)", [prefRecv room], [prefRecv group]]];
                }
                else
                {
                    recv = [[Receiver alloc] initWithPref:prefRecv title:[NSString stringWithFormat:@"%@ (%@) %d", [prefRecv room], [prefRecv group], i-groupStart+1]];
                }

                [receivers addObject:recv];
                [recv release];
            }

            groupStart = groupEnd;
        }

        roomStart = roomEnd;
    }

    if (iReceiverList)
    {
        [iReceiverList release];
    }
    iReceiverList = [[NSArray alloc] initWithArray:receivers];

    // get the index of the selected receiver
    iSelectedReceiverIdx = -1;

    NSString* selectedUdn = [iPreferences selectedReceiverUdn];
    for (NSInteger i=0 ; i<[iReceiverList count] ; i++)
    {
        if ([selectedUdn compare:[[iReceiverList objectAtIndex:i] udn]] == NSOrderedSame)
        {
            iSelectedReceiverIdx = i;
            break;
        }
    }

    // update the PopUp button for the list of receivers
    [buttonReceiver removeAllItems];

    for (Receiver* receiver in iReceiverList)
    {
        [buttonReceiver addItemWithTitle: [receiver title]];
    }

    [buttonReceiver selectItemAtIndex:iSelectedReceiverIdx];

    if (iSelectedReceiverIdx != -1)
    {
        [self updateReceiverStatus:[iReceiverList objectAtIndex:iSelectedReceiverIdx]];
    }
    else
    {
        [self updateReceiverStatus:nil];
    }
}


- (void) preferenceSubnetListChanged:(NSNotification*)aNotification
{
    // get the latest subnet list
    if (iSubnetList)
    {
        [iSubnetList release];
    }
    iSubnetList = [[iPreferences subnetList] retain];

    // get the latest selected subnet
    uint32_t selected = [iPreferences selectedSubnetAddress];

    // update UI
    [buttonNetworkAdapter removeAllItems];

    for (PrefSubnet* subnet in iSubnetList)
    {
        uint32_t address = [subnet address];
        uint32_t byte1 = address & 0xff;
        uint32_t byte2 = (address >> 8) & 0xff;
        uint32_t byte3 = (address >> 16) & 0xff;
        uint32_t byte4 = (address >> 24) & 0xff;

        NSString* title = [NSString stringWithFormat:@"%d.%d.%d.%d (%@)", byte1, byte2, byte3, byte4, [subnet name]];
        [buttonNetworkAdapter addItemWithTitle:title];

        if ([subnet address] == selected)
        {
            [buttonNetworkAdapter selectItemAtIndex:[buttonNetworkAdapter numberOfItems]-1];
        }
    }

    if ([buttonNetworkAdapter numberOfItems] == 0) {
        [buttonNetworkAdapter selectItemAtIndex:-1];
    }
}


- (void) preferenceAllChanged:(NSNotification*)aNotification
{
    // general
    [self preferenceReceiverListChanged:nil];
    [buttonVolumeControl selectCellAtRow:0 column:([iPreferences rotaryVolumeControl] ? 0 : 1)];

    // advanced->network
    [self preferenceSubnetListChanged:nil];
    [buttonSongcastMode selectCellAtRow:0 column:([iPreferences multicastEnabled] ? 1 : 0)];
    [textMulticastChannel setIntegerValue:[iPreferences multicastChannel]];
    [textMusicLatencyMs setIntegerValue:[iPreferences musicLatencyMs]];
    [textVideoLatencyMs setIntegerValue:[iPreferences videoLatencyMs]];

    // advanced->updates
    [self preferenceAutoUpdatesEnabledChanged:nil];
    [buttonBeta setState:([iPreferences betaUpdatesEnabled] ? NSOnState : NSOffState)];
    [buttonUsageData setState:([iPreferences usageDataEnabled] ? NSOnState : NSOffState)];
}


- (void) updateReceiverStatus:(Receiver*)aSelected
{
    if ([iPreferences enabled])
    {
        if (aSelected != nil)
        {
            // songcast is on and a receiver is selected
            switch ([aSelected status])
            {
            default:
            case eReceiverStateOffline:
                [textReceiverStatus setStringValue:[CellResources textUnavailable]];
                [imageReceiverStatus setImage:[CellResources imageDisconnected]];
                break;

            case eReceiverStateDisconnected:
                [textReceiverStatus setStringValue:[CellResources textDisconnected]];
                [imageReceiverStatus setImage:[CellResources imageDisconnected]];
                break;

            case eReceiverStateConnecting:
                [textReceiverStatus setStringValue:[CellResources textConnecting]];
                [imageReceiverStatus setImage:[CellResources imageDisconnected]];
                break;

            case eReceiverStateConnected:
                [textReceiverStatus setStringValue:[CellResources textConnected]];
                [imageReceiverStatus setImage:[CellResources imageConnected]];
                break;
            }

        }
        else
        {
            // songcast is on but no receiver is selected
            [textReceiverStatus setStringValue:nil];
            [imageReceiverStatus setImage:nil];
        }
    }
    else
    {
        // songcast is switched off
        [textReceiverStatus setStringValue:[CellResources textSongcastOff]];
        [imageReceiverStatus setImage:[CellResources imageDisconnected]];
    }
}


@end



// Receiver class for data displayed in the table view
@implementation Receiver

@synthesize udn;
@synthesize title;
@synthesize status;


- (id) initWithPref:(PrefReceiver*)aPref title:(NSString*)aTitle
{
    self = [super init];

    self.udn = [aPref udn];
    self.status = [aPref status];
    self.title = aTitle;

    return self;
}


- (id) copyWithZone:(NSZone*)aZone
{
    Receiver* copy = [[Receiver alloc] init];
    [copy setUdn:udn];
    [copy setTitle:title];
    [copy setStatus:status];
    return copy;    
}


- (void) dealloc
{
    [udn release];
    [title release];
    [super dealloc];
}

@end



// Implementation of cell resources
@implementation CellResources

static NSImage* imageConnected;
static NSImage* imageDisconnected;
static NSString* textConnected;
static NSString* textConnecting;
static NSString* textDisconnected;
static NSString* textUnavailable;
static NSString* textSongcastOff;

+ (void) loadResources:(NSBundle*)aBundle
{
    imageConnected = [[NSImage alloc] initWithContentsOfFile:[aBundle pathForResource:@"Green" ofType:@"png"]];
    imageDisconnected = [[NSImage alloc] initWithContentsOfFile:[aBundle pathForResource:@"Red" ofType:@"png"]];

    textConnected = @"Connected";
    textConnecting = @"Connecting";
    textDisconnected = @"Disconnected";
    textUnavailable = @"Unavailable";
    textSongcastOff = @"Songcast Off";
}

+ (NSImage*) imageConnected
{
    return imageConnected;
}

+ (NSImage*) imageDisconnected
{
    return imageDisconnected;
}

+ (NSString*) textConnected
{
    return textConnected;
}

+ (NSString*) textConnecting
{
    return textConnecting;
}

+ (NSString*) textDisconnected
{
    return textDisconnected;
}

+ (NSString*) textUnavailable
{
    return textUnavailable;
}

+ (NSString*) textSongcastOff
{
    return textSongcastOff;
}

@end



