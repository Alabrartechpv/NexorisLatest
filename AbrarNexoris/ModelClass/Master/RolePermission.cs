using System;

namespace ModelClass.Master
{
    /// <summary>
    /// Represents permission settings for a role on a specific form
    /// </summary>
    public class RolePermission
    {
        public int RolePermissionID { get; set; }
        public int RoleID { get; set; }
        public int FormID { get; set; }
        public string FormKey { get; set; }
        public string FormName { get; set; }
        public string Category { get; set; }
        public bool CanView { get; set; }
        public bool CanAdd { get; set; }
        public bool CanEdit { get; set; }
        public bool CanDelete { get; set; }
    }

    /// <summary>
    /// Grid model for displaying form permissions in admin UI
    /// </summary>
    public class FormPermissionGrid
    {
        public int FormID { get; set; }
        public string FormKey { get; set; }
        public string FormName { get; set; }
        public string Category { get; set; }
        public bool CanView { get; set; }
        public bool CanAdd { get; set; }
        public bool CanEdit { get; set; }
        public bool CanDelete { get; set; }
    }
}
