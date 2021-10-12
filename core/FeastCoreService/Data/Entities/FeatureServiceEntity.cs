using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Azure.Feast.Data
{
    public class FeatureServiceEntity
    {
        [Key]
        public long Id { get; set; }

        public string Name { get; set; }

        public string Project { get; set; }

        public string Features { get; set; }

        public string Tags { get; set; }

        public string Description { get; set; }

        public string Proto { get; set; }

        public DateTime CreatedTime { get; set; }

        public DateTime LastUpdatedTime { get; set; }

        public void CopyFrom(FeatureServiceEntity entity)
        {
            this.Features = entity.Features;
            this.Tags = entity.Tags;
            this.Description = entity.Description;
            this.Proto = entity.Proto;
        }
    }
}
