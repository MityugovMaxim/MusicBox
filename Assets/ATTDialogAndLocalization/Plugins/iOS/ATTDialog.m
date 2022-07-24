//------------------------------------------------------------------------------
// Copyright (c) 2021 Azur Games Company.
// All Right Reserved.
//------------------------------------------------------------------------------


#import <AppTrackingTransparency/AppTrackingTransparency.h>
#import <AdSupport/AdSupport.h>

#import "ATTDialog.h"


@implementation ATTDialog

-(void)ShowDialogWithCompletition:(ATTCompleteCallback) onATTComplete {
    __block int attStatus = 2;
    if (@available(iOS 14, *)) {
        [ATTrackingManager requestTrackingAuthorizationWithCompletionHandler:^(ATTrackingManagerAuthorizationStatus status) {
            
            switch (status) {
                case ATTrackingManagerAuthorizationStatusNotDetermined:
                    // The user has not yet received an authorization request to authorize access to app-related data that can be used for tracking the user or the device.
                    attStatus = 0;
                    break;
                case ATTrackingManagerAuthorizationStatusAuthorized:
                    // The user authorizes access to app-related data that can be used for tracking the user or the device.
                    attStatus = 1;
                    break;
                case ATTrackingManagerAuthorizationStatusDenied:
                    // The user denies authorization to access app-related data that can be used for tracking the user or the device.
                    attStatus = 2;
                    break;
                case ATTrackingManagerAuthorizationStatusRestricted:
                    // The authorization to access app-related data that can be used for tracking the user or the device is restricted.
                    attStatus = 3;
                    break;
            }
            onATTComplete(attStatus);
        }];
    } else {
        attStatus = 1;
        onATTComplete(attStatus);
    }
    
    
     
}

-(void) CheckStatus:(ATTCompleteCallback) onATTComplete
{
    int attStatus = 2;

    if (@available(iOS 14, *)) {
        ATTrackingManagerAuthorizationStatus status = [ATTrackingManager trackingAuthorizationStatus];
        switch (status) {
            case ATTrackingManagerAuthorizationStatusNotDetermined:
                attStatus = 0;
                [self ShowDialogWithCompletition: onATTComplete];
                // The user has not yet received an authorization request to authorize access to app-related data that can be used for tracking the user or the device.
                break;
            case ATTrackingManagerAuthorizationStatusAuthorized:
                // The user authorizes access to app-related data that can be used for tracking the user or the device.
                attStatus = 1;
                break;
            case ATTrackingManagerAuthorizationStatusDenied:
                // The user denies authorization to access app-related data that can be used for tracking the user or the device.
                attStatus = 2;
                break;
            case ATTrackingManagerAuthorizationStatusRestricted:
                // The authorization to access app-related data that can be used for tracking the user or the device is restricted.
                attStatus = 3;
                break;
        }
    } else {
        attStatus = 1;
    }
    
    if (attStatus != 0)
    {
        onATTComplete(attStatus);
    }
     
}


@end
