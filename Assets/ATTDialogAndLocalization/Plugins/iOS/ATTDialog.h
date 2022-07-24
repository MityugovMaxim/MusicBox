//
//  ATTrackingDialog.h
//  UnityFramework
//
//  Created by Vladimir Kuzmin on 28.07.2021.
//

typedef void(*ATTCompleteCallback)(int status);

@interface ATTDialog : NSObject

-(void) CheckStatus:(ATTCompleteCallback) onATTComplete;
-(void)ShowDialogWithCompletition:(ATTCompleteCallback) onATTComplete;
@end

