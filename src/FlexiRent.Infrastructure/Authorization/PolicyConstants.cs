namespace FlexiRent.Infrastructure.Authorization;

public static class PolicyConstants
{
    public const string RequireAdmin = "RequireAdmin";
    public const string RequireModerator = "RequireModerator";
    public const string RequireServiceProvider = "RequireServiceProvider";
    public const string RequireVendor = "RequireVendor";
    public const string RequireAdminOrModerator = "RequireAdminOrModerator";
    public const string RequireVerifiedUser = "RequireVerifiedUser";
}