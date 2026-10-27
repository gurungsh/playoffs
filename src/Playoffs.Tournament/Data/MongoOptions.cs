using System.ComponentModel.DataAnnotations;

namespace Playoffs.Tournament.Data;

public sealed class MongoOptions
{
    public const string SectionName = "Mongo";

    [Required]
    public string ConnectionString { get; set; } = "";

    [Required]
    public string Database { get; set; } = "";
}