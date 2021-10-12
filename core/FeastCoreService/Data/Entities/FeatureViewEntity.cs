using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Azure.Feast.Data
{
    public class FeatureViewEntity
    {
        [Key]
        public long Id { get; set; }

        public string Name { get; set; }

        public string Project { get; set; }

        public string Entities { get; set; }

        public string Features { get; set; }

        public string Tags { get; set; }

        public double TimeToLiveInMs { get; set; }

        public bool Online { get; set; }

        public string BatchSource { get; set; }

        public string StreamSource { get; set; }

        public string MaterializationIntervals { get; set; }

        public string Proto { get; set; }

        public DateTime CreatedTime { get; set; }

        public DateTime LastUpdatedTime { get; set; }


        public void CopyFrom(FeatureViewEntity entity)
        {
            this.Entities = entity.Entities;
            this.Features = entity.Features;
            this.Tags = entity.Tags;
            this.TimeToLiveInMs = entity.TimeToLiveInMs;
            this.Online = entity.Online;
            this.BatchSource = entity.BatchSource;
            this.StreamSource = entity.StreamSource;
            this.MaterializationIntervals = entity.MaterializationIntervals;
            this.Proto = entity.Proto;
        }
    }
}
