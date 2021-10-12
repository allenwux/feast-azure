using System;
using System.Collections.Generic;
using System.Text;

namespace Azure.Feast.Data
{
    public class ProjectEntity
    {
        public long Id { get; set; }

        public string ProjectName { get; set; }

        public string Description { get; set; }

        public string OnlineStoreType { get; set; }

        public string OnlineStoreConfig { get; set; }

        public string OfflineStoreType { get; set; }

        public string OfflineStoreConfig { get; set; }

        public string Provider { get; set; }

        public string Flags { get; set; }

        public bool IsDefault { get; set; }

        public DateTime CreatedTime { get; set; }

        public DateTime LastUpdatedTime { get; set; }

        public void CopyFrom(ProjectEntity entity)
        {
            this.Description = entity.Description;
            this.OnlineStoreConfig = entity.OnlineStoreConfig;
            this.OnlineStoreType = entity.OnlineStoreType;
            this.OfflineStoreConfig = entity.OfflineStoreConfig;
            this.OfflineStoreType = entity.OfflineStoreType;
            this.Flags = entity.Flags;
            this.IsDefault = entity.IsDefault;
        }
    }
}
