using System;
using System.IO;
using System.Reflection;

namespace FountainSharp.Editor
{
	public static class Resource
	{
		static Resource ()
		{
		}

		public static Stream FromPath (string path)
		{
			var stream = Assembly.GetCallingAssembly().GetManifestResourceStream (path);
			if (stream == null) {
				throw new FileNotFoundException ("Could not locate resource " + path);
			}
			return stream;
		}

	}
}