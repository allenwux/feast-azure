using Azure.Feast.Data;
using Azure.Feast.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Azure.Feast.Clients.FeastCore
{
    public class FeastCoreClient : IFeastCoreClient
    {
        private readonly ISqlDbContext _dbContext;
        private readonly IDataMapper<FeatureViewRequest, FeatureViewEntity, FeatureViewResponse> _featureViewMapper;
        private readonly IDataMapper<FeatureServiceRequest, FeatureServiceEntity, FeatureServiceResponse> _featureServiceMapper;
        private readonly IDataMapper<EntityRequest, EntityEntity, EntityResponse> _entityMapper;
        private readonly IDataMapper<ProjectRequest, ProjectEntity, ProjectResponse> _projectMapper;
        private readonly ILogger<FeastCoreClient> _logger;

        public FeastCoreClient(ISqlDbContext dbContext,
            IDataMapper<FeatureViewRequest, FeatureViewEntity, FeatureViewResponse> featureViewMapper,
            IDataMapper<FeatureServiceRequest, FeatureServiceEntity, FeatureServiceResponse> featureServiceMapper,
            IDataMapper<EntityRequest, EntityEntity, EntityResponse> entityMapper,
            IDataMapper<ProjectRequest, ProjectEntity, ProjectResponse> projectMapper,
            ILogger<FeastCoreClient> logger)
        {
            this._dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            this._featureViewMapper = featureViewMapper ?? throw new ArgumentNullException(nameof(featureViewMapper));
            this._featureServiceMapper = featureServiceMapper ?? throw new ArgumentNullException(nameof(featureServiceMapper));
            this._entityMapper = entityMapper ?? throw new ArgumentNullException(nameof(entityMapper));
            this._projectMapper = projectMapper ?? throw new ArgumentNullException(nameof(projectMapper));
            this._logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        #region Feature Views
        public async Task<FeatureViewResponse> CreateOrUpdateFeatureViewAsync(string projectName, string featureViewName, FeatureViewRequest request, FeastCoreRequestHeaders headers)
        {
            if (!await this._dbContext.Projects.AnyAsync(x => x.ProjectName == projectName))
            {
                throw new FeastCoreNotFoundUserException(
                    ErrorUtils.FormatErrorMessage(ErrorMessages.PROJECT_NOT_FOUND, projectName), (int)ErrorCode.OBJECT_NOT_FOUND, headers.TraceId);
            }

            var featureViewEntity = await this._dbContext.FeatureViews.SingleOrDefaultAsync(x => x.Project == projectName && x.Name == featureViewName);

            if (featureViewEntity == null)
            {
                featureViewEntity = this._featureViewMapper.Map(request);

                // Project is an optional parameter in the client library
                if (string.IsNullOrEmpty(featureViewEntity.Project))
                {
                    featureViewEntity.Project = projectName;
                }

                this._dbContext.FeatureViews.Add(featureViewEntity);
                await _dbContext._SaveChangesAsync();
            }
            else
            {
                featureViewEntity.CopyFrom(this._featureViewMapper.Map(request));

                // Project is an optional parameter in the client library
                if (string.IsNullOrEmpty(featureViewEntity.Project))
                {
                    featureViewEntity.Project = projectName;
                }

                featureViewEntity.LastUpdatedTime = DateTime.UtcNow;

                this._dbContext.FeatureViews.Update(featureViewEntity);
                await this._dbContext._SaveChangesAsync();
            }

            return this._featureViewMapper.Map(featureViewEntity);

        }

        public async Task<FeatureViewResponse> CreateFeatureViewAsync(string projectName, string featureViewName, FeatureViewRequest request, FeastCoreRequestHeaders headers)
        {
            if (!await this._dbContext.Projects.AnyAsync(x => x.ProjectName == projectName))
            {
                throw new FeastCoreNotFoundUserException(
                    ErrorUtils.FormatErrorMessage(ErrorMessages.PROJECT_NOT_FOUND, projectName), (int)ErrorCode.OBJECT_NOT_FOUND, headers.TraceId);
            }

            if (await this._dbContext.FeatureViews.AnyAsync(x => x.Project == projectName && x.Name == featureViewName))
            {
                throw new FeastCoreConflictUserException(
                    ErrorUtils.FormatErrorMessage(ErrorMessages.FEATURE_VIEW_ALREADY_EXIST, featureViewName, projectName), (int)ErrorCode.OBJECT_ALREDAY_EXIST, headers.TraceId);
            }

            var featureViewEntity = this._featureViewMapper.Map(request);

            // Project is an optional parameter in the client library
            if (string.IsNullOrEmpty(featureViewEntity.Project))
            {
                featureViewEntity.Project = projectName;
            }

            this._dbContext.FeatureViews.Add(featureViewEntity);
            await _dbContext._SaveChangesAsync();

            return this._featureViewMapper.Map(featureViewEntity);

        }

        public async Task<FeatureViewResponse> UpdateFeatureViewAsync(string projectName, string featureViewName, FeatureViewRequest request, FeastCoreRequestHeaders headers)
        {
            if (!await this._dbContext.Projects.AnyAsync(x => x.ProjectName == projectName))
            {
                throw new FeastCoreNotFoundUserException(
                    ErrorUtils.FormatErrorMessage(ErrorMessages.PROJECT_NOT_FOUND, projectName), (int)ErrorCode.OBJECT_NOT_FOUND, headers.TraceId);
            }

            var featureViewEntity = await this._dbContext.FeatureViews.SingleOrDefaultAsync(x => x.Project == projectName && x.Name == featureViewName);

            if (featureViewEntity == null)
            {
                throw new FeastCoreNotFoundUserException(
                    ErrorUtils.FormatErrorMessage(ErrorMessages.FEATURE_VIEW_NOT_FOUND, featureViewName, projectName), (int)ErrorCode.OBJECT_NOT_FOUND, headers.TraceId);
            }

            featureViewEntity.CopyFrom(this._featureViewMapper.Map(request));

            // Project is an optional parameter in the client library
            if (string.IsNullOrEmpty(featureViewEntity.Project))
            {
                featureViewEntity.Project = projectName;
            }

            featureViewEntity.LastUpdatedTime = DateTime.UtcNow;

            this._dbContext.FeatureViews.Update(featureViewEntity);
            await this._dbContext._SaveChangesAsync();

            return this._featureViewMapper.Map(featureViewEntity);
        }

        public async Task DeleteFeatureViewAsync(string projectName, string featureViewName, FeastCoreRequestHeaders headers)
        {
            if (!await this._dbContext.Projects.AnyAsync(x => x.ProjectName == projectName))
            {
                throw new FeastCoreNotFoundUserException(
                    ErrorUtils.FormatErrorMessage(ErrorMessages.PROJECT_NOT_FOUND, projectName), (int)ErrorCode.OBJECT_NOT_FOUND, headers.TraceId);
            }

            var featureViewEntity = await this._dbContext.FeatureViews.SingleOrDefaultAsync(x => x.Project == projectName && x.Name == featureViewName);

            if (featureViewEntity == null)
            {
                throw new FeastCoreNotFoundUserException(
                    ErrorUtils.FormatErrorMessage(ErrorMessages.FEATURE_VIEW_NOT_FOUND, featureViewName, projectName), (int)ErrorCode.OBJECT_NOT_FOUND, headers.TraceId);
            }

            this._dbContext.FeatureViews.Remove(featureViewEntity);
            await this._dbContext._SaveChangesAsync();

            return;
        }

        public async Task<FeatureViewResponse> GetFeatureViewAsync(string projectName, string featureViewName, FeastCoreRequestHeaders headers)
        {
            if (!await this._dbContext.Projects.AnyAsync(x => x.ProjectName == projectName))
            {
                throw new FeastCoreNotFoundUserException(
                    ErrorUtils.FormatErrorMessage(ErrorMessages.PROJECT_NOT_FOUND, projectName), (int)ErrorCode.OBJECT_NOT_FOUND, headers.TraceId);
            }

            var featureViewEntity = await this._dbContext.FeatureViews.SingleOrDefaultAsync(x => x.Project == projectName && x.Name == featureViewName);

            if (featureViewEntity == null)
            {
                throw new FeastCoreNotFoundUserException(
                    ErrorUtils.FormatErrorMessage(ErrorMessages.FEATURE_VIEW_NOT_FOUND, featureViewName, projectName), (int)ErrorCode.OBJECT_NOT_FOUND, headers.TraceId);
            }

            return this._featureViewMapper.Map(featureViewEntity);
        }

        public async Task<List<FeatureViewResponse>> ListFeatureViewsAsync(string projectName, FeastCoreRequestHeaders headers)
        {
            if (!await this._dbContext.Projects.AnyAsync(x => x.ProjectName == projectName))
            {
                throw new FeastCoreNotFoundUserException(
                    ErrorUtils.FormatErrorMessage(ErrorMessages.PROJECT_NOT_FOUND, projectName), (int)ErrorCode.OBJECT_NOT_FOUND, headers.TraceId);
            }

            var result = await this._dbContext.FeatureViews.Select(x => this._featureViewMapper.Map(x)).ToListAsync();

            return result;
        }
        #endregion

        #region Entities

        public async Task<EntityResponse> CreateOrUpdateEntityAsync(string projectName, string entityName, EntityRequest request, FeastCoreRequestHeaders headers)
        {
            if (!await this._dbContext.Projects.AnyAsync(x => x.ProjectName == projectName))
            {
                throw new FeastCoreNotFoundUserException(
                    ErrorUtils.FormatErrorMessage(ErrorMessages.PROJECT_NOT_FOUND, projectName), (int)ErrorCode.OBJECT_NOT_FOUND, headers.TraceId);
            }

            var entityEntity = await this._dbContext.Entities.SingleOrDefaultAsync(x => x.Project == projectName && x.Name == entityName);

            if (entityEntity == null)
            {
                entityEntity = this._entityMapper.Map(request);

                // Project is an optional parameter in the client library
                if (string.IsNullOrEmpty(entityEntity.Project))
                {
                    entityEntity.Project = projectName;
                }
                this._dbContext.Entities.Add(entityEntity);
                await _dbContext._SaveChangesAsync();
            }
            else
            {
                entityEntity.CopyFrom(this._entityMapper.Map(request));

                // Project is an optional parameter in the client library
                if (string.IsNullOrEmpty(entityEntity.Project))
                {
                    entityEntity.Project = projectName;
                }

                entityEntity.LastUpdatedTime = DateTime.UtcNow;

                this._dbContext.Entities.Update(entityEntity);
                await this._dbContext._SaveChangesAsync();
            }


            return this._entityMapper.Map(entityEntity);
        }

        public async Task<EntityResponse> CreateEntityAsync(string projectName, string entityName, EntityRequest request, FeastCoreRequestHeaders headers)
        {
            if (!await this._dbContext.Projects.AnyAsync(x => x.ProjectName == projectName))
            {
                throw new FeastCoreNotFoundUserException(
                    ErrorUtils.FormatErrorMessage(ErrorMessages.PROJECT_NOT_FOUND, projectName), (int)ErrorCode.OBJECT_NOT_FOUND, headers.TraceId);
            }

            if (await this._dbContext.Entities.AnyAsync(x => x.Project == projectName && x.Name == entityName))
            {
                throw new FeastCoreConflictUserException(
                    ErrorUtils.FormatErrorMessage(ErrorMessages.ENTITY_ALREADY_EXIST, entityName, projectName), (int)ErrorCode.OBJECT_ALREDAY_EXIST, headers.TraceId);
            }

            var entityEntity = this._entityMapper.Map(request);

            // Project is an optional parameter in the client library
            if (string.IsNullOrEmpty(entityEntity.Project))
            {
                entityEntity.Project = projectName;
            }
            this._dbContext.Entities.Add(entityEntity);
            await _dbContext._SaveChangesAsync();

            return this._entityMapper.Map(entityEntity);

        }

        public async Task<EntityResponse> UpdateEntityAsync(string projectName, string entityName, EntityRequest request, FeastCoreRequestHeaders headers)
        {
            if (!await this._dbContext.Projects.AnyAsync(x => x.ProjectName == projectName))
            {
                throw new FeastCoreNotFoundUserException(
                    ErrorUtils.FormatErrorMessage(ErrorMessages.PROJECT_NOT_FOUND, projectName), (int)ErrorCode.OBJECT_NOT_FOUND, headers.TraceId);
            }

            var entityEntity = await this._dbContext.Entities.SingleOrDefaultAsync(x => x.Project == projectName && x.Name == entityName);

            if (entityEntity == null)
            {
                throw new FeastCoreNotFoundUserException(
                    ErrorUtils.FormatErrorMessage(ErrorMessages.ENTITY_NOT_FOUND, entityName, projectName), (int)ErrorCode.OBJECT_NOT_FOUND, headers.TraceId);
            }

            entityEntity.CopyFrom(this._entityMapper.Map(request));

            // Project is an optional parameter in the client library
            if (string.IsNullOrEmpty(entityEntity.Project))
            {
                entityEntity.Project = projectName;
            }

            entityEntity.LastUpdatedTime = DateTime.UtcNow;

            this._dbContext.Entities.Update(entityEntity);
            await this._dbContext._SaveChangesAsync();

            return this._entityMapper.Map(entityEntity);
        }

        public async Task DeleteEntityAsync(string projectName, string entityName, FeastCoreRequestHeaders headers)
        {
            if (!await this._dbContext.Projects.AnyAsync(x => x.ProjectName == projectName))
            {
                throw new FeastCoreNotFoundUserException(
                    ErrorUtils.FormatErrorMessage(ErrorMessages.PROJECT_NOT_FOUND, projectName), (int)ErrorCode.OBJECT_NOT_FOUND, headers.TraceId);
            }

            var entityEntity = await this._dbContext.Entities.SingleOrDefaultAsync(x => x.Project == projectName && x.Name == entityName);

            if (entityEntity == null)
            {
                throw new FeastCoreNotFoundUserException(
                    ErrorUtils.FormatErrorMessage(ErrorMessages.ENTITY_NOT_FOUND, entityName, projectName), (int)ErrorCode.OBJECT_NOT_FOUND, headers.TraceId);
            }

            this._dbContext.Entities.Remove(entityEntity);
            await this._dbContext._SaveChangesAsync();

            return;
        }

        public async Task<EntityResponse> GetEntityAsync(string projectName, string entityName, FeastCoreRequestHeaders headers)
        {
            if (!await this._dbContext.Projects.AnyAsync(x => x.ProjectName == projectName))
            {
                throw new FeastCoreNotFoundUserException(
                    ErrorUtils.FormatErrorMessage(ErrorMessages.PROJECT_NOT_FOUND, projectName), (int)ErrorCode.OBJECT_NOT_FOUND, headers.TraceId);
            }

            var entityEntity = await this._dbContext.Entities.SingleOrDefaultAsync(x => x.Project == projectName && x.Name == entityName);

            if (entityEntity == null)
            {
                throw new FeastCoreNotFoundUserException(
                    ErrorUtils.FormatErrorMessage(ErrorMessages.ENTITY_NOT_FOUND, entityName, projectName), (int)ErrorCode.OBJECT_NOT_FOUND, headers.TraceId);
            }

            return this._entityMapper.Map(entityEntity);
        }

        public async Task<List<EntityResponse>> ListEntitiesAsync(string projectName, FeastCoreRequestHeaders headers)
        {
            if (!await this._dbContext.Projects.AnyAsync(x => x.ProjectName == projectName))
            {
                throw new FeastCoreNotFoundUserException(
                    ErrorUtils.FormatErrorMessage(ErrorMessages.PROJECT_NOT_FOUND, projectName), (int)ErrorCode.OBJECT_NOT_FOUND, headers.TraceId);
            }

            var result = await this._dbContext.Entities.Select(x => this._entityMapper.Map(x)).ToListAsync();

            return result;
        }
        #endregion

        #region Feature Services
        public async Task<FeatureServiceResponse> CreateOrUpdateFeatureServiceAsync(string projectName, string featureServiceName, FeatureServiceRequest request, FeastCoreRequestHeaders headers)
        {
            if (!await this._dbContext.Projects.AnyAsync(x => x.ProjectName == projectName))
            {
                throw new FeastCoreNotFoundUserException(
                    ErrorUtils.FormatErrorMessage(ErrorMessages.PROJECT_NOT_FOUND, projectName), (int)ErrorCode.OBJECT_NOT_FOUND, headers.TraceId);
            }

            var featureServiceEntity = await this._dbContext.FeatureServices.SingleOrDefaultAsync(x => x.Project == projectName && x.Name == featureServiceName);

            if (featureServiceEntity == null)
            {
                featureServiceEntity = this._featureServiceMapper.Map(request);

                // Project is an optional parameter in the client library
                if (string.IsNullOrEmpty(featureServiceEntity.Project))
                {
                    featureServiceEntity.Project = projectName;
                }

                this._dbContext.FeatureServices.Add(featureServiceEntity);
                await _dbContext._SaveChangesAsync();
            }
            else
            {
                featureServiceEntity.CopyFrom(this._featureServiceMapper.Map(request));

                // Project is an optional parameter in the client library
                if (string.IsNullOrEmpty(featureServiceEntity.Project))
                {
                    featureServiceEntity.Project = projectName;
                }

                featureServiceEntity.LastUpdatedTime = DateTime.UtcNow;

                this._dbContext.FeatureServices.Update(featureServiceEntity);
                await this._dbContext._SaveChangesAsync();
            }

            return this._featureServiceMapper.Map(featureServiceEntity);
        }

        public async Task<FeatureServiceResponse> CreateFeatureServiceAsync(string projectName, string featureServiceName, FeatureServiceRequest request, FeastCoreRequestHeaders headers)
        {
            if (!await this._dbContext.Projects.AnyAsync(x => x.ProjectName == projectName))
            {
                throw new FeastCoreNotFoundUserException(
                    ErrorUtils.FormatErrorMessage(ErrorMessages.PROJECT_NOT_FOUND, projectName), (int)ErrorCode.OBJECT_NOT_FOUND, headers.TraceId);
            }

            if (await this._dbContext.FeatureServices.AnyAsync(x => x.Project == projectName && x.Name == featureServiceName))
            {
                throw new FeastCoreConflictUserException(
                    ErrorUtils.FormatErrorMessage(ErrorMessages.FEATURE_SERVICE_ALREADY_EXIST, featureServiceName, projectName), (int)ErrorCode.OBJECT_ALREDAY_EXIST, headers.TraceId);
            }

            var featureServiceEntity = this._featureServiceMapper.Map(request);

            // Project is an optional parameter in the client library
            if (string.IsNullOrEmpty(featureServiceEntity.Project))
            {
                featureServiceEntity.Project = projectName;
            }

            this._dbContext.FeatureServices.Add(featureServiceEntity);
            await _dbContext._SaveChangesAsync();

            return this._featureServiceMapper.Map(featureServiceEntity);

        }

        public async Task<FeatureServiceResponse> UpdateFeatureServiceAsync(string projectName, string featureServiceName, FeatureServiceRequest request, FeastCoreRequestHeaders headers)
        {
            if (!await this._dbContext.Projects.AnyAsync(x => x.ProjectName == projectName))
            {
                throw new FeastCoreNotFoundUserException(
                    ErrorUtils.FormatErrorMessage(ErrorMessages.PROJECT_NOT_FOUND, projectName), (int)ErrorCode.OBJECT_NOT_FOUND, headers.TraceId);
            }

            var featureServiceEntity = await this._dbContext.FeatureServices.SingleOrDefaultAsync(x => x.Project == projectName && x.Name == featureServiceName);

            if (featureServiceEntity == null)
            {
                throw new FeastCoreNotFoundUserException(
                    ErrorUtils.FormatErrorMessage(ErrorMessages.FEATURE_SERVICE_NOT_FOUND, featureServiceName, projectName), (int)ErrorCode.OBJECT_NOT_FOUND, headers.TraceId);
            }

            featureServiceEntity.CopyFrom(this._featureServiceMapper.Map(request));

            // Project is an optional parameter in the client library
            if (string.IsNullOrEmpty(featureServiceEntity.Project))
            {
                featureServiceEntity.Project = projectName;
            }

            featureServiceEntity.LastUpdatedTime = DateTime.UtcNow;

            this._dbContext.FeatureServices.Update(featureServiceEntity);
            await this._dbContext._SaveChangesAsync();

            return this._featureServiceMapper.Map(featureServiceEntity);
        }

        public async Task DeleteFeatureServiceAsync(string projectName, string featureServiceName, FeastCoreRequestHeaders headers)
        {
            if (!await this._dbContext.Projects.AnyAsync(x => x.ProjectName == projectName))
            {
                throw new FeastCoreNotFoundUserException(
                    ErrorUtils.FormatErrorMessage(ErrorMessages.PROJECT_NOT_FOUND, projectName), (int)ErrorCode.OBJECT_NOT_FOUND, headers.TraceId);
            }

            var featureServiceEntity = await this._dbContext.FeatureServices.SingleOrDefaultAsync(x => x.Project == projectName && x.Name == featureServiceName);

            if (featureServiceEntity == null)
            {
                throw new FeastCoreNotFoundUserException(
                    ErrorUtils.FormatErrorMessage(ErrorMessages.FEATURE_SERVICE_NOT_FOUND, featureServiceName, projectName), (int)ErrorCode.OBJECT_NOT_FOUND, headers.TraceId);
            }

            this._dbContext.FeatureServices.Remove(featureServiceEntity);
            await this._dbContext._SaveChangesAsync();

            return;
        }

        public async Task<FeatureServiceResponse> GetFeatureServiceAsync(string projectName, string featureServiceName, FeastCoreRequestHeaders headers)
        {
            if (!await this._dbContext.Projects.AnyAsync(x => x.ProjectName == projectName))
            {
                throw new FeastCoreNotFoundUserException(
                    ErrorUtils.FormatErrorMessage(ErrorMessages.PROJECT_NOT_FOUND, projectName), (int)ErrorCode.OBJECT_NOT_FOUND, headers.TraceId);
            }

            var featureServiceEntity = await this._dbContext.FeatureServices.SingleOrDefaultAsync(x => x.Project == projectName && x.Name == featureServiceName);

            if (featureServiceEntity == null)
            {
                throw new FeastCoreNotFoundUserException(
                    ErrorUtils.FormatErrorMessage(ErrorMessages.FEATURE_SERVICE_NOT_FOUND, featureServiceName, projectName), (int)ErrorCode.OBJECT_NOT_FOUND, headers.TraceId);
            }

            return this._featureServiceMapper.Map(featureServiceEntity);
        }

        public async Task<List<FeatureServiceResponse>> ListFeatureServicesAsync(string projectName, FeastCoreRequestHeaders headers)
        {
            if (!await this._dbContext.Projects.AnyAsync(x => x.ProjectName == projectName))
            {
                throw new FeastCoreNotFoundUserException(
                    ErrorUtils.FormatErrorMessage(ErrorMessages.PROJECT_NOT_FOUND, projectName), (int)ErrorCode.OBJECT_NOT_FOUND, headers.TraceId);
            }

            var result = await this._dbContext.FeatureServices.Select(x => this._featureServiceMapper.Map(x)).ToListAsync();

            return result;
        }
        #endregion

        #region Projects

        public async Task<ProjectResponse> CreateProjectAsync(string projectName, ProjectRequest request, FeastCoreRequestHeaders headers)
        {
            if (await this._dbContext.Projects.AnyAsync(x => x.ProjectName == projectName))
            {
                throw new FeastCoreConflictUserException(
                    ErrorUtils.FormatErrorMessage(ErrorMessages.PROJECT_ALREADY_EXIST, projectName), (int)ErrorCode.OBJECT_ALREDAY_EXIST, headers.TraceId);
            }

            var projectEntity = this._projectMapper.Map(request);
            projectEntity.CreatedTime = DateTime.UtcNow;
            projectEntity.LastUpdatedTime = projectEntity.CreatedTime;
            this._dbContext.Projects.Add(projectEntity);
            await _dbContext._SaveChangesAsync();

            return this._projectMapper.Map(projectEntity);

        }

        public async Task<ProjectResponse> UpdateProjectAsync(string projectName, ProjectRequest request, FeastCoreRequestHeaders headers)
        {
            var projectEntity = await this._dbContext.Projects.SingleOrDefaultAsync(x => x.ProjectName == projectName);

            if (projectEntity == null)
            {
                throw new FeastCoreNotFoundUserException(
                    ErrorUtils.FormatErrorMessage(ErrorMessages.PROJECT_NOT_FOUND, projectName), (int)ErrorCode.OBJECT_NOT_FOUND, headers.TraceId);
            }

            projectEntity.CopyFrom(this._projectMapper.Map(request));
            projectEntity.LastUpdatedTime = DateTime.UtcNow;

            this._dbContext.Projects.Update(projectEntity);
            await this._dbContext._SaveChangesAsync();

            return this._projectMapper.Map(projectEntity);
        }

        public async Task DeleteProjectAsync(string projectName, FeastCoreRequestHeaders headers)
        {
            var projectEntity = await this._dbContext.Projects.SingleOrDefaultAsync(x => x.ProjectName == projectName);

            if (projectEntity == null)
            {
                throw new FeastCoreNotFoundUserException(
                    ErrorUtils.FormatErrorMessage(ErrorMessages.PROJECT_NOT_FOUND, projectName), (int)ErrorCode.OBJECT_NOT_FOUND, headers.TraceId);
            }

            if (await this._dbContext.Entities.AnyAsync(x => x.Project == projectName && x.Name != "__dummy") ||
                await this._dbContext.FeatureViews.AnyAsync(x => x.Project == projectName) ||
                await this._dbContext.FeatureServices.AnyAsync(x => x.Project == projectName))
            {
                throw new FeastCoreConflictUserException(
                    ErrorUtils.FormatErrorMessage(ErrorMessages.PROJECT_CAN_NOT_BE_DELETED, projectName), (int)ErrorCode.PROJECT_NOT_EMPTY, headers.TraceId);
            }

            this._dbContext.Projects.Remove(projectEntity);
            await this._dbContext._SaveChangesAsync();

            return;
        }

        public async Task<ProjectResponse> GetProjectAsync(string projectName, FeastCoreRequestHeaders headers)
        {
            var projectEntity = await this._dbContext.Projects.SingleOrDefaultAsync(x => x.ProjectName == projectName);

            if (projectEntity == null)
            {
                throw new FeastCoreNotFoundUserException(
                    ErrorUtils.FormatErrorMessage(ErrorMessages.PROJECT_NOT_FOUND, projectName), (int)ErrorCode.OBJECT_NOT_FOUND, headers.TraceId);
            }

            return this._projectMapper.Map(projectEntity);
        }

        public async Task<List<ProjectResponse>> ListProjectsAsync(FeastCoreRequestHeaders headers)
        {
            var result = await this._dbContext.Projects.Select(x => this._projectMapper.Map(x)).ToListAsync();

            return result;
        }
        #endregion
    }
}
