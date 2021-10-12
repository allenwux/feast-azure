using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Azure.Feast.Data
{
    public class UserPermissionEntity
    {
        [Key]
        public long Id { get; set; }

        public string UserId { get; set; }

        public string UserName { get; set; }

        public int Role { get; set; }

        public string ProjectName { get; set; }

        public int Permission { get; set; }

        public DateTime CreatedTime { get; set; }

        public DateTime LastUpdatedTime { get; set; }

        public void UpdateFrom(UserPermissionEntity entity)
        {
            this.UserName = entity.UserName;
            this.Role = entity.Role;
            this.ProjectName = entity.ProjectName;
            this.Permission = entity.Permission;
            this.LastUpdatedTime = DateTime.UtcNow;
        }
    }
}
