namespace Redbean.Api
{
	public static class SecurityExtension
	{
		private static readonly AES128 AES128 = new();
		public static string Encryption(this string value) => AES128.Encryption(value);
		public static string Decryption(this string value) => AES128.Decryption(value);
	}
}