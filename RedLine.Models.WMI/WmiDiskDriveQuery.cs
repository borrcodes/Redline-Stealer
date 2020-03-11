namespace RedLine.Models.WMI
{
	public class WmiDiskDriveQuery : WmiQueryBase
	{
		public WmiDiskDriveQuery()
			: base("Win32_DiskDrive", null, new string[5]
			{
				"DeviceID",
				"MediaType",
				"Model",
				"PNPDeviceID",
				"SerialNumber"
			})
		{
		}
	}
}
