using System.Management;

namespace RedLine.Models.WMI
{
	public class WmiQueryBase
	{
		private readonly SelectQuery _selectQuery;

		internal SelectQuery SelectQuery => _selectQuery;

		protected WmiQueryBase(string className, string condition = null, string[] selectedProperties = null)
		{
			_selectQuery = new SelectQuery(className, condition, selectedProperties);
		}
	}
}
