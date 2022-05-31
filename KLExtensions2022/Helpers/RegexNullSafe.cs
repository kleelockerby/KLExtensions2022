using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace KLExtensions2022.Helpers
{
    public static class RegexNullSafe
    {
		public static bool IsMatch(string input, string pattern)
		{
			if (input == null || pattern == null)
			{
				return false;
			}

			return Regex.IsMatch(input, pattern);
		}
	}
}
