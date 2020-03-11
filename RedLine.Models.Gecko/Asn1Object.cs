using System.Collections.Generic;
using System.Text;

namespace RedLine.Models.Gecko
{
	public class Asn1Object
	{
		public Asn1Type ObjectType
		{
			get;
			set;
		}

		public byte[] ObjectData
		{
			get;
			set;
		}

		public int ObjectLength
		{
			get;
			set;
		}

		public List<Asn1Object> Objects
		{
			get;
			set;
		}

		public Asn1Object()
		{
			Objects = new List<Asn1Object>();
		}

		public override string ToString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			StringBuilder stringBuilder2 = new StringBuilder();
			switch (ObjectType)
			{
			case Asn1Type.Sequence:
				stringBuilder.AppendLine("SEQUENCE {");
				break;
			case Asn1Type.Integer:
			{
				byte[] objectData = ObjectData;
				foreach (byte b2 in objectData)
				{
					stringBuilder2.AppendFormat("{0:X2}", b2);
				}
				stringBuilder.Append("\tINTEGER ").Append(stringBuilder2).AppendLine();
				break;
			}
			case Asn1Type.OctetString:
			{
				byte[] objectData = ObjectData;
				foreach (byte b3 in objectData)
				{
					stringBuilder2.AppendFormat("{0:X2}", b3);
				}
				stringBuilder.Append("\tOCTETSTRING ").AppendLine(stringBuilder2.ToString());
				break;
			}
			case Asn1Type.ObjectIdentifier:
			{
				byte[] objectData = ObjectData;
				foreach (byte b in objectData)
				{
					stringBuilder2.AppendFormat("{0:X2}", b);
				}
				stringBuilder.Append("\tOBJECTIDENTIFIER ").AppendLine(stringBuilder2.ToString());
				break;
			}
			}
			foreach (Asn1Object @object in Objects)
			{
				stringBuilder.Append(@object.ToString());
			}
			if (ObjectType == Asn1Type.Sequence)
			{
				stringBuilder.AppendLine("}");
			}
			stringBuilder2.Remove(0, stringBuilder2.Length - 1);
			return stringBuilder.ToString();
		}
	}
}
