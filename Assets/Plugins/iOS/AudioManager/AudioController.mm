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
	
	[session setActive:YES error:nil];
	[session setCategory:AVAudioSessionCategoryPlayback
		mode:AVAudioSessionModeSpokenAudio
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
		
		NSUInteger reason = [[[notification userInfo] objectForKey:AVAudioSessionRouteChangeReasonKey] unsignedIntValue];
		
		switch (reason)
		{
			case AVAudioSessionRouteChangeReasonUnknown:
				NSLog(@"[AudioManager] Route change. Reason: Unknown.");
				break;
			case AVAudioSessionRouteChangeReasonOverride:
				NSLog(@"[AudioManager] Route change. Reason: Override.");
				break;
			case AVAudioSessionRouteChangeReasonCategoryChange:
				NSLog(@"[AudioManager] Route change. Reason: Category change. Category: %@", [session category]);
				break;
			case AVAudioSessionRouteChangeReasonWakeFromSleep:
				NSLog(@"[AudioManager] Route change. Reason: Wake from sleep.");
				break;
			case AVAudioSessionRouteChangeReasonNewDeviceAvailable:
				NSLog(@"[AudioManager] Route change. Reason: New device available.");
				m_Callback();
				break;
			case AVAudioSessionRouteChangeReasonOldDeviceUnavailable:
				NSLog(@"[AudioManager] Route change. Reason: Old device unavailable.");
				m_Callback();
				break;
			case AVAudioSessionRouteChangeReasonRouteConfigurationChange:
				NSLog(@"[AudioManager] Route change. Reason: Route configuration change.");
				break;
			case AVAudioSessionRouteChangeReasonNoSuitableRouteForCategory:
				NSLog(@"[AudioManager] Route change. Reason: No suitable route for category.");
				break;
		}
		
		if ([session category] != AVAudioSessionCategoryPlayback)
		{
			[session setCategory:AVAudioSessionCategoryPlayback
				mode:AVAudioSessionModeSpokenAudio
				options:AVAudioSessionCategoryOptionInterruptSpokenAudioAndMixWithOthers
				error:nil];
		}
	}
}

- (void) audioInterruptionHandler:(NSNotification *) notification
{
	if ([[notification name] isEqualToString:AVAudioSessionInterruptionNotification])
	{
		 NSUInteger type = [[[notification userInfo] objectForKey: AVAudioSessionInterruptionTypeKey] unsignedIntValue];
		
		AVAudioSession* session = [AVAudioSession sharedInstance];
		
		switch (type)
		{
			case AVAudioSessionInterruptionTypeBegan:
				NSLog(@"[AudioManager] Interruption type: Began");
				break;
				
			case AVAudioSessionInterruptionTypeEnded:
				NSLog(@"[AudioManager] Interruption type: Ended");
				[session setCategory:AVAudioSessionCategoryPlayback mode:AVAudioSessionModeSpokenAudio options:AVAudioSessionCategoryOptionInterruptSpokenAudioAndMixWithOthers error:nil];
				break;
		}
	}
}
@end

RouteObserver* m_RouteObserver;

extern "C"
{
	void AudioController_Register(CommandHandler _AudioSourceChanged)
	{
		if (m_RouteObserver != NULL)
			[m_RouteObserver remove];
		
		m_RouteObserver = [[RouteObserver new] init:_AudioSourceChanged];
	}

	void AudioController_Unregister()
	{
		if (m_RouteObserver == NULL)
			return;
		
		[m_RouteObserver remove];
	}

	const char* AudioController_GetOutputName()
	{
		NSString* portName = AVAudioSession.sharedInstance.currentRoute.outputs[0].portName;
		
		return MakeStringCopy(portName);
	}

	const char* AudioController_GetOutputID()
	{
		NSString* portUID = AVAudioSession.sharedInstance.currentRoute.outputs[0].UID;
		
		return MakeStringCopy(portUID);
	}

	int AudioController_GetOutputType()
	{
		// 0 - BuiltIn
		// 1 - Headphones
		// 2 - Bluetooth
		// 3 - Unknown
		
		NSString* portType = AVAudioSession.sharedInstance.currentRoute.outputs[0].portType;
		
		if ([portType isEqualToString: AVAudioSessionPortBuiltInSpeaker])
			return 0;
		
		if ([portType isEqualToString: AVAudioSessionPortHeadphones])
			return 1;
		
		if ([portType isEqualToString: AVAudioSessionPortBluetoothLE])
			return 2;
		
		if ([portType isEqualToString: AVAudioSessionPortBluetoothA2DP])
			return 2;
		
		return 3;
	}

	void AudioController_EnableAudio()
	{
		AVAudioSession* session = [AVAudioSession sharedInstance];
		[session setCategory:AVAudioSessionCategoryPlayback
			mode:AVAudioSessionModeSpokenAudio
			options:AVAudioSessionCategoryOptionInterruptSpokenAudioAndMixWithOthers
			error:nil];
		UnitySetAudioSessionActive(true);
	}

	void AudioController_DisableAudio()
	{
		UnitySetAudioSessionActive(false);
	}
}
