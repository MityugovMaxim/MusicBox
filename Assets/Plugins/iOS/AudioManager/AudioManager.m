#import <Foundation/Foundation.h>
#import <AVKit/AVKit.h>
#import <MediaPlayer/MediaPlayer.h>

typedef void (*CommandHandler)();

@interface RouteObserver : NSObject
@end

@implementation RouteObserver

CommandHandler m_Callback;

- (void) remove
{
    [[NSNotificationCenter defaultCenter] removeObserver:self];
}

- (id) init:(CommandHandler) callback
{
    self = [super init];
    
    if (!self)
        return nil;
    
    m_Callback = callback;

    [[NSNotificationCenter defaultCenter] addObserver:self
        selector:@selector(routeChangedHandler:)
        name:AVAudioSessionRouteChangeNotification
        object:nil];

    return self;
}

- (void) routeChangedHandler:(NSNotification *) notification
{
    if ([[notification name] isEqualToString:AVAudioSessionRouteChangeNotification])
        m_Callback();
}
@end

RouteObserver* m_RouteObserver;

float GetInputLatency()
{
    return AVAudioSession.sharedInstance.IOBufferDuration + AVAudioSession.sharedInstance.inputLatency;
}

float GetOutputLatency()
{
    return AVAudioSession.sharedInstance.IOBufferDuration + AVAudioSession.sharedInstance.outputLatency;
}

void EnableAudio()
{
    [AVAudioSession.sharedInstance setCategory:AVAudioSessionCategoryPlayback error:nil];
    
    UnitySetAudioSessionActive(1);
}

void DisableAudio()
{
    [AVAudioSession.sharedInstance setCategory:AVAudioSessionCategoryPlayback error:nil];
    
    UnitySetAudioSessionActive(0);
}

void UnregisterRemoteCommands()
{
    [m_RouteObserver remove];
}

void RegisterRemoteCommands(
    CommandHandler _PlayHandler,
    CommandHandler _PauseHandler,
    CommandHandler _NextTrackHandler,
    CommandHandler _PreviousTrackHandler,
    CommandHandler _SourceChanged
)
{
    [AVAudioSession.sharedInstance setCategory:AVAudioSessionCategoryPlayback error:nil];
    
    m_RouteObserver = [[RouteObserver new] init:_SourceChanged];
    
    MPRemoteCommandCenter *commandCenter = [MPRemoteCommandCenter sharedCommandCenter];

    [commandCenter.playCommand addTargetWithHandler:^MPRemoteCommandHandlerStatus(MPRemoteCommandEvent* _Nonnull event)
     {
        _PlayHandler();
        return MPRemoteCommandHandlerStatusSuccess;
    }];
    
    [commandCenter.pauseCommand addTargetWithHandler:^MPRemoteCommandHandlerStatus(MPRemoteCommandEvent* _Nonnull event)
     {
        _PauseHandler();
        return MPRemoteCommandHandlerStatusSuccess;
    }];
    
    [commandCenter.nextTrackCommand addTargetWithHandler:^MPRemoteCommandHandlerStatus(MPRemoteCommandEvent* _Nonnull event)
     {
        _NextTrackHandler();
        return MPRemoteCommandHandlerStatusSuccess;
    }];
    
    [commandCenter.previousTrackCommand addTargetWithHandler:^MPRemoteCommandHandlerStatus(MPRemoteCommandEvent* _Nonnull event)
     {
        _PauseHandler();
        return MPRemoteCommandHandlerStatusSuccess;
    }];
}
