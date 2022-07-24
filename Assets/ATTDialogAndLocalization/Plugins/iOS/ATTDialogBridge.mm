#import "ATTDialog.h"

static const char* StringCopy(const char* string);

const char* StringCopy(const char* string)
{
    if (string == NULL) {
        return NULL;
    }
    
    char* res = (char*)malloc(strlen(string) + 1);
    strcpy(res, string);
    return res;
}

#if defined (__cplusplus)
extern "C" {
#endif
    
    void AdapterCallATTDialog(ATTCompleteCallback onATTComplete)
    {
        ATTDialog *instance = [[ATTDialog alloc] init];
        [instance CheckStatus:onATTComplete];
    }
    

#if defined (__cplusplus)
}
#endif
