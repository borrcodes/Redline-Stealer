using Org.BouncyCastle.Crypto.Modes.Gcm;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto.Utilities;
using Org.BouncyCastle.Utilities;
using System;

namespace Org.BouncyCastle.Crypto.Modes
{
	public class GcmBlockCipher : IAeadBlockCipher
	{
		private const int BlockSize = 16;

		private static readonly byte[] Zeroes = new byte[16];

		private readonly IBlockCipher cipher;

		private readonly IGcmMultiplier multiplier;

		private bool forEncryption;

		private int macSize;

		private byte[] nonce;

		private byte[] A;

		private KeyParameter keyParam;

		private byte[] H;

		private byte[] initS;

		private byte[] J0;

		private byte[] bufBlock;

		private byte[] macBlock;

		private byte[] S;

		private byte[] counter;

		private int bufOff;

		private ulong totalLength;

		public virtual string AlgorithmName => cipher.AlgorithmName + "/GCM";

		public GcmBlockCipher(IBlockCipher c)
			: this(c, null)
		{
		}

		public GcmBlockCipher(IBlockCipher c, IGcmMultiplier m)
		{
			if (c.GetBlockSize() != 16)
			{
				throw new ArgumentException("cipher required with a block size of " + 16 + ".");
			}
			if (m == null)
			{
				m = new Tables8kGcmMultiplier();
			}
			cipher = c;
			multiplier = m;
		}

		public virtual int GetBlockSize()
		{
			return 16;
		}

		public virtual void Init(bool forEncryption, ICipherParameters parameters)
		{
			this.forEncryption = forEncryption;
			macBlock = null;
			if (parameters is AeadParameters)
			{
				AeadParameters aeadParameters = (AeadParameters)parameters;
				nonce = aeadParameters.GetNonce();
				A = aeadParameters.GetAssociatedText();
				int num = aeadParameters.MacSize;
				if (num < 96 || num > 128 || num % 8 != 0)
				{
					throw new ArgumentException("Invalid value for MAC size: " + num);
				}
				macSize = num / 8;
				keyParam = aeadParameters.Key;
			}
			else
			{
				if (!(parameters is ParametersWithIV))
				{
					throw new ArgumentException("invalid parameters passed to GCM");
				}
				ParametersWithIV parametersWithIV = (ParametersWithIV)parameters;
				nonce = parametersWithIV.GetIV();
				A = null;
				macSize = 16;
				keyParam = (KeyParameter)parametersWithIV.Parameters;
			}
			int num2 = forEncryption ? 16 : (16 + macSize);
			bufBlock = new byte[num2];
			if (nonce == null || nonce.Length < 1)
			{
				throw new ArgumentException("IV must be at least 1 byte");
			}
			if (A == null)
			{
				A = new byte[0];
			}
			cipher.Init(forEncryption: true, keyParam);
			H = new byte[16];
			cipher.ProcessBlock(H, 0, H, 0);
			multiplier.Init(H);
			initS = gHASH(A);
			if (nonce.Length == 12)
			{
				J0 = new byte[16];
				Array.Copy(nonce, 0, J0, 0, nonce.Length);
				J0[15] = 1;
			}
			else
			{
				J0 = gHASH(nonce);
				byte[] array = new byte[16];
				packLength((ulong)((long)nonce.Length * 8L), array, 8);
				GcmUtilities.Xor(J0, array);
				multiplier.MultiplyH(J0);
			}
			S = Arrays.Clone(initS);
			counter = Arrays.Clone(J0);
			bufOff = 0;
			totalLength = 0uL;
		}

		public virtual byte[] GetMac()
		{
			return Arrays.Clone(macBlock);
		}

		public virtual int GetOutputSize(int len)
		{
			if (forEncryption)
			{
				return len + bufOff + macSize;
			}
			return len + bufOff - macSize;
		}

		public virtual int GetUpdateOutputSize(int len)
		{
			return (len + bufOff) / 16 * 16;
		}

		public virtual int ProcessByte(byte input, byte[] output, int outOff)
		{
			return Process(input, output, outOff);
		}

		public virtual int ProcessBytes(byte[] input, int inOff, int len, byte[] output, int outOff)
		{
			int num = 0;
			for (int i = 0; i != len; i++)
			{
				bufBlock[bufOff++] = input[inOff + i];
				if (bufOff == bufBlock.Length)
				{
					gCTRBlock(bufBlock, 16, output, outOff + num);
					if (!forEncryption)
					{
						Array.Copy(bufBlock, 16, bufBlock, 0, macSize);
					}
					bufOff = bufBlock.Length - 16;
					num += 16;
				}
			}
			return num;
		}

		private int Process(byte input, byte[] output, int outOff)
		{
			bufBlock[bufOff++] = input;
			if (bufOff == bufBlock.Length)
			{
				gCTRBlock(bufBlock, 16, output, outOff);
				if (!forEncryption)
				{
					Array.Copy(bufBlock, 16, bufBlock, 0, macSize);
				}
				bufOff = bufBlock.Length - 16;
				return 16;
			}
			return 0;
		}

		public int DoFinal(byte[] output, int outOff)
		{
			int num = bufOff;
			if (!forEncryption)
			{
				if (num < macSize)
				{
					throw new InvalidCipherTextException("data too short");
				}
				num -= macSize;
			}
			if (num > 0)
			{
				byte[] array = new byte[16];
				Array.Copy(bufBlock, 0, array, 0, num);
				gCTRBlock(array, num, output, outOff);
			}
			byte[] array2 = new byte[16];
			packLength((ulong)((long)A.Length * 8L), array2, 0);
			packLength(totalLength * 8, array2, 8);
			GcmUtilities.Xor(S, array2);
			multiplier.MultiplyH(S);
			byte[] array3 = new byte[16];
			cipher.ProcessBlock(J0, 0, array3, 0);
			GcmUtilities.Xor(array3, S);
			int num2 = num;
			macBlock = new byte[macSize];
			Array.Copy(array3, 0, macBlock, 0, macSize);
			if (forEncryption)
			{
				Array.Copy(macBlock, 0, output, outOff + bufOff, macSize);
				num2 += macSize;
			}
			else
			{
				byte[] array4 = new byte[macSize];
				Array.Copy(bufBlock, num, array4, 0, macSize);
				if (!Arrays.ConstantTimeAreEqual(macBlock, array4))
				{
					throw new InvalidCipherTextException("mac check in GCM failed");
				}
			}
			Reset(clearMac: false);
			return num2;
		}

		public virtual void Reset()
		{
			Reset(clearMac: true);
		}

		private void Reset(bool clearMac)
		{
			S = Arrays.Clone(initS);
			counter = Arrays.Clone(J0);
			bufOff = 0;
			totalLength = 0uL;
			if (bufBlock != null)
			{
				Array.Clear(bufBlock, 0, bufBlock.Length);
			}
			if (clearMac)
			{
				macBlock = null;
			}
			cipher.Reset();
		}

		private void gCTRBlock(byte[] buf, int bufCount, byte[] output, int outOff)
		{
			int num = 15;
			while (num >= 12 && ++counter[num] == 0)
			{
				num--;
			}
			byte[] array = new byte[16];
			cipher.ProcessBlock(counter, 0, array, 0);
			byte[] val;
			if (forEncryption)
			{
				Array.Copy(Zeroes, bufCount, array, bufCount, 16 - bufCount);
				val = array;
			}
			else
			{
				val = buf;
			}
			for (int num2 = bufCount - 1; num2 >= 0; num2--)
			{
				array[num2] ^= buf[num2];
				output[outOff + num2] = array[num2];
			}
			GcmUtilities.Xor(S, val);
			multiplier.MultiplyH(S);
			totalLength += (ulong)bufCount;
		}

		private byte[] gHASH(byte[] b)
		{
			byte[] array = new byte[16];
			for (int i = 0; i < b.Length; i += 16)
			{
				byte[] array2 = new byte[16];
				int length = Math.Min(b.Length - i, 16);
				Array.Copy(b, i, array2, 0, length);
				GcmUtilities.Xor(array, array2);
				multiplier.MultiplyH(array);
			}
			return array;
		}

		private static void packLength(ulong len, byte[] bs, int off)
		{
			Pack.UInt32_To_BE((uint)(len >> 32), bs, off);
			Pack.UInt32_To_BE((uint)len, bs, off + 4);
		}
	}
}
