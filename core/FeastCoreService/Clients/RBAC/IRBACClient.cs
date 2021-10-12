using Azure.Feast.Data;
using Azure.Feast.Utils;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Azure.Feast.Clients.RBAC
{
    public interface IRBACClient
    {
        Task<CanAccessResult> CanAccessAsync(CanAccessQuery query);

        Task<UserPermissionResponse> InitRBACAsync(UserPermissionRequest request, FeastCoreRequestHeaders header);

        Task<UserPermissionResponse> AddUserPermissionAsync(UserPermissionRequest request, FeastCoreRequestHeaders header);

        Task<UserPermissionResponse> UpdateUserPermissionAsync(UserPermissionRequest request, FeastCoreRequestHeaders header);

        Task<UserPermissionResponse> RemoveUserPermissionAsync(UserPermissionRequest request, FeastCoreRequestHeaders header);

        Task<List<UserPermissionResponse>> ListUserPermissionsAsync(string userId, FeastCoreRequestHeaders header);

    }
}
