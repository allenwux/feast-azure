using System;
using System.Collections.Generic;
using System.Text;

namespace Azure.Feast.Data
{
    public class UserPermissionMapper : IDataMapper<UserPermissionRequest, UserPermissionEntity, UserPermissionResponse>
    {
        public UserPermissionEntity Map(UserPermissionRequest request)
        {
            var entity = new UserPermissionEntity
            {
                UserId = request.UserId,
                UserName = request.UserName,
                Role = request.Role,
                ProjectName = request.ProjectName,
                Permission = request.Permission,
            };

            return entity;
        }

        public UserPermissionResponse Map(UserPermissionEntity entity)
        {
            var response = new UserPermissionResponse
            {
                UserId = entity.UserId,
                UserName = entity.UserName,
                Role = entity.Role,
                ProjectName = entity.ProjectName,
                Permission = entity.Permission,
                CreatedTime = entity.CreatedTime,
                LastUpdatedTime = entity.LastUpdatedTime
            };

            return response;
        }
    }
}
