#import <Foundation/Foundation.h>
#import <stdlib.h>
#import "UnityInterface.h" // optional

// Import Swift header dynamically so you don't hardcode project name
#import "UnityFramework/UnityFramework-Swift.h"

static AccCloudKitManager* _ckMgr = nil;

static char* strdup_from_nsstring(NSString* s) {
    if (!s) return NULL;
    const char* utf = [s UTF8String];
    if (!utf) return NULL;
    char* dup = strdup(utf);
    return dup;
}

extern "C" {

    const char* _GetSavedEmail() {
        @autoreleasepool {
            if (!_ckMgr) _ckMgr = [[AccCloudKitManager alloc] init];
            NSString* email = [_ckMgr getCachedEmail];
            if (!email) return NULL;
            char* out = strdup_from_nsstring(email);
            return out; // caller will free via _FreeCString
        }
    }

    void _FreeCString(const char* ptr) {
        if (ptr) free((void*)ptr);
    }

    void _SaveEmailNative(const char* cEmail) {
        @autoreleasepool {
            if (!_ckMgr) _ckMgr = [[AccCloudKitManager alloc] init];
            NSString* email = cEmail ? [NSString stringWithUTF8String:cEmail] : @"";
            [_ckMgr saveEmail:email];
        }
    }

    // Optional: request CloudKit fetch now (async)
    /*void _FetchCloudKitAndCache() {
        @autoreleasepool {
            if (!_ckMgr) _ckMgr = [[AccCloudKitManager alloc] init];
            [_ckMgr fetchFromCloudKitAndCache];
        }
    }*/
    
    void saveFID(const char* cEmail) {
        @autoreleasepool {
            if (!_ckMgr) _ckMgr = [[AccCloudKitManager alloc] init];
            NSString* email = cEmail ? [NSString stringWithUTF8String:cEmail] : @"";
            [_ckMgr saveEmail:email];
        }
    }

    typedef void (*LoadFIDCallback)(const char*);

    void loadFID(LoadFIDCallback callback) {
        @autoreleasepool {
            if (!_ckMgr) _ckMgr = [[AccCloudKitManager alloc] init];
            
            [_ckMgr fetchFromCloudKitAndCacheWithCompletion:^(NSString* email) {
                const char* cstr = email ? strdup([email UTF8String]) : NULL;
                if (callback) callback(cstr);
                if (cstr) free((void*)cstr);
            }];
        }
    }
}
