using System;

namespace ModelClass.Master
{
    /// <summary>
    /// Represents a user role in the RBAC system
    /// </summary>
    public class Role
    {
        public int RoleID { get; set; }
        public string RoleName { get; set; }
        public string Description { get; set; }
        public int? UserLevelID { get; set; }  // Links to existing Users.UserLevelID
        public bool IsActive { get; set; }
    }

    /// <summary>
    /// Role dropdown item for combo boxes
    /// </summary>
    public class RoleDDl
    {
        public int RoleID { get; set; }
        public string RoleName { get; set; }
        public int? UserLevelID { get; set; }  // Links to existing Users.UserLevelID
    }
}
