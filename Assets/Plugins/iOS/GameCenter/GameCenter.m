//
//  GameCenter.m
//  GameCenter
//
//  Created by m.mityugov on 21.07.2021.
//

#import <Foundation/Foundation.h>
#import <GameKit/GameKit.h>

void GameCenterAuth()
{
    GKLocalPlayer *localPlayer = [GKLocalPlayer localPlayer];

        if(localPlayer.authenticated == NO)
        {
            [localPlayer setAuthenticateHandler:(^(UIViewController* viewcontroller, NSError *error) {
                if (error)
                {
                    NSLog(@"Error occured");
                }
                else if (viewcontroller)
                {
                    NSLog(@"Need to log in");
                    //AppDelegate *appDelegate = (AppDelegate*)[[UIApplication sharedApplication] delegate];
                    //[appDelegate.window.rootViewController presentViewController:viewcontroller animated:YES completion:nil];
                }
                else
                {
                    NSLog(@"Success");

                }
            })];

        }
}
