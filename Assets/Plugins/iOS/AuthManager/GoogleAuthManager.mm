#import "GoogleSignIn.h"
#import "UnityBridge.h"
#import <UnityAppController.h>
#import <objc/runtime.h>

typedef void (*GoogleAuthSuccessCallback)(const char* idToken, const char* accessToken);
typedef void (*GoogleAuthFailedCallback)(const char* error);

@interface UnityAppController (GoogleAppController)
- (BOOL)application:(UIApplication*) application openURL:(NSURL*) url options:(NSDictionary<NSString*, id>*) options;
@end

@implementation UnityAppController (GoogleController)
+ (void) load
{
    Method origin;
    Method target;
    origin = class_getInstanceMethod(self, @selector(application:openURL:options:));
    target = class_getInstanceMethod(self, @selector(GoogleAppController:openURL:options:));
    method_exchangeImplementations(origin, target);
}

- (BOOL)GoogleAppController:(UIApplication*)application openURL:(NSURL*)url options:(NSDictionary *)options
{
    BOOL handled = false;
    
    handled |= [self GoogleAppController:application openURL:url options:options];
    
    handled |= [[GIDSignIn sharedInstance] handleURL:url];
    
    return handled;
}
@end

@interface GoogleAuthManager : NSObject
- (void)signInWithSuccessCallback:(GoogleAuthSuccessCallback)_Succes failedCallback:(GoogleAuthFailedCallback) _Failed;
@end

@implementation GoogleAuthManager
- (void)signInWithSuccessCallback:(GoogleAuthSuccessCallback)_Success failedCallback:(GoogleAuthFailedCallback) _Failed
{
    NSString* path = [[NSBundle mainBundle] pathForResource:@"GoogleService-Info" ofType:@"plist"];
    NSString* clientID = [[[NSDictionary alloc] initWithContentsOfFile:path] valueForKey:@"CLIENT_ID"];
    
    GIDConfiguration* config = [[GIDConfiguration alloc] initWithClientID:clientID];
    
    [[GIDSignIn sharedInstance] signInWithConfiguration:config
                               presentingViewController:UnityGetGLViewController()
                                               callback:^(GIDGoogleUser * _Nullable user, NSError * _Nullable error) {
        if (user == nil)
        {
            if (_Failed != nil)
                _Failed(CString(@"[GoogleAuthManager] Sign in failed. User is null."));
        }
        else if (user.authentication == nil)
        {
            if (_Failed != nil)
                _Failed(CString(@"[GoogleAuthManager] Sign in failed. User authentication is null."));
        }
        else if (error != nil)
        {
            if (_Failed != nil)
                _Failed(CString(@"[GoogleAuthManager] Sign in failed. Error occurred."));
        }
        else
        {
            if (_Success != nil)
                _Success(CString(user.authentication.idToken), CString(user.authentication.accessToken));
        }
    }];
}
@end

extern "C"
{
    void GoogleAuthManager_Login(GoogleAuthSuccessCallback _Success, GoogleAuthFailedCallback _Failed)
    {
        [[GoogleAuthManager alloc] signInWithSuccessCallback:_Success failedCallback:_Failed];
    }
}
