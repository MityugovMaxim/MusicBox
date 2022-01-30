#import "UnityBridge.h"
#import <Foundation/Foundation.h>
#import <AuthenticationServices/AuthenticationServices.h>
#import <CommonCrypto/CommonCrypto.h>

typedef void (*AppleAuthSuccessCallback)(const char* idToken, const char* nonce, const char* displayName);
typedef void (*AppleAuthFailedCallback)(const char* error);

@interface AppleAuthManager : NSObject
+ (instancetype) sharedManager;
@end

API_AVAILABLE(ios(13.0))
@interface AppleAuthManager () <ASAuthorizationControllerDelegate, ASAuthorizationControllerPresentationContextProviding>
@end

@implementation AppleAuthManager
NSString* nonce;
AppleAuthSuccessCallback success;
AppleAuthFailedCallback failed;

+ (instancetype) sharedManager
{
    static AppleAuthManager *_defaultManager = nil;
    static dispatch_once_t defaultManagerInitialization;
    
    dispatch_once(&defaultManagerInitialization, ^{
        _defaultManager = [[AppleAuthManager alloc] init];
    });
    
    return _defaultManager;
}

- (instancetype) init
{
    self = [super init];
    
    return self;
}

- (void) signInWithSuccessCallback:(AppleAuthSuccessCallback) _Success failedCallback:(AppleAuthFailedCallback) _Failed API_AVAILABLE(ios(13.0))
{
    nonce = [self generateNonceWithLength:32];
    success = _Success;
    failed = _Failed;
    
    ASAuthorizationAppleIDProvider* provider = [[ASAuthorizationAppleIDProvider alloc] init];
    ASAuthorizationAppleIDRequest* request = [provider createRequest];
    
    request.requestedScopes = @[ASAuthorizationScopeFullName, ASAuthorizationScopeEmail];
    request.nonce = [self generateHashWithString:nonce];
    
    ASAuthorizationController* authController = [[ASAuthorizationController alloc] initWithAuthorizationRequests:@[request]];
    
    [authController setDelegate:self];
    [authController setPresentationContextProvider:self];
    [authController performRequests];
}

- (void)authorizationController:(ASAuthorizationController *)controller
   didCompleteWithAuthorization:(ASAuthorization *)authorization API_AVAILABLE(ios(13.0))
{
    if ([[authorization credential] isKindOfClass:[ASAuthorizationAppleIDCredential class]])
    {
        ASAuthorizationAppleIDCredential* credential = (ASAuthorizationAppleIDCredential*)[authorization credential];
        
        NSString* idToken = [[NSString alloc] initWithData:[credential identityToken] encoding:NSUTF8StringEncoding];
        
        NSString* displayName = [NSString stringWithFormat:@"%@ %@", credential.fullName.givenName, credential.fullName.familyName];
        
        [self invokeSuccessWithIDToken:idToken nonce:nonce displayName:displayName];
    }
}

- (void) authorizationController:(ASAuthorizationController*) controller didCompleteWithError:(NSError*) error API_AVAILABLE(ios(13.0))
{
    [self invokeFailedWithError:error];
}

- (ASPresentationAnchor) presentationAnchorForAuthorizationController:(ASAuthorizationController *)controller API_AVAILABLE(ios(13.0))
{
    return UnityGetMainWindow();
}

- (void) invokeSuccessWithIDToken:(NSString*) idToken nonce:(NSString*) nonce displayName:(NSString*)displayName
{
    dispatch_async(dispatch_get_main_queue(), ^{
        if (success != nil)
            success(CString(idToken), CString(nonce), CString(displayName));
    });
}

- (void) invokeFailedWithError:(NSError*) error
{
    dispatch_async(dispatch_get_main_queue(), ^{
        if (failed != nil)
            failed(CString(error.localizedDescription));
    });
}

- (NSString*) generateNonceWithLength:(NSInteger) length
{
  NSAssert(length > 0, @"[AppleAuthManager] Generate nonce failed. Nonce length must be greater than zero.");
  
  NSString* charset = @"0123456789ABCDEFGHIJKLMNOPQRSTUVXYZabcdefghijklmnopqrstuvwxyz-._";
  NSMutableString* nonce = [NSMutableString string];
  
  NSInteger index = length;
  while (index > 0)
  {
    NSMutableArray* buffer = [NSMutableArray arrayWithCapacity:16];
    for (NSInteger i = 0; i < 16; i++)
    {
      uint8_t random = 0;
      int errorCode = SecRandomCopyBytes(kSecRandomDefault, 1, &random);
      NSAssert(errorCode == errSecSuccess, @"[AppleAuthManager] Generate nonce failed. Unable to generate nonce: OSStatus %i", errorCode);
      [buffer addObject:@(random)];
    }

    for (NSNumber* value in buffer)
    {
      if (index == 0)
        break;

      if (value.unsignedIntValue < charset.length)
      {
        unichar character = [charset characterAtIndex:value.unsignedIntValue];
        [nonce appendFormat:@"%C", character];
        index--;
      }
    }
  }

  return [nonce copy];
}

- (NSString*) generateHashWithString:(NSString*) input
{
    const char* string = [input UTF8String];
    unsigned char result[CC_SHA256_DIGEST_LENGTH];
    CC_SHA256(string, (CC_LONG)strlen(string), result);

    NSMutableString* hash = [NSMutableString stringWithCapacity:CC_SHA256_DIGEST_LENGTH * 2];
    for (NSInteger i = 0; i < CC_SHA256_DIGEST_LENGTH; i++)
        [hash appendFormat:@"%02x", result[i]];
    
    return hash;
}
@end

extern "C"
{
    void AppleAuthManager_Login(AppleAuthSuccessCallback _Success, AppleAuthFailedCallback _Failed)
    {
        if (@available(iOS 13.0, *))
        {
            [[AppleAuthManager sharedManager] signInWithSuccessCallback:_Success failedCallback:_Failed];
        }
        else
        {
            dispatch_async(dispatch_get_main_queue(), ^{
                if (failed != nil)
                    failed(CString(@"Sign in with Apple is not supported."));
            });
        }
    }
}
