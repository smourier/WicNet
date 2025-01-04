using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

#if DEBUG
[assembly: AssemblyConfiguration("DEBUG")]
#else
[assembly: AssemblyConfiguration("RELEASE")]
#endif
[assembly: AssemblyTitle("WicNetExplorer")]
[assembly: AssemblyDescription("WicNet Explorer")]
[assembly: AssemblyCompany("Simon Mourier")]
[assembly: AssemblyProduct("WicNet")]
[assembly: AssemblyCopyright("Copyright (C) 2021-2025 Simon Mourier. All rights reserved.")]
[assembly: AssemblyCulture("")]
[assembly: ComVisible(false)]
[assembly: Guid("4f39b26c-c2c2-49fd-bc3e-cafa2f9cef57")]
[assembly: SupportedOSPlatform("windows")]
