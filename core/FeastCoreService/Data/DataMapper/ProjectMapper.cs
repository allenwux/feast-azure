using Feast.Core;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Azure.Feast.Data
{
    public class ProjectMapper : IDataMapper<ProjectRequest, ProjectEntity, ProjectResponse>
    {
        static Dictionary<string, Type> _onlineStoreTypeMap = new Dictionary<string, Type>
        {
            {"redis", typeof(RedisOnlineStore)},
        };

        public ProjectEntity Map(ProjectRequest request)
        {
            var entity = new ProjectEntity
            {
                ProjectName = request.ProjectName,
                Description = request.Description,
                Provider = request.Provider,
                OnlineStoreType = request.OnlineStore == null ? null : request.OnlineStore.Type,
                OnlineStoreConfig = request.OnlineStore == null ? null : JsonConvert.SerializeObject(request.OnlineStore),
                OfflineStoreType = request.OfflineStore == null ? null: request.OfflineStore.Type,
                OfflineStoreConfig = request.OfflineStore == null ? null: JsonConvert.SerializeObject(request.OfflineStore),
                Flags = request.Flags,
                IsDefault = request.IsDefault,
            };

            return entity;
        }

        public ProjectResponse Map(ProjectEntity entity)
        {
            var response = new ProjectResponse
            {
                ProjectName = entity.ProjectName,
                Description = entity.Description,
                OnlineStore = entity.OnlineStoreConfig == null ? null : JsonConvert.DeserializeObject(entity.OnlineStoreConfig),
                OfflineStore = entity.OfflineStoreConfig == null ? null : JsonConvert.DeserializeObject(entity.OfflineStoreConfig),
                Flags = entity.Flags,
                IsDefault = entity.IsDefault,
                Provider = entity.Provider,
                CreatedTime = entity.CreatedTime,
                LastUpdatedTime = entity.LastUpdatedTime
            };

            return response;
        }
    }
}
