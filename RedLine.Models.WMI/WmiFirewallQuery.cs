namespace RedLine.Models.WMI
{
	public class WmiFirewallQuery : WmiQueryBase
	{
		public WmiFirewallQuery()
			: base("FirewallProduct", null, new string[2]
			{
				"displayName",
				"pathToSignedProductExe"
			})
		{
		}
	}
}
