using System.Reflection;
using System.Runtime.InteropServices;

#if DEBUG
[assembly: AssemblyConfiguration("DEBUG")]
#else
[assembly: AssemblyConfiguration("RELEASE")]
#endif
[assembly: AssemblyTitle("WicNet")]
[assembly: AssemblyDescription("WIC, Direct2D and DWrite interop code.")]
[assembly: AssemblyCompany("Simon Mourier")]
[assembly: AssemblyProduct("WicNet")]
[assembly: AssemblyCopyright("Copyright (C) 2021-2024 Simon Mourier. All rights reserved.")]
[assembly: AssemblyCulture("")]
[assembly: ComVisible(false)]
[assembly: Guid("40d973c4-2a06-4a0f-a91f-029fce007a04")]
