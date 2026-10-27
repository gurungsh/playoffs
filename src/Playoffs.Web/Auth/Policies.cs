namespace Playoffs.Web.Auth;

// Policy names match the Keycloak realm roles; Admin passes every policy.
public static class Policies
{
    public const string Admin = "Admin";
    public const string Organizer = "Organizer";
}