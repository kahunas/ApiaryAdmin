namespace ApiaryAdmin.Auth.Model;

public class ApiaryRoles
{
    public const string Admin = nameof(Admin);
    public const string ApiaryUser = nameof(ApiaryUser);
    
    public static readonly IReadOnlyCollection<string> All = new[] { Admin, ApiaryUser };
}