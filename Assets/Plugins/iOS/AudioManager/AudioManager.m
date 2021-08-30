#import <Foundation/Foundation.h>
#import <AVKit/AVKit.h>
#import <MediaPlayer/MediaPlayer.h>

typedef void (*CommandHandler)();

@interface RouteObserver : NSObject
@end

@implementation RouteObserver

CommandHandler m_Callback;
NSUInteger     m_OutputCount;
NSString*      m_OutputUID;

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
    
    m_OutputCount = AVAudioSession.sharedInstance.currentRoute.outputs.count;
    m_OutputUID   = AVAudioSession.sharedInstance.currentRoute.outputs[0].UID;

    [[NSNotificationCenter defaultCenter] addObserver:self
        selector:@selector(routeChangedHandler:)
        name:AVAudioSessionRouteChangeNotification
        object:nil];

    return self;
}

- (void) routeChangedHandler:(NSNotification *) notification
{
    if ([[notification name] isEqualToString:AVAudioSessionRouteChangeNotification])
    {
        NSUInteger outputCount = AVAudioSession.sharedInstance.currentRoute.outputs.count;
        NSString*  outputUID   = AVAudioSession.sharedInstance.currentRoute.outputs[0].UID;
        
        if (m_OutputCount == outputCount && [m_OutputUID isEqualToString:outputUID])
            return;
        
        m_OutputCount = outputCount;
        m_OutputUID   = outputUID;
        
        m_Callback();
    }
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
    [AVAudioSession.sharedInstance setActive:YES error:nil];
    [AVAudioSession.sharedInstance setCategory:AVAudioSessionCategoryPlayback error:nil];
    [AVAudioSession.sharedInstance setMode:AVAudioSessionModeMoviePlayback error:nil];
    
    UnitySetAudioSessionActive(true);
}

void DisableAudio()
{
    [AVAudioSession.sharedInstance setActive:NO error:nil];
    
    UnitySetAudioSessionActive(false);
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
    [AVAudioSession.sharedInstance setMode:AVAudioSessionModeMoviePlayback error:nil];
    
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
        _PreviousTrackHandler();
        return MPRemoteCommandHandlerStatusSuccess;
    }];
}
