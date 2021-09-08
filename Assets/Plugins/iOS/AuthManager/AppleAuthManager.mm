#import <Foundation/Foundation.h>
#import <AuthenticationServices/AuthenticationServices.h>
#import <CommonCrypto/CommonCrypto.h>

#define MakeStringCopy( _x_ ) ( _x_ != NULL && [_x_ isKindOfClass:[NSString class]] ) ? strdup( [_x_ UTF8String] ) : NULL
#define GetStringParam( _x_ ) ( _x_ != NULL ) ? [NSString stringWithUTF8String:_x_] : [NSString stringWithUTF8String:""]

typedef void (*AppleAuthCallback)(const char* tokenID);

API_AVAILABLE(ios(13.0))
@interface AppleAuthManager : NSObject <ASAuthorizationControllerDelegate>
@property (nonatomic, assign) AppleAuthCallback loginSuccess;
@property (nonatomic, assign) AppleAuthCallback loginFailed;
@end

@implementation AppleAuthManager
+ (instancetype) sharedInstance
{
    static AppleAuthManager* defaultInstance = nil;
    static dispatch_once_t defaultInstanceInitialization;
    
    dispatch_once(&defaultInstanceInitialization, ^{
        defaultInstance = [[AppleAuthManager alloc] init];
    });
    
    return defaultInstance;
}

- (instancetype) init
{
    self = [super init];
    return self;
}

- (void) loginWithAppleWithNonce:(NSString*)nonce
{
    ASAuthorizationAppleIDProvider* provider = [[ASAuthorizationAppleIDProvider alloc] init];
    ASAuthorizationAppleIDRequest* request = [provider createRequest];
    
    [request setRequestedScopes:@[ASAuthorizationScopeFullName, ASAuthorizationScopeEmail]];
    [request setNonce: [self stringBySha256HashingString:nonce]];
    
    ASAuthorizationController* authController = [[ASAuthorizationController alloc] initWithAuthorizationRequests:@[request]];
    
    [authController setDelegate:self];
    [authController performRequests];
}

- (void) authorizationController:(ASAuthorizationController*)controller didCompleteWithAuthorization:(ASAuthorization*)authorization
{
    if ([[authorization credential] isKindOfClass:[ASAuthorizationAppleIDCredential class]])
    {
        ASAuthorizationAppleIDCredential* credential = (ASAuthorizationAppleIDCredential*)[authorization credential];
        
        NSString* identityToken = [[NSString alloc] initWithData:[credential identityToken] encoding:NSUTF8StringEncoding];
        
        if (self.loginSuccess != nil)
            self.loginSuccess(MakeStringCopy(identityToken));
    }
}

- (void) authorizationController:(ASAuthorizationController*)controller didCompleteWithError:(NSError*)error
{
    NSString* message = [error localizedDescription];
    
    if (self.loginFailed != nil)
        self.loginFailed(MakeStringCopy(message));
}

- (NSString *)stringBySha256HashingString:(NSString *)input
{
    const char *string = [input UTF8String];
    unsigned char result[CC_SHA256_DIGEST_LENGTH];
    CC_SHA256(string, (CC_LONG)strlen(string), result);

    NSMutableString *hashed = [NSMutableString stringWithCapacity:CC_SHA256_DIGEST_LENGTH * 2];
    for (NSInteger i = 0; i < CC_SHA256_DIGEST_LENGTH; i++)
        [hashed appendFormat:@"%02x", result[i]];
    return hashed;
}
@end

extern "C"
{
    void AppleAuthManager_Login(const char* _Nonce, AppleAuthCallback _Success, AppleAuthCallback _Failed)
    {
        if (@available(iOS 13.0, *))
        {
            NSString* nonce = GetStringParam(_Nonce);
            
            AppleAuthManager* authManager = [AppleAuthManager sharedInstance];
            [authManager setLoginSuccess:_Success];
            [authManager setLoginFailed:_Failed];
            [authManager loginWithAppleWithNonce:nonce];
        }
        else
        {
            if (_Failed != nil)
                _Failed("Sign in with Apple not supported")
        }
    }
}
