namespace RedLine.Models.WMI
{
	public class WmiNetworkAdapterQuery : WmiQueryBase
	{
		private static readonly string[] COLUMN_NAMES = new string[3]
		{
			"GUID",
			"MACAddress",
			"PNPDeviceID"
		};

		public WmiNetworkAdapterQuery(WmiNetworkAdapterType adapterType = WmiNetworkAdapterType.All)
			: base("Win32_NetworkAdapter", null, COLUMN_NAMES)
		{
			switch (adapterType)
			{
			case WmiNetworkAdapterType.Physical:
				base.SelectQuery.Condition = "PhysicalAdapter=1";
				break;
			case WmiNetworkAdapterType.Virtual:
				base.SelectQuery.Condition = "PhysicalAdapter=0";
				break;
			}
		}
	}
}
