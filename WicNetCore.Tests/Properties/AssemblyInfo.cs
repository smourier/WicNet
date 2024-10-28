using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

#if DEBUG
[assembly: AssemblyConfiguration("DEBUG")]
#else
[assembly: AssemblyConfiguration("RELEASE")]
#endif
[assembly: AssemblyTitle("WicNetCore.Tests")]
[assembly: AssemblyDescription("WIC, Direct2D and DWrite interop code.")]
[assembly: AssemblyCompany("Simon Mourier")]
[assembly: AssemblyProduct("WicNet")]
[assembly: AssemblyCopyright("Copyright (C) 2021-2024 Simon Mourier. All rights reserved.")]
[assembly: AssemblyCulture("")]
[assembly: ComVisible(false)]
[assembly: Guid("31cdfd19-60ce-428e-b249-365a781e2ee5")]
[assembly: DisableRuntimeMarshalling]
[assembly: SupportedOSPlatform("windows8.0")]

