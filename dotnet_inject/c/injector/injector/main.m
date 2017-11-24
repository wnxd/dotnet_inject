//
//  main.m
//  injector
//
//  Created by wnxd on 2017/11/24.
//  Copyright © 2017年 wnxd. All rights reserved.
//

#import <Foundation/Foundation.h>
#import <dlfcn.h>

typedef bool (*dotnet_inject)(int pid, const char* lib);

int main(int argc, const char * argv[]) {
    @autoreleasepool {
        printf("enter pid: ");
        int pid;
        scanf("%i", &pid);
        printf("enter lib: ");
        char lib[256];
        scanf("%s", lib);
        void* handle = dlopen("dotnet_inject.dylib", RTLD_NOW);
        dotnet_inject func = dlsym(handle, "dotnet_inject");
        bool r = func(pid, lib);
        printf("inject: %d", r);
    }
    return 0;
}
