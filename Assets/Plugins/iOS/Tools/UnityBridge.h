#import <Foundation/Foundation.h>

#define CString(_String) (_String != NULL && [_String isKindOfClass:[NSString class]]) ? strdup([_String UTF8String]) : NULL
#define GetString(_String) (_String != NULL) ? [NSString stringWithUTF8String:_String] : [NSString stringWithUTF8String:""]
