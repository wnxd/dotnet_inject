#import <mach-o/dyld.h>
#import <dlfcn.h>
#import <Foundation/Foundation.h>
#import <xamarin/xamarin.h>
#import "dotnet_helper.h"

#define xamarin_lib "/Library/Frameworks/Xamarin.Mac.framework/Versions/Current/lib/mono/Xamarin.Mac/"

extern void _Z23xamarin_set_is_mkbundleb(bool value);
extern void mono_assemblies_init(void);

MonoDomain* domain = NULL;
bool xammac_init = false;
bool self_init = false;
void (*_mono_assemblies_init)(void) = mono_assemblies_init;
MonoAssembly* (*_mono_assembly_open)(const char*, MonoImageOpenStatus*) = mono_assembly_open;
MonoImage* (*_mono_assembly_get_image)(MonoAssembly*) = mono_assembly_get_image;
MonoClass* (*_mono_class_from_name)(MonoImage*, const char*, const char*) = mono_class_from_name;
MonoMethod* (*_mono_class_get_method_from_name)(MonoClass*, const char*, int) = mono_class_get_method_from_name;
MonoObject* (*_mono_runtime_invoke)(MonoMethod*, void*, void**, MonoObject**) = mono_runtime_invoke;

int push_env(const char* variable, const char* value)
{
    size_t len = strlen(value);
    const char* current;
    int rv;
    if ((current = getenv(variable)) && *current) {
        char* buf = (char*)malloc (len+strlen(current)+2);
        memcpy(buf, value, len);
        buf[len] = ':';
        strcpy (buf+len+1, current);
        rv = setenv(variable, buf, 1);
        free (buf);
    } else
        rv = setenv (variable, value, 1);
    return rv;
}

void xamarin_init(const char* path)
{
    char cpath[256];
    getcwd(cpath, 256);
    chdir(path);
    _Z23xamarin_set_is_mkbundleb(true);
    xamarin_set_is_unified(true);
    xamarin_initialize();
    xammac_init = true;
    chdir(cpath);
    NSLog(@"xamarin_init");
}

__attribute__((constructor))
void dotnet_init()
{
    NSLog(@"init");
    void* mod = dlopen(_dyld_get_image_name(0), RTLD_LAZY);
    NSLog(@"%p %s", mod, _dyld_get_image_name(0));
    void* func = dlsym(mod, "mono_assembly_open");
    NSLog(@"mono_assembly_open: %p", func);
    if (func != NULL)
    {
        _mono_assembly_open = func;
        _mono_assembly_get_image = dlsym(mod, "mono_assembly_get_image");
        _mono_class_from_name = dlsym(mod, "mono_class_from_name");
        if (_mono_class_from_name == NULL)
            _mono_class_from_name = dlsym(mod, "mono_class_from_name_case");
        _mono_class_get_method_from_name = dlsym(mod, "mono_class_get_method_from_name");
        _mono_runtime_invoke = dlsym(mod, "mono_runtime_invoke");
        if (_mono_runtime_invoke == NULL)
            _mono_runtime_invoke = dlsym(mod, "mono_runtime_invoke_array");
        xammac_init = true;
    }
    else
    {
        domain = mono_jit_init("wnxd");
        push_env("MONO_PATH", xamarin_lib);
        _mono_assemblies_init();
        if (xamarin_file_exists("/Library/Frameworks/Xamarin.Mac.framework/Versions/Current/lib/x86_64/full/Xamarin.Mac.dll"))
            xamarin_init("/Library/Frameworks/Xamarin.Mac.framework/Versions/Current/lib/x86_64/full/");
    }
    dlclose(mod);
}

int xammac_setup ()
{
    return 0;
}

void dotnet_load(const char* path, const char* lib)
{
    NSLog(@"load");
    if (self_init == false)
    {
        push_env("MONO_PATH", path);
        self_init = true;
    }
    char* n = strrchr(lib, '/');
    if (n != NULL)
    {
        char p[256];
        memcpy(p, lib, n-lib);
        push_env("MONO_PATH", p);
        _mono_assemblies_init();
    }
    if (xammac_init == false)
        xamarin_init(path);
    MonoAssembly* assembly = _mono_assembly_open(lib, NULL);
    NSLog(@"assembly: %p", assembly);
    if(assembly != NULL)
    {
        MonoImage* image = _mono_assembly_get_image(assembly);
        MonoClass* class = _mono_class_from_name(image, "", "Inject");
        if (class != NULL)
        {
            MonoMethod* method = _mono_class_get_method_from_name(class, "Entry", 0);
            if (method != NULL)
                _mono_runtime_invoke(method, nil, nil, nil);
        }
    }
    NSLog(@"load: %s", lib);
}
