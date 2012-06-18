using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace Linn
{
	internal class AssemblyInfo
	{
		//dlls for xp versions of functions		
		[DllImport("Kernel32.dll", SetLastError=true, EntryPoint="GetModuleHandle")]
		private static extern IntPtr XPGetModuleHandle(IntPtr ModuleName);

		[DllImport("Kernel32.dll", SetLastError=true, EntryPoint="GetModuleFileName")]
		private static extern Int32 XPGetModuleFileName(IntPtr hModule,StringBuilder ModuleName, Int32 cch);
		
		//dlls for cf versions of functions
		[DllImport("CoreDll.dll", SetLastError=true, EntryPoint="GetModuleHandle")] 
		private static extern IntPtr CFGetModuleHandle(IntPtr ModuleName);

		[DllImport("CoreDll.dll", SetLastError=true, EntryPoint="GetModuleFileName")]
		private static extern Int32 CFGetModuleFileName(IntPtr hModule, StringBuilder ModuleName, Int32 cch);		
		
		internal static AssemblyInfoModel GetAssemblyInfo() {
			
			string description = "";
		    string version = "";
		    string company = "";
		    string copyright = "";
		    string title = "";
		    string product = "";
		    
		    System.Reflection.Assembly entryAssembly = GetEntryAssembly();
		    
            object[] attributes = entryAssembly.GetCustomAttributes(typeof(AssemblyDescriptionAttribute), false);
            if (attributes.Length != 0)
            {
            	description = ((AssemblyDescriptionAttribute)attributes[0]).Description;
            }
                     
            attributes = entryAssembly.GetCustomAttributes(typeof(AssemblyCompanyAttribute), false);
            if (attributes.Length != 0)
            {
            	company = ((AssemblyCompanyAttribute)attributes[0]).Company;
            }
                    
            attributes = entryAssembly.GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false);
            if (attributes.Length != 0)
            {
                copyright = ((AssemblyCopyrightAttribute)attributes[0]).Copyright;
            }
            
           	attributes = entryAssembly.GetCustomAttributes(typeof(AssemblyProductAttribute), false);
         	if (attributes.Length != 0)
            {
                product = ((AssemblyProductAttribute)attributes[0]).Product;
            }
         	 	
         	attributes = entryAssembly.GetCustomAttributes(typeof(AssemblyTitleAttribute), false);
         	if (attributes.Length != 0)
            {
                title = ((AssemblyTitleAttribute)attributes[0]).Title;
                //return System.IO.Path.GetFileNameWithoutExtension(GetEntryAssembly().CodeBase);
            }
        
            version = entryAssembly.GetName().Version.ToString().Replace(".0", "");
            
            AssemblyInfoModel properties = new AssemblyInfoModel(description
                                                                                         ,version
                                                                                         ,company
                                                                                         ,copyright
                                                                                         ,title
                                                                                         ,product);
            
            return properties;			
		}
		
		private static System.Reflection.Assembly GetEntryAssembly() {
			
			if (Environment.OSVersion.Platform == PlatformID.WinCE) {
				return CFGetEntryAssembly();
			}
			else {
				return XPGetEntryAssembly();
			}
		}	
		
		private static System.Reflection.Assembly XPGetEntryAssembly() {
			StringBuilder sb = null;
			IntPtr hModule = XPGetModuleHandle(IntPtr.Zero);
			if (IntPtr.Zero != hModule)
			{
				sb = new StringBuilder(255);
				if (0 == XPGetModuleFileName(hModule, sb, sb.Capacity))
				{	
					sb = null;
				}
			}
			
			if (sb == null) {
				return null;
			}
			
			string assemblyName = sb.ToString();
			return System.Reflection.Assembly.LoadFrom(assemblyName);
		}
		
		private static System.Reflection.Assembly CFGetEntryAssembly() {
			StringBuilder sb = null;
			IntPtr hModule = CFGetModuleHandle(IntPtr.Zero);
			if (IntPtr.Zero != hModule)
			{
				sb = new StringBuilder(255);
				if (0 == CFGetModuleFileName(hModule, sb, sb.Capacity))
				{	
					sb = null;
				}
			}
			
			if (sb == null) {
				return null;
			}
			
			string assemblyName = sb.ToString();
			return System.Reflection.Assembly.LoadFrom(assemblyName);
		}
		
	}
}
