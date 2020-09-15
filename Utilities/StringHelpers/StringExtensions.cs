using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Utilities.StringHelpers
{
	public static class StringExtensions
	{
		#region Public Methods

		/// <summary>
		/// Converts/removes all non-ASCII to ASCII.
		/// </summary>
		/// <param name="inString">The in string.</param>
		/// <returns></returns>
		public static string ConvertAllToASCII(this string inString)
		{
			var newStringBuilder = new StringBuilder();
			newStringBuilder.Append(inString.Normalize(NormalizationForm.FormKD)
											.Where(x => x < 128)
											.ToArray());
			return newStringBuilder.ToString();
		}

		/// <summary>
		/// Converts string to SSML.
		/// </summary>
		/// <param name="unformattedMsg">The unformatted MSG.</param>
		/// <returns></returns>
		public static string ConvertToSSML(this string unformatted)
		{
			StringBuilder tempValue = new StringBuilder();
			unformatted = unformatted.Replace("&", " and ")
				.Replace(">", " greater than ")
				.Replace("<", " less than ")
				.Replace("'", "")
				.Replace("\"", "");
			tempValue.Append("<speak>");
			tempValue.Append(unformatted);
			tempValue.Append("</speak>");
			var retValue = Regex.Replace(tempValue.ToString(), @"\r\n?|\n|\\n|\\r\\n", @"<break time='250ms'/>");
			//remove too may breaks
			string pattern = @"<break time='250ms'\/>\s*<break time='250ms'\/>";
			string substitution = @"<break time='250ms'/>";
			RegexOptions options = RegexOptions.Multiline;
			Regex regex = new Regex(pattern, options);
			for (int i = 0; i < 3; i++)
			{
				retValue = regex.Replace(retValue, substitution);
			}

			return retValue;
		}

		public static string ExelFormatNumbersToNumber(this string text)
		{
			string pattern = @"""|\?|\$|!|%|,|\(|\)";
			//pattern = @"\(|\)|%|\$|""|\?\!|,";
			string substitution = @"";
			RegexOptions options = RegexOptions.Multiline;
			Regex regex = new Regex(pattern, options);
			var stripped = regex.Replace(text, substitution);
			return stripped;
		}

		/// <summary>
		/// Determines whether string [is null or white space].
		/// </summary>
		/// <param name="value">The value.</param>
		/// <returns>
		///   <c>true</c> if [is null or white space] [the specified value]; otherwise, <c>false</c>.
		/// </returns>
		public static bool IsNullOrWhiteSpace(this string value)
		{
			return string.IsNullOrWhiteSpace(value);
		}

		public static string RemovePunctuations(this string inString)
		{
			string pattern = @"[^\w\s]";
			string substitution = @"";
			RegexOptions options = RegexOptions.Multiline;

			Regex regex = new Regex(pattern, options);
			var stripped = regex.Replace(inString, substitution);
			return stripped;
		}

		/// <summary>
		/// Replaces the first.
		/// </summary>
		/// <param name="text">The text.</param>
		/// <param name="search">The search.</param>
		/// <param name="replace">The replace.</param>
		/// <returns></returns>
		public static string ReplaceFirst(this string text, string search, string replace)
		{
			int pos = text.IndexOf(search);
			if (pos < 0)
			{
				return text;
			}
			return text.Substring(0, pos) + replace + text.Substring(pos + search.Length);
		}

		public static string StripSpecialChar(this string text)
		{
			string pattern = @"(\""|\.|\?|\$\!)";
			string substitution = @"";
			RegexOptions options = RegexOptions.Multiline;
			Regex regex = new Regex(pattern, options);
			var stripped = regex.Replace(text, substitution);
			return stripped;
		}

		/// <summary>
		/// Converts to Thousands, millions, and billions.
		/// </summary>
		/// <param name="num">The number.</param>
		/// <returns></returns>
		public static string ToKMB(this decimal num)
		{
			if (num > 999999999 || num < -999999999)
			{
				return num.ToString("0,,,.### Billions", CultureInfo.InvariantCulture);
			}
			else
			if (num > 999999 || num < -999999)
			{
				return num.ToString("0,,.## Millions", CultureInfo.InvariantCulture);
			}
			else
			if (num > 999 || num < -999)
			{
				return num.ToString("0,.# Thousands", CultureInfo.InvariantCulture);
			}
			else
			{
				return num.ToString(CultureInfo.InvariantCulture);
			}
		}

		/// <summary>
		/// Truncates a string at word rather than at a number.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <param name="length">The length.</param>
		/// <returns></returns>
		public static string TruncateAtWord(this string value, int length)
		{
			if (value == null || value.Length < length || value.IndexOf(" ", length) == -1)
				return value;

			return value.Substring(0, value.IndexOf(" ", length));
		}

		#endregion Public Methods
	}
}