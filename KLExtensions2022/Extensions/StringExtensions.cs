using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.IO;
using System.Diagnostics;
using System.Linq;
using KLExtensions2022.Helpers;

namespace KLExtensions2022
{
	internal static class StringExtensions
	{
		public const string CarriageReturnLineFeed = "\r\n";
		public const string Empty = "";
		public const char CarriageReturn = '\r';
		public const char LineFeed = '\n';
		public const char Tab = '\t';

		private delegate void ActionLine(TextWriter textWriter, string line);

		internal static string ToSentenceCase(this string s)
		{
			return Regex.Replace(s, "[a-z][A-Z]", m => m.Value[0] + " " + char.ToUpper(m.Value[1]));
		}

		internal static string ToEscapedFilename(this string fileNameToEscape)
		{
			char[] illegalchars = { '\\', '/', ':', '*', '?', '"', '<', '>', '|' };
			return RemoveChars(fileNameToEscape, illegalchars);
		}

		private static string RemoveChars(string content, IEnumerable<char> illegalchars)
		{
			if (string.IsNullOrEmpty(content)) return string.Empty;
			return illegalchars.Aggregate(content, (current, item) => current.Replace(item.ToString(), string.Empty));
		}

		internal static string ReplaceNewLineCharacters(this string str) => str.Replace("\n", "");

		internal static string JoinToString(this List<string> items, string delimeter)
		{
			return string.Join(delimeter, items.ToArray());
		}

		internal static string RemoveFileNameExtension(this string filename)
		{
			string[] fileParts = filename.Split('.');
			if (fileParts.Length > 0)
			{
				return fileParts[0];
			}
			return filename;
		}

		internal static string RemoveFileNameFromPath(this string fileNamePath)
		{
			string[] fileParts = FileHelper.SplitPath(fileNamePath);
			string[] newFileParts = new string[fileParts.Length - 1];

			string filePath = "";
			for (int i = 0 ; i < fileParts.Length - 1 ; i++)
			{
				filePath += fileParts[i] + "\\";
				newFileParts[i] = fileParts[i];
			}
			newFileParts[0] = $"{newFileParts[0]}\\";
			int index = filePath.LastIndexOf("\\");
			filePath = filePath.Remove(index);

			return Path.Combine(newFileParts);
		}

		public static bool Contains(this string source, string toCheck, StringComparison comp) => source.IndexOf(toCheck, comp) >= 0;

		public static string[] Lines(this string source) => source.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);

		public static long? ParseNumber(string s)
		{
			try
			{
				return !s.StartsWith("0x", StringComparison.Ordinal) ? Convert.ToInt64(s, 10) : Convert.ToInt64(s.Replace("0x", ""), 16);
			}
			catch (Exception)
			{
				return null;
			}
		}

		[DebuggerStepThrough]
		public static bool IsEmpty(this string value)
		{
			return string.IsNullOrWhiteSpace(value);
		}

    }
}
