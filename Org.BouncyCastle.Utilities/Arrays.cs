using System;
using System.Text;

namespace Org.BouncyCastle.Utilities
{
	public sealed class Arrays
	{
		private Arrays()
		{
		}

		public static bool AreEqual(bool[] a, bool[] b)
		{
			if (a == b)
			{
				return true;
			}
			if (a == null || b == null)
			{
				return false;
			}
			return HaveSameContents(a, b);
		}

		public static bool AreEqual(char[] a, char[] b)
		{
			if (a == b)
			{
				return true;
			}
			if (a == null || b == null)
			{
				return false;
			}
			return HaveSameContents(a, b);
		}

		public static bool AreEqual(byte[] a, byte[] b)
		{
			if (a == b)
			{
				return true;
			}
			if (a == null || b == null)
			{
				return false;
			}
			return HaveSameContents(a, b);
		}

		[Obsolete("Use 'AreEqual' method instead")]
		public static bool AreSame(byte[] a, byte[] b)
		{
			return AreEqual(a, b);
		}

		public static bool ConstantTimeAreEqual(byte[] a, byte[] b)
		{
			int num = a.Length;
			if (num != b.Length)
			{
				return false;
			}
			int num2 = 0;
			while (num != 0)
			{
				num--;
				num2 |= (a[num] ^ b[num]);
			}
			return num2 == 0;
		}

		public static bool AreEqual(int[] a, int[] b)
		{
			if (a == b)
			{
				return true;
			}
			if (a == null || b == null)
			{
				return false;
			}
			return HaveSameContents(a, b);
		}

		private static bool HaveSameContents(bool[] a, bool[] b)
		{
			int num = a.Length;
			if (num != b.Length)
			{
				return false;
			}
			while (num != 0)
			{
				num--;
				if (a[num] != b[num])
				{
					return false;
				}
			}
			return true;
		}

		private static bool HaveSameContents(char[] a, char[] b)
		{
			int num = a.Length;
			if (num != b.Length)
			{
				return false;
			}
			while (num != 0)
			{
				num--;
				if (a[num] != b[num])
				{
					return false;
				}
			}
			return true;
		}

		private static bool HaveSameContents(byte[] a, byte[] b)
		{
			int num = a.Length;
			if (num != b.Length)
			{
				return false;
			}
			while (num != 0)
			{
				num--;
				if (a[num] != b[num])
				{
					return false;
				}
			}
			return true;
		}

		private static bool HaveSameContents(int[] a, int[] b)
		{
			int num = a.Length;
			if (num != b.Length)
			{
				return false;
			}
			while (num != 0)
			{
				num--;
				if (a[num] != b[num])
				{
					return false;
				}
			}
			return true;
		}

		public static string ToString(object[] a)
		{
			StringBuilder stringBuilder = new StringBuilder(91);
			if (a.Length != 0)
			{
				stringBuilder.Append(a[0]);
				for (int i = 1; i < a.Length; i++)
				{
					stringBuilder.Append(", ").Append(a[i]);
				}
			}
			stringBuilder.Append(']');
			return stringBuilder.ToString();
		}

		public static int GetHashCode(byte[] data)
		{
			if (data == null)
			{
				return 0;
			}
			int num = data.Length;
			int num2 = num + 1;
			while (--num >= 0)
			{
				num2 *= 257;
				num2 ^= data[num];
			}
			return num2;
		}

		public static byte[] Clone(byte[] data)
		{
			if (data != null)
			{
				return (byte[])data.Clone();
			}
			return null;
		}

		public static int[] Clone(int[] data)
		{
			if (data != null)
			{
				return (int[])data.Clone();
			}
			return null;
		}
	}
}
