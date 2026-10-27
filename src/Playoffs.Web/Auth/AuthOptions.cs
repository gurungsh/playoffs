using System.ComponentModel.DataAnnotations;

namespace Playoffs.Web.Auth;

public sealed class AuthOptions
{
    public const string SectionName = "Auth";

    [Required]
    public string Authority { get; set; } = "";

    [Required]
    public string ClientId { get; set; } = "";

    [Required]
    public string ClientSecret { get; set; } = "";

    public bool RequireHttpsMetadata { get; set; } = true;
}