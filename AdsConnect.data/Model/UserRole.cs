using System;
using System.Collections.Generic;

namespace AdsConnect.data.Model;

public partial class UserRole
{
    public Guid UserId { get; set; }

    public Guid RoleId { get; set; }

    public DateTime AssignedDate { get; set; }

    public Guid? AssignedBy { get; set; }

    public virtual AppUser? AssignedByNavigation { get; set; }

    public virtual Role Role { get; set; } = null!;

    public virtual AppUser User { get; set; } = null!;
}
