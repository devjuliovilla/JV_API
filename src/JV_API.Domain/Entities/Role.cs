using JV_API.Domain.Common;

namespace JV_API.Domain.Entities;

public class Role : EntityBase
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public ICollection<UserRole> UserRoles { get; set; } = [];
}
