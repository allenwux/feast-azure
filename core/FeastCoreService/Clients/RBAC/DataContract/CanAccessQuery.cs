using System;
using System.Collections.Generic;
using System.Text;

namespace Azure.Feast.Clients.RBAC
{
    public class CanAccessQuery
    {
        public string UserId { get; set; }

        public bool RequireAdminPermission { get; set; }

        public string ProjectName { get; set; }

        public int Operations { get; set; }
    }
}
