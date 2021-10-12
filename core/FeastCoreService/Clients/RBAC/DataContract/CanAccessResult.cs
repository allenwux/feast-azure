using System;
using System.Collections.Generic;
using System.Text;

namespace Azure.Feast.Clients.RBAC
{
    public class CanAccessResult
    {
        public bool CanAccess { get; set; }

        public RBACRole Role { get; set; }

        public string ProjectName { get; set; }

        public int Operations { get; set; }
    }
}
