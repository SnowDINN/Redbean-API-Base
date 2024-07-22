using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace Redbean;

public class JwtSecurity
{
	private static readonly byte[] Key = Encoding.ASCII.GetBytes($"{Guid.NewGuid()}".Replace("-", ""));

	public static readonly SymmetricSecurityKey SecurityKey = new(Key);
	public static readonly SigningCredentials Credentials = new(SecurityKey, SecurityAlgorithms.HmacSha256);
}