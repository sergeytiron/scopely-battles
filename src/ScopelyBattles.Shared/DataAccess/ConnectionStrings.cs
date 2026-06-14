using System.ComponentModel.DataAnnotations;

namespace ScopelyBattles.Shared.DataAccess;

public sealed class ConnectionStrings
{
    [Required]
    public required string Postgres { get; set; }
}
