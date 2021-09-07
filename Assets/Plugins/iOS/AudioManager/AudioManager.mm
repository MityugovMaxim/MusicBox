#import <Foundation/Foundation.h>
#import <AVKit/AVKit.h>
#import <MediaPlayer/MediaPlayer.h>

#define MakeStringCopy( _x_ ) ( _x_ != NULL && [_x_ isKindOfClass:[NSString class]] ) ? strdup( [_x_ UTF8String] ) : NULL
#define GetStringParam( _x_ ) ( _x_ != NULL ) ? [NSString stringWithUTF8String:_x_] : [NSString stringWithUTF8String:""]

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
	
	AVAudioSession* session = [AVAudioSession sharedInstance];
	
	m_OutputCount = session.currentRoute.outputs.count;
	m_OutputUID   = session.currentRoute.outputs[0].UID;
	
	[session setActive:YES error:nil];
	[session setCategory:AVAudioSessionCategoryPlayback
		mode:AVAudioSessionModeMoviePlayback
		options:AVAudioSessionCategoryOptionInterruptSpokenAudioAndMixWithOthers
		error:nil];
	
	NSNotificationCenter* notificationCenter = [NSNotificationCenter defaultCenter];
	[notificationCenter addObserver:self
		selector:@selector(routeChangedHandler:)
		name:AVAudioSessionRouteChangeNotification
		object:nil];
	
	[notificationCenter addObserver:self
		selector:@selector(audioInterruptionHandler:)
		name:AVAudioSessionInterruptionNotification
		object:nil];
	
	return self;
}

- (void) routeChangedHandler:(NSNotification *) notification
{
	if ([[notification name] isEqualToString:AVAudioSessionRouteChangeNotification])
	{
		AVAudioSession* session = [AVAudioSession sharedInstance];
		
		NSUInteger outputCount = session.currentRoute.outputs.count;
		NSString*  outputUID   = session.currentRoute.outputs[0].UID;
		
		if (m_OutputCount == outputCount && [m_OutputUID isEqualToString:outputUID])
			return;
		
		m_OutputCount = outputCount;
		m_OutputUID   = outputUID;
		
		[session setActive:YES error:nil];
		[session setCategory:AVAudioSessionCategoryPlayback
			mode:AVAudioSessionModeMoviePlayback
			options:AVAudioSessionCategoryOptionInterruptSpokenAudioAndMixWithOthers
			error:nil];
		
		m_Callback();
	}
}

- (void) audioInterruptionHandler:(NSNotification *) notification
{
	if ([[notification name] isEqualToString:AVAudioSessionInterruptionNotification])
	{
		 NSInteger interruptionType = [[[notification userInfo] objectForKey: AVAudioSessionInterruptionTypeKey] integerValue];
		
		AVAudioSession* session = [AVAudioSession sharedInstance];
		
		switch (interruptionType)
		{
			case AVAudioSessionInterruptionTypeBegan:
				UnitySetAudioSessionActive(false);
				[session setActive:NO error:nil];
				break;
				
			case AVAudioSessionInterruptionTypeEnded:
				UnitySetAudioSessionActive(true);
				[session setActive:YES error:nil];
				[session setCategory:AVAudioSessionCategoryPlayback mode:AVAudioSessionModeMoviePlayback options:AVAudioSessionCategoryOptionInterruptSpokenAudioAndMixWithOthers error:nil];
				break;
		}
	}
}
@end

RouteObserver* m_RouteObserver;

extern "C"
{
	float AudioManager_GetInputLatency()
	{
		return AVAudioSession.sharedInstance.IOBufferDuration + AVAudioSession.sharedInstance.inputLatency;
	}

	float AudioManager_GetOutputLatency()
	{
		return AVAudioSession.sharedInstance.IOBufferDuration + AVAudioSession.sharedInstance.outputLatency;
	}

	const char* AudioManager_GetOutputName()
	{
		NSString* portName = AVAudioSession.sharedInstance.currentRoute.outputs[0].portName;
		
		return MakeStringCopy(portName);
	}

	const char* AudioManager_GetOutputUID()
	{
		NSString* portUID = AVAudioSession.sharedInstance.currentRoute.outputs[0].UID;
		
		return MakeStringCopy(portUID);
	}

	bool AudioManager_IsOutputWireless()
	{
		NSString* portType = AVAudioSession.sharedInstance.currentRoute.outputs[0].portType;
		
		if ([portType isEqualToString: AVAudioSessionPortBluetoothLE])
			return true;
		
		if ([portType isEqualToString: AVAudioSessionPortBluetoothA2DP])
			return true;
		
		return false;
	}

	void AudioManager_EnableAudio()
	{
		AVAudioSession* session = [AVAudioSession sharedInstance];
		
		UnitySetAudioSessionActive(true);
		[session setActive:YES error:nil];
		[session setCategory:AVAudioSessionCategoryPlayback
			mode:AVAudioSessionModeMoviePlayback
			options:AVAudioSessionCategoryOptionInterruptSpokenAudioAndMixWithOthers
			error:nil];
	}

	void AudioManager_DisableAudio()
	{
		UnitySetAudioSessionActive(false);
		[AVAudioSession.sharedInstance setActive:NO error:nil];
	}

	void AudioManager_UnregisterRemoteCommands()
	{
		[m_RouteObserver remove];
	}

	void AudioManager_RegisterRemoteCommands(
		CommandHandler _PlayHandler,
		CommandHandler _PauseHandler,
		CommandHandler _NextTrackHandler,
		CommandHandler _PreviousTrackHandler,
		CommandHandler _SourceChanged
	)
	{
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
}
