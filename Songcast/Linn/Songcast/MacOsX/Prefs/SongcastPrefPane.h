
#import <PreferencePanes/PreferencePanes.h>
#import "Preferences.h"


// Receiver class for data displayed in the table view
@interface Receiver : NSObject<NSCopying>
{
    NSString* udn;
    NSString* title;
    EReceiverState status;
}

@property (copy) NSString* udn;
@property (copy) NSString* title;
@property (assign) EReceiverState status;

- (id) initWithPref:(PrefReceiver*)aPref uniqueInRoom:(bool)aUnique;

@end


// Main class for the preference pane
@interface SongcastPrefPane : NSPreferencePane 
{
    IBOutlet NSButton* buttonAutoUpdates;
    IBOutlet NSButton* buttonBeta;
    IBOutlet NSPopUpButton* buttonNetworkAdapter;
    IBOutlet NSPopUpButton* buttonReceiver;
    IBOutlet NSMatrix* buttonVolumeControl;
    IBOutlet NSMatrix* buttonSongcastMode;
    IBOutlet NSImageView* imageReceiverStatus;
    IBOutlet NSTextField* textAbout;
    IBOutlet NSTextField* textDescription;
    IBOutlet NSTextField* textMulticastChannel;
    IBOutlet NSTextField* textMusicLatencyMs;
    IBOutlet NSTextField* textReceiverStatus;
    IBOutlet NSTextField* textVideoLatencyMs;

    Preferences* iPreferences;
    NSArray* iReceiverList;
    NSArray* iSubnetList;
    NSInteger iSelectedReceiverIdx;
}

@property (assign) NSButton* buttonAutoUpdates;
@property (assign) NSButton* buttonBeta;
@property (assign) NSPopUpButton* buttonNetworkAdapter;
@property (assign) NSPopUpButton* buttonReceiver;
@property (assign) NSMatrix* buttonVolumeControl;
@property (assign) NSMatrix* buttonSongcastMode;
@property (assign) NSImageView* imageReceiverStatus;
@property (assign) NSTextField* textAbout;
@property (assign) NSTextField* textDescription;
@property (assign) NSTextField* textMulticastChannel;
@property (assign) NSTextField* textMusicLatencyMs;
@property (assign) NSTextField* textReceiverStatus;
@property (assign) NSTextField* textVideoLatencyMs;

- (void) mainViewDidLoad;
- (IBAction) buttonAutoUpdatesClicked:(id)aSender;
- (IBAction) buttonBetaClicked:(id)aSender;
- (IBAction) buttonCheckForUpdatesClicked:(id)aSender;
- (IBAction) buttonHelpClicked:(id)aSender;
- (IBAction) buttonMulticastChannelClicked:(id)aSender;
- (IBAction) buttonMusicLatencyDefaultClicked:(id)aSender;
- (IBAction) buttonNetworkAdapterClicked:(id)aSender;
- (IBAction) buttonReceiverClicked:(id)aSender;
- (IBAction) buttonRefreshClicked:(id)aSender;
- (IBAction) buttonSongcastModeClicked:(id)aSender;
- (IBAction) buttonVideoLatencyDefaultClicked:(id)aSender;
- (IBAction) buttonVolumeControlClicked:(id)aSender;
- (IBAction) textMusicLatencyChanged:(id)aSender;
- (IBAction) textVideoLatencyChanged:(id)aSender;
- (void) preferenceAutoUpdatesEnabledChanged:(NSNotification*)aNotification;
- (void) preferenceReceiverListChanged:(NSNotification*)aNotification;
- (void) preferenceSubnetListChanged:(NSNotification*)aNotification;
- (void) preferenceAllChanged:(NSNotification*)aNotification;
- (void) updateReceiverStatus:(Receiver*)aSelected;

@end



