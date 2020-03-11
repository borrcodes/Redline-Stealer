using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Management;
using System.Reflection;

namespace RedLine.Models.WMI
{
	public class WmiService : IWmiService
	{
		private static TResult Extract<TResult>(ManagementBaseObject managementObject) where TResult : class, new()
		{
			TResult val = new TResult();
			PropertyInfo[] properties = typeof(TResult).GetProperties();
			foreach (PropertyInfo propertyInfo in properties)
			{
				WmiResultAttribute wmiResultAttribute = (WmiResultAttribute)Attribute.GetCustomAttribute(propertyInfo, typeof(WmiResultAttribute));
				if (wmiResultAttribute != null)
				{
					object value = managementObject.Properties[wmiResultAttribute.PropertyName].Value;
					Type type = Nullable.GetUnderlyingType(propertyInfo.PropertyType) ?? propertyInfo.PropertyType;
					object value2 = (value == null) ? null : ((type == typeof(DateTime)) ? ((object)ManagementDateTimeConverter.ToDateTime(value.ToString()).ToUniversalTime()) : ((!(type == typeof(Guid))) ? Convert.ChangeType(managementObject.Properties[wmiResultAttribute.PropertyName].Value, type) : ((object)Guid.Parse(value.ToString()))));
					propertyInfo.SetValue(val, value2, null);
				}
			}
			return val;
		}

		private ManagementObjectCollection QueryAll(SelectQuery selectQuery, ManagementObjectSearcher searcher = null)
		{
			searcher = (searcher ?? new ManagementObjectSearcher());
			searcher.Query = selectQuery;
			return searcher.Get();
		}

		private ManagementBaseObject QueryFirst(SelectQuery selectQuery, ManagementObjectSearcher searcher = null)
		{
			return QueryAll(selectQuery, searcher).Cast<ManagementBaseObject>().FirstOrDefault();
		}

		public TResult QueryFirst<TResult>(WmiQueryBase wmiQuery) where TResult : class, new()
		{
			ManagementBaseObject managementBaseObject = QueryFirst(wmiQuery.SelectQuery);
			if (managementBaseObject != null)
			{
				return Extract<TResult>(managementBaseObject);
			}
			return null;
		}

		public ReadOnlyCollection<TResult> QueryAll<TResult>(WmiQueryBase wmiQuery, ManagementObjectSearcher searcher = null) where TResult : class, new()
		{
			return new ReadOnlyCollection<TResult>(QueryAll(wmiQuery.SelectQuery, searcher)?.Cast<ManagementBaseObject>().Select(Extract<TResult>).ToList());
		}
	}
}
