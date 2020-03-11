using System;

namespace RedLine.Models.WMI
{
	[AttributeUsage(AttributeTargets.Property)]
	public class WmiResultAttribute : Attribute
	{
		public string PropertyName
		{
			get;
		}

		public WmiResultAttribute(string propertyName)
		{
			PropertyName = propertyName;
		}
	}
}
