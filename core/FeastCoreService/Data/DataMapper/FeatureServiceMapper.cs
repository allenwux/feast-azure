using Feast.Core;
using Google.Protobuf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Azure.Feast.Data
{
    public class FeatureServiceMapper : IDataMapper<FeatureServiceRequest, FeatureServiceEntity, FeatureServiceResponse>
    {
        public FeatureServiceResponse Map(FeatureServiceEntity entity)
        {
            // For now, just return proto. 
            var response = new FeatureServiceResponse
            {
                Proto = entity.Proto,
            };

            return response;
        }

        public FeatureServiceEntity Map(FeatureServiceRequest request)
        {
            if (request.Data == null)
            {
                request.Data = FeatureService.Parser.ParseFrom(Convert.FromBase64String(request.Proto));
            }

            if (request.Proto == null)
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    request.Data.WriteTo(ms);
                    request.Proto = Convert.ToBase64String(ms.ToArray());
                }
            }

            var entity = new FeatureServiceEntity
            {
                Name = request.Data.Spec.Name,
                Project = request.Data.Spec.Project ?? null,
                Features = request.Data.Spec.Features.ToString(),
                Tags = request.Data.Spec.Tags.ToString(),
                Description = request.Data.Spec.Description,
                Proto = request.Proto,
            };

            entity.CreatedTime = request.Data.Meta.CreatedTimestamp == null ? DateTime.UtcNow : request.Data.Meta.CreatedTimestamp.ToDateTime();
            entity.LastUpdatedTime = request.Data.Meta.LastUpdatedTimestamp == null ? entity.CreatedTime : request.Data.Meta.LastUpdatedTimestamp.ToDateTime();
            return entity;
        }
    }
}
