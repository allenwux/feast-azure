using System;
using System.Collections.Generic;
using System.Text;

namespace Azure.Feast.Data
{
    public class EntityEntity
    {
        public long Id { get; set; }

        public string Name { get; set; }

        public string Project { get; set; }

        public int ValueType { get; set; }

        public string Description { get; set; }

        public string JoinKey { get; set; }

        public string Labels { get; set; }

        public string Proto { get; set; }

        public DateTime CreatedTime { get; set; }

        public DateTime LastUpdatedTime { get; set; }

        public void CopyFrom(EntityEntity entity)
        {
            this.ValueType = entity.ValueType;
            this.Description = entity.Description;
            this.JoinKey = entity.JoinKey;
            this.Labels = entity.Labels;
            this.Proto = entity.Proto;
        }
    }
}
