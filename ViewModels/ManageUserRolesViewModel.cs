using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyPhotoBiz.ViewModels
{
    /// <summary>
    /// Represents view model data for manage user roles.
    /// </summary>
    public class ManageUserRolesViewModel
    {
        public string RoleId { get; set; } = null!;
        public string RoleName { get; set; } = null!;
        public bool Selected { get; set; }
    }
}
