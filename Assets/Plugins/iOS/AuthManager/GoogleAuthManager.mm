#import "GoogleSignIn.h"
#import "UnityBridge.h"
#import <UnityAppController.h>
#import <objc/runtime.h>

typedef void (*GoogleAuthSuccessCallback)(const char* idToken, const char* accessToken);
typedef void (*GoogleAuthCanceledCallback)();
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
- (void)signInWithSuccessCallback:(GoogleAuthSuccessCallback) _Succes canceledCallback:(GoogleAuthCanceledCallback) _Canceled failedCallback:(GoogleAuthFailedCallback) _Failed;
@end

@implementation GoogleAuthManager
- (void)signInWithSuccessCallback:(GoogleAuthSuccessCallback) _Success canceledCallback:(GoogleAuthCanceledCallback) _Canceled failedCallback:(GoogleAuthFailedCallback) _Failed
{
    NSString* path = [[NSBundle mainBundle] pathForResource:@"GoogleService-Info" ofType:@"plist"];
    NSString* clientID = [[[NSDictionary alloc] initWithContentsOfFile:path] valueForKey:@"CLIENT_ID"];
    
    GIDConfiguration* config = [[GIDConfiguration alloc] initWithClientID:clientID];
    
    [[GIDSignIn sharedInstance] signInWithConfiguration:config
                               presentingViewController:UnityGetGLViewController()
                                               callback:^(GIDGoogleUser * _Nullable user, NSError * _Nullable error) {
        if (error != nil)
        {
            switch (error.code)
            {
                case kGIDSignInErrorCodeKeychain:
                    _Failed(CString(@"Keychain access denied."));
                    break;
                case kGIDSignInErrorCodeHasNoAuthInKeychain:
                    _Failed(CString(@"There are no valid auth tokens in the keychain."));
                    break;
                case kGIDSignInErrorCodeCanceled:
                    _Canceled();
                    break;
                case kGIDSignInErrorCodeEMM:
                    _Failed(CString(@"Enterprise Mobility Management related error has occurred."));
                    break;
                case kGIDSignInErrorCodeNoCurrentUser:
                    _Failed(CString(@"There is no current user."));
                    break;
                case kGIDSignInErrorCodeScopesAlreadyGranted:
                    _Failed(CString(@"Requested scopes have already been granted."));
                    break;
                default:
                    _Failed(CString(@"Unknown error."));
                    break;
            }
        }
        else if (user == nil)
        {
            if (_Failed != nil)
                _Failed(CString(@"User is null."));
        }
        else if (user.authentication == nil)
        {
            if (_Failed != nil)
                _Failed(CString(@"User authentication is null."));
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
    void GoogleAuthManager_Login(GoogleAuthSuccessCallback _Success, GoogleAuthCanceledCallback _Canceled, GoogleAuthFailedCallback _Failed)
    {
        [[GoogleAuthManager alloc] signInWithSuccessCallback:_Success canceledCallback:_Canceled failedCallback:_Failed];
    }
}
