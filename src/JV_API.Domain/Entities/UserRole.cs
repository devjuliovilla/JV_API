using JV_API.Domain.Common;

namespace JV_API.Domain.Entities;

public class UserRole : EntityBase
{
    public long UserId { get; set; }
    public User User { get; set; } = null!;
    public long RoleId { get; set; }
    public Role Role { get; set; } = null!;
}
