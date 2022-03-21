using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

#if DEBUG
[assembly: AssemblyConfiguration("DEBUG")]
#else
[assembly: AssemblyConfiguration("RELEASE")]
#endif
[assembly: AssemblyTitle("WicNet")]
[assembly: AssemblyDescription("WIC and Direct2D interop codd.")]
[assembly: AssemblyCompany("Simon Mourier")]
[assembly: AssemblyProduct("WicNet")]
[assembly: AssemblyCopyright("Copyright (C) 2021-2022 Simon Mourier. All rights reserved.")]
[assembly: AssemblyCulture("")]
[assembly: ComVisible(false)]
[assembly: Guid("40d973c4-2a06-4a0f-a91f-029fce007a04")]

[assembly: AssemblyVersion("1.4.0.0")]
[assembly: AssemblyFileVersion("1.4.0.0")]
