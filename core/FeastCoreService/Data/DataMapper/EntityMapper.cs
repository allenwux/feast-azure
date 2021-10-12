using Feast.Core;
using Google.Protobuf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Azure.Feast.Data
{
    public class EntityMapper : IDataMapper<EntityRequest, EntityEntity, EntityResponse>
    {
        public EntityResponse Map(EntityEntity entity)
        {
            // For now, just return proto. 
            var response = new EntityResponse
            {
                Proto = entity.Proto,
            };

            return response;
        }

        public EntityEntity Map(EntityRequest request)
        {
            if(request.Data == null)
            {
                request.Data = Entity.Parser.ParseFrom(Convert.FromBase64String(request.Proto));
            }

            if (request.Proto == null)
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    request.Data.WriteTo(ms);
                    request.Proto = Convert.ToBase64String(ms.ToArray());
                }
            }

            var entity = new EntityEntity
            {
                Name = request.Data.Spec.Name,
                Project = request.Data.Spec.Project ?? null,
                ValueType = (int)request.Data.Spec.ValueType,
                Description = request.Data.Spec.Description ?? null,
                JoinKey = request.Data.Spec.JoinKey,
                Labels = request.Data.Spec.Labels.ToString(),
                Proto = request.Proto,
            };

            entity.CreatedTime = request.Data.Meta.CreatedTimestamp == null ? DateTime.UtcNow : request.Data.Meta.CreatedTimestamp.ToDateTime();
            entity.LastUpdatedTime = request.Data.Meta.LastUpdatedTimestamp == null ? entity.CreatedTime : request.Data.Meta.LastUpdatedTimestamp.ToDateTime();

            return entity;
        }
    }
}
