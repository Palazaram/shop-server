namespace Shop.Domain.Roles;

public static class RoleNames
{
    public const string Admin = "Admin";
    public const string Customer = "Customer";

    public static string FromId(int roleId) => roleId switch
    {
        RoleIds.Admin => Admin,
        RoleIds.Customer => Customer,
        _ => throw new ArgumentOutOfRangeException(nameof(roleId), $"Unknown role id: {roleId}")
    };
}