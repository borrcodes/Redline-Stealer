using System.Collections.Generic;
using System.Linq;

namespace RedLine.Models.WMI
{
	public class WmiProcessQuery : WmiQueryBase
	{
		private static readonly string[] COLUMN_NAMES = new string[5]
		{
			"CommandLine",
			"Name",
			"ExecutablePath",
			"SIDType",
			"ParentProcessId"
		};

		public WmiProcessQuery()
			: base("Win32_Process", null, COLUMN_NAMES)
		{
		}

		public WmiProcessQuery(IEnumerable<string> processNames)
			: this()
		{
			base.SelectQuery.Condition = string.Join(" OR ", processNames.Select((string processName) => "Name LIKE '%" + processName + "%'"));
		}
	}
}
