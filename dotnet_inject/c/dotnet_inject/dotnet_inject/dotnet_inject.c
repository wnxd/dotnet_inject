#include <dlfcn.h>
#include <pthread.h>
#include <syslog.h>
#include <Carbon/Carbon.h>
#include "dotnet_inject.h"
#include "mach_inject.h"

typedef struct
{
    ptrdiff_t offset;
    void* data;
} info;

typedef void (*dotnet_load)(const char* path, const char* lib);

void EventLoopTimerEntry(EventLoopTimerRef inTimer, char* param)
{
    size_t plen = *((size_t*)param);
    param += sizeof(size_t);
    char path[256];
    memcpy(path, param, plen);
    path[plen] = '\0';
    char name[256];
    memcpy(name, param, plen);
    name[plen] = '\0';
    param += plen;
    size_t llen = *((size_t*)param);
    param += sizeof(size_t);
    char lib[256];
    memcpy(lib, param, llen);
    lib[llen] = '\0';
    strcat(name, "dotnet_helper.dylib");
    syslog(LOG_NOTICE, "%s | %s", path, lib);
    void* helper = dlopen(name, RTLD_NOW);
    if (helper != NULL)
    {
        dotnet_load load = dlsym(helper, "dotnet_load");
        if (load != NULL)
            load(path, lib);
    }
}

void* pthread_entry(info* param)
{
    EventLoopTimerProcPtr proc = (EventLoopTimerProcPtr) EventLoopTimerEntry;
    proc += param->offset;
    EventLoopTimerUPP upp = NewEventLoopTimerUPP(proc);
    InstallEventLoopTimer(GetMainEventLoop(), 0, 0, upp, (void*)param->data, NULL);
    free(param);
    return NULL;
}

void inject_init(ptrdiff_t codeOffset, void *paramBlock, size_t paramSize, void* dummy_pthread_struct)
{
#if defined (__i386__) || defined(__x86_64__)
    // On intel, per-pthread data is a zone of data that must be allocated.
    // if not, all function trying to access per-pthread data (all mig functions for instance)
    // will crash.
#if __MAC_OS_X_VERSION_MIN_REQUIRED >= 101200
    // on macOS Serria, should use _pthread_set_self from libSystem.B.dylb
    extern void _pthread_set_self(char*);
    _pthread_set_self(dummy_pthread_struct);
#else
    extern void __pthread_set_self(char*);
    __pthread_set_self(dummy_pthread_struct);
#endif
#endif
    pthread_attr_t attr;
    pthread_attr_init(&attr);
    int policy;
    pthread_attr_getschedpolicy(&attr, &policy);
    pthread_attr_setdetachstate(&attr, PTHREAD_CREATE_DETACHED);
    pthread_attr_setinheritsched(&attr, PTHREAD_EXPLICIT_SCHED);
    struct sched_param sched;
    sched.sched_priority = sched_get_priority_max(policy);
    pthread_attr_setschedparam(&attr, &sched);
    pthread_t thread;
    info* p = malloc(sizeof(info));
    p->offset = codeOffset;
    p->data = paramBlock;
    pthread_create(&thread, &attr, (void* (*)(void*))((long)pthread_entry+codeOffset), p);
    pthread_attr_destroy(&attr);
    thread_suspend(mach_thread_self());
}

bool dotnet_inject(int pid, const char* lib)
{
    char path[256];
    getcwd(path, sizeof(path));
    size_t plen = strlen(path);
    path[plen] = '/';
    plen++;
    path[plen] = '\0';
    size_t llen = strlen(lib);
    size_t size = sizeof(size_t)+sizeof(size_t)+plen+llen;
    char* data = malloc(size);
    char* c = data;
    *((size_t*)c) = plen;
    c += sizeof(size_t);
    memcpy(c, path, plen);
    c += plen;
    *((size_t*)c) = llen;
    c += sizeof(size_t);
    memcpy(c, lib, llen);
    mach_error_t err =  mach_inject(inject_init, data, size, pid, 0);
    return err == err_none;
}
