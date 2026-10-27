using System.ComponentModel.DataAnnotations;

namespace Playoffs.Tournament.Api;

public sealed class AuthOptions
{
    public const string SectionName = "Auth";

    [Required]
    public string Authority { get; set; } = "";

    [Required]
    public string Audience { get; set; } = "";

    public bool RequireHttpsMetadata { get; set; } = true;
}