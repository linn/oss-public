using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// General Information about an assembly is controlled through the following
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("Kinsky")]
[assembly: AssemblyDescription("A PDA UPnP control point.")]
[assembly: AssemblyCompany("Linn")]
[assembly: AssemblyProduct("Kinsky (NightlyBuild)")]
[assembly: AssemblyCopyright("Copyright 2009-2011")]
[assembly: ComVisible(false)]
[assembly: Guid("73515870-0f89-4efe-9288-19af6e299f00")]
[assembly: AssemblyVersion("4.2.5")]

// Below attribute is to suppress FxCop warning "CA2232 : Microsoft.Usage : Add STAThreadAttribute to assembly"
// as Device app does not support STA thread.
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2232:MarkWindowsFormsEntryPointsWithStaThread")]
