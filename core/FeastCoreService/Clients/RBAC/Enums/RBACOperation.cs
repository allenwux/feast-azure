using System;
using System.Collections.Generic;
using System.Text;

namespace Azure.Feast.Clients.RBAC
{
    public enum RBACOperation
    {
        Create = 1,
        Update = 2,
        Read = 4,
        Delete = 8,
        List = 16,
        All = 31,
    }
}
