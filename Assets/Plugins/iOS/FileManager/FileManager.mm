#import <Foundation/Foundation.h>
#import <MobileCoreServices/MobileCoreServices.h>
#import <UnityBridge.h>

typedef void (*SelectSuccess)(const char* _URL);
typedef void (*SelectCanceled)();
typedef void (*SelectFailed)(const char* _Error);

@interface FileManager : NSObject
+ (void)selectFileWithExtensions:(NSArray<NSString*>*)UTIs withSuccess:(SelectSuccess)success withFailed:(SelectFailed)failed withCanceled:(SelectCanceled)canceled;
+ (NSString*)convertExtensionToUTI:(NSString *)extension;
@end

@implementation FileManager
static UIDocumentPickerViewController* filePicker;
static SelectSuccess selectSuccess;
static SelectFailed selectFailed;
static SelectCanceled selectCanceled;

+ (void)selectFileWithExtensions:(NSArray<NSString*>*)UTIs withSuccess:(SelectSuccess)success withFailed:(SelectFailed)failed withCanceled:(SelectCanceled)canceled
{
    selectSuccess = success;
    selectFailed = failed;
    selectCanceled = canceled;
    filePicker = [[UIDocumentPickerViewController alloc] initWithDocumentTypes:UTIs inMode:UIDocumentPickerModeImport];
    filePicker.delegate = (id)self;
    
    if (@available(iOS 13.0, *))
        filePicker.shouldShowFileExtensions = YES;
    filePicker.allowsMultipleSelection = NO;
    
    [UnityGetGLViewController() presentViewController:filePicker animated:YES completion:NULL];
}

+ (NSString*) convertExtensionToUTI:(NSString*) extension
{
    CFStringRef data = UTTypeCreatePreferredIdentifierForTag(kUTTagClassFilenameExtension, (__bridge CFStringRef) extension, NULL);
    NSString* UTI = (__bridge NSString*)data;
    CFRelease(data);
    return UTI;
}

+ (void) documentPicker:(UIDocumentPickerViewController*)controller didPickDocumentAtURL:(NSURL*)url
{
    [self documentPickerCompleted:controller documents:@[url]];
}

+ (void) documentPicker:(UIDocumentPickerViewController*)controller didPickDocumentsAtURLs:(NSArray<NSURL*>*)urls
{
    [self documentPickerCompleted:controller documents:urls];
}

+ (void) documentPickerCompleted:(UIDocumentPickerViewController*)controller documents:(NSArray<NSURL*>*)urls
{
    NSString* filePath = [urls count] > 0 ? urls[0].path : @"";
    
    if (selectSuccess != nil)
        selectSuccess(CString(filePath));
    
    selectSuccess = nil;
    selectFailed = nil;
    selectCanceled = nil;
    
    filePicker = nil;
    
    [controller dismissViewControllerAnimated:YES completion:nil];
}

+ (void) documentPickerWasCancelled:(UIDocumentPickerViewController*) controller
{
    if (selectCanceled != nil)
        selectCanceled();
    
    selectSuccess = nil;
    selectFailed = nil;
    selectCanceled = nil;
    
    filePicker = nil;
    
    [controller dismissViewControllerAnimated:YES completion:nil];
}
@end

extern "C"
{
    void SelectFile(const char* _Extensions[], int _Count, SelectSuccess _Success, SelectFailed _Failed, SelectCanceled _Canceled)
    {
        NSMutableArray* UTIs = [[NSMutableArray alloc] initWithCapacity:_Count];
        for (int i = 0; i < _Count; i++)
        {
            NSString* extension = GetString(_Extensions[i]);
            NSString* UTI = [FileManager convertExtensionToUTI:extension];
            [UTIs addObject:UTI];
        }
        [FileManager selectFileWithExtensions:UTIs withSuccess:_Success withFailed:_Failed withCanceled:_Canceled];
    }
}
