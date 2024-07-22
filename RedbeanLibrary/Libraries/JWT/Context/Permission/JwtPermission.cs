namespace Redbean;

public enum PermissionType
{
	Administrator,
	User
}

public class JwtPermission
{
	public static readonly List<string> administratorGroup = [];
	public static readonly Dictionary<PermissionType, string> permissionGroup = [];

	public static void SetAdministrators(List<string> administrator)
	{
		administratorGroup.Clear();
		administratorGroup.AddRange(administrator);
	}

	public static bool IsAdministratorExist(string value) => 
		administratorGroup.Contains(value);

	public static void SetPermission(PermissionType type, string value) =>
		permissionGroup[type] = value;

	public static string GetPermission(PermissionType type) => 
		permissionGroup[type];
}