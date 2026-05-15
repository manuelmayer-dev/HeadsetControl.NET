using System.Runtime.InteropServices;

// Restrict the default OS loader to safe directories (CA5392). Our own
// DllImportResolver in NativeLibraryLoader handles per-RID resolution
// before the fallback ever runs.
[assembly: DefaultDllImportSearchPaths(DllImportSearchPath.SafeDirectories)]
