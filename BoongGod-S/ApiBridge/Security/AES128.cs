using System;
using System.Security.Cryptography;
using System.Text;

namespace Redbean.Api
{
	public class AES128
	{
		private const string SecurityKey = "redbean.boongsin.api.network";
		
		private readonly Aes Aes;

		public AES128()
		{
			Aes = Aes.Create();
			Aes.Mode = CipherMode.CBC;
			Aes.Padding = PaddingMode.PKCS7;
			Aes.KeySize = 128;
			Aes.BlockSize = 128;
		}

		private static string key
		{
			get
			{
				var md5 = MD5.Create();
				var result = md5.ComputeHash(Encoding.UTF8.GetBytes(SecurityKey));

				return Encoding.UTF8.GetString(result);
			}
		}

		public string Encryption(string text)
		{
			var sourceArray = Encoding.UTF8.GetBytes(key);
			var keyBytes = new byte[16];
			
			var count = sourceArray.Length;
			if (count > keyBytes.Length) 
				count = keyBytes.Length;
			Array.Copy(sourceArray, keyBytes, count);
			
			Aes.Key = keyBytes;
			Aes.IV = keyBytes;
			
			var encryptor = Aes.CreateEncryptor();
			var buffer = Encoding.UTF8.GetBytes(text);

			return Convert.ToBase64String(encryptor.TransformFinalBlock(buffer, 0, buffer.Length));
		}

		public string Decryption(string text)
		{
			var fromBase64String = Convert.FromBase64String(text);
			var sourceArray = Encoding.UTF8.GetBytes(key);
			var keyBytes = new byte[16];
			
			var count = sourceArray.Length;
			if (count > keyBytes.Length) 
				count = keyBytes.Length;
			Array.Copy(sourceArray, keyBytes, count);
			
			Aes.Key = keyBytes;
			Aes.IV = keyBytes;
			
			var decryptedString = Aes.CreateDecryptor().TransformFinalBlock(fromBase64String, 0, fromBase64String.Length);
			return Encoding.UTF8.GetString(decryptedString);
		}
	}
}