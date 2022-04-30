#import <Foundation/Foundation.h>
#import <MobileCoreServices/MobileCoreServices.h>
#import <UnityBridge.h>

typedef void (*SelectSuccess)(const char* _URL);
typedef void (*SelectCanceled)();
typedef void (*SelectFailed)(const char* _Error);

@interface FileManager : NSObject
+ (void)selectFileWithExtension:(NSString*)extension withSuccess:(SelectSuccess)success withFailed:(SelectFailed)failed withCanceled:(SelectCanceled)canceled;
+ (NSString*)convertExtensionToUTI:(NSString *)extension;
@end

@implementation FileManager
static UIDocumentPickerViewController* filePicker;
static SelectSuccess selectSuccess;
static SelectFailed selectFailed;
static SelectCanceled selectCanceled;

+ (void)selectFileWithExtension:(NSString*)extension withSuccess:(SelectSuccess)success withFailed:(SelectFailed)failed withCanceled:(SelectCanceled)canceled
{
    selectSuccess = success;
    selectFailed = failed;
    selectCanceled = canceled;
    NSString* type = [self convertExtensionToUTI:extension];
    NSArray<NSString*>* types = [[NSArray alloc] initWithObjects:type, nil];
    filePicker = [[UIDocumentPickerViewController alloc] initWithDocumentTypes:types inMode:UIDocumentPickerModeImport];
    filePicker.delegate = (id)self;
    
    if (@available(iOS 13.0, *))
        filePicker.shouldShowFileExtensions = YES;
    
    [UnityGetGLViewController() presentViewController:filePicker animated:YES completion:NULL];
}

+ (NSString*)convertExtensionToUTI:(NSString*)extension
{
    NSString* UTI = (__bridge_transfer NSString*)UTTypeCreatePreferredIdentifierForTag(
        kUTTagClassFilenameExtension,
        (__bridge CFStringRef)extension,
        NULL
    );
    return UTI;
}

+ (void)documentPicker:(UIDocumentPickerViewController*)controller didPickDocumentAtURL:(NSURL*)url
{
    [self documentPickerCompleted:controller documents:@[url]];
}

+ (void)documentPicker:(UIDocumentPickerViewController*)controller didPickDocumentsAtURLs:(NSArray<NSURL*>*)urls
{
    [self documentPickerCompleted:controller documents:urls];
}

+ (void)documentPickerCompleted:(UIDocumentPickerViewController*)controller documents:(NSArray<NSURL*>*)urls
{
    filePicker = nil;
    
    if(controller.documentPickerMode == UIDocumentPickerModeImport)
    {
        NSString* filePath = [urls count] > 0 ? urls[0].path : @"";
        if (selectSuccess != nil)
            selectSuccess(CString(filePath));
    }
    
    [controller dismissViewControllerAnimated:YES completion:nil];
}

+ (void)documentPickerWasCancelled:(UIDocumentPickerViewController*) controller
{
    filePicker = nil;
    
    if (selectCanceled != nil)
        selectCanceled();
    
    [controller dismissViewControllerAnimated:YES completion:nil];
}
@end

extern "C"
{
    void SelectFile(const char* _Extension, SelectSuccess _Success, SelectFailed _Failed, SelectCanceled _Canceled)
    {
        NSString* extension = GetString(_Extension);
        
        [FileManager selectFileWithExtension:extension withSuccess:_Success withFailed:_Failed withCanceled:_Canceled];
    }
}
