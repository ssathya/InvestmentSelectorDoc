using System;
using System.Security.Cryptography;
using System.Text;

namespace Utilities.StringHelpers
{
	public static class StringEncrypt
	{
		#region Private Fields

		private static readonly string topSecret = "My name is Bond; James Bond!";

		#endregion Private Fields

		#region Public Methods

		public static string Crypt(this string text)
		{
			using (var objHashMD5 = new MD5CryptoServiceProvider())
			{
				var txtArray = Encoding.ASCII.GetBytes(text);
				return Convert.ToBase64String(new TripleDESCryptoServiceProvider
				{
					Key = objHashMD5.ComputeHash(Encoding.ASCII.GetBytes(topSecret)),
					Mode = CipherMode.ECB
				}.CreateEncryptor()
					.TransformFinalBlock(txtArray, 0, txtArray.Length));
			}
		}

		public static string Decrypt(this string text)
		{
			try
			{
				byte[] byteHash;
				using (var objHashMD5 = new MD5CryptoServiceProvider())
				{
					byteHash = objHashMD5.ComputeHash(Encoding.ASCII.GetBytes(topSecret));
				}

				return Encoding.ASCII.GetString(new TripleDESCryptoServiceProvider
				{
					Key = byteHash,
					Mode = CipherMode.ECB
				}.CreateDecryptor().TransformFinalBlock(Convert.FromBase64String(text), 0, Convert.FromBase64String(text).Length));
			}
			catch (Exception ex)
			{
				Console.WriteLine("Data decryption failed");
				Console.WriteLine(ex.Message);
			}
			return null;
		}

		#endregion Public Methods
	}
}