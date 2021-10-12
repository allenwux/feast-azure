using Feast.Core;
using Google.Protobuf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Azure.Feast.Data
{
    public class FeatureViewMapper : IDataMapper<FeatureViewRequest, FeatureViewEntity, FeatureViewResponse>
    {
        public FeatureViewResponse Map(FeatureViewEntity entity)
        {
            // For now, just return proto. 
            var response = new FeatureViewResponse
            {
                Proto = entity.Proto,
            };

            return response;
        }

        public FeatureViewEntity Map(FeatureViewRequest request)
        {
            if (request.Data == null)
            {
                request.Data = FeatureView.Parser.ParseFrom(Convert.FromBase64String(request.Proto));
            }

            if (request.Proto == null)
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    request.Data.WriteTo(ms);
                    request.Proto = Convert.ToBase64String(ms.ToArray());
                }
            }

            var entity = new FeatureViewEntity
            {
                Name = request.Data.Spec.Name,
                Project = request.Data.Spec.Project ?? null,
                Features = request.Data.Spec.Features == null ? null : request.Data.Spec.Features.ToString(),
                Entities = request.Data.Spec.Entities == null ? null : request.Data.Spec.Entities.ToString(),
                Tags = request.Data.Spec.Tags == null ? null : request.Data.Spec.Tags.ToString(),
                TimeToLiveInMs = request.Data.Spec.Ttl.ToTimeSpan().TotalMilliseconds,
                Online = request.Data.Spec.Online,
                BatchSource = request.Data.Spec.BatchSource == null ? null : request.Data.Spec.BatchSource.ToString(),
                StreamSource = request.Data.Spec.StreamSource == null ? null: request.Data.Spec.StreamSource.ToString(),
                Proto = request.Proto,
            };

            entity.CreatedTime = request.Data.Meta.CreatedTimestamp == null ? DateTime.UtcNow : request.Data.Meta.CreatedTimestamp.ToDateTime();
            entity.LastUpdatedTime = request.Data.Meta.LastUpdatedTimestamp == null ? entity.CreatedTime : request.Data.Meta.LastUpdatedTimestamp.ToDateTime();

            return entity;
        }
    }
}
