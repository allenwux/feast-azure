using Azure.Feast.Data;
using Azure.Feast.Utils;
using Feast.Core;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Azure.Feast.Clients.FeastCore
{
    public interface IFeastCoreClient
    {
        #region Feature Views
        Task<FeatureViewResponse> CreateOrUpdateFeatureViewAsync(string projectName, string featureViewName, FeatureViewRequest request, FeastCoreRequestHeaders headers);

        Task<FeatureViewResponse> CreateFeatureViewAsync(string projectName, string featureViewName, FeatureViewRequest request, FeastCoreRequestHeaders headers);

        Task<FeatureViewResponse> UpdateFeatureViewAsync(string projectName, string featureViewName, FeatureViewRequest request, FeastCoreRequestHeaders headers);

        Task DeleteFeatureViewAsync(string projectName, string featureViewName, FeastCoreRequestHeaders headers);

        Task<FeatureViewResponse> GetFeatureViewAsync(string projectName, string featureViewName, FeastCoreRequestHeaders headers);

        Task<List<FeatureViewResponse>> ListFeatureViewsAsync(string projectName, FeastCoreRequestHeaders headers);
        #endregion

        #region Entity
        Task<EntityResponse> CreateOrUpdateEntityAsync(string projectName, string entityName, EntityRequest request, FeastCoreRequestHeaders headers);

        Task<EntityResponse> CreateEntityAsync(string projectName, string entityName, EntityRequest request, FeastCoreRequestHeaders headers);

        Task<EntityResponse> UpdateEntityAsync(string projectName, string entityName, EntityRequest request, FeastCoreRequestHeaders headers);

        Task DeleteEntityAsync(string projectName, string entityName, FeastCoreRequestHeaders headers);

        Task<EntityResponse> GetEntityAsync(string projectName, string entityName, FeastCoreRequestHeaders headers);

        Task<List<EntityResponse>> ListEntitiesAsync(string projectName, FeastCoreRequestHeaders headers);
        #endregion

        #region Feature Services
        Task<FeatureServiceResponse> CreateOrUpdateFeatureServiceAsync(string projectName, string featureServiceName, FeatureServiceRequest request, FeastCoreRequestHeaders headers);

        Task<FeatureServiceResponse> CreateFeatureServiceAsync(string projectName, string featureServiceName, FeatureServiceRequest request, FeastCoreRequestHeaders headers);

        Task<FeatureServiceResponse> UpdateFeatureServiceAsync(string projectName, string featureServiceName, FeatureServiceRequest request, FeastCoreRequestHeaders headers);

        Task DeleteFeatureServiceAsync(string projectName, string featureServiceName, FeastCoreRequestHeaders headers);

        Task<FeatureServiceResponse> GetFeatureServiceAsync(string projectName, string featureServiceName, FeastCoreRequestHeaders headers);

        Task<List<FeatureServiceResponse>> ListFeatureServicesAsync(string projectName, FeastCoreRequestHeaders headers);
        #endregion

        #region Project

        Task<ProjectResponse> CreateProjectAsync(string projectName, ProjectRequest request, FeastCoreRequestHeaders headers);

        Task<ProjectResponse> UpdateProjectAsync(string projectName, ProjectRequest request, FeastCoreRequestHeaders headers);

        Task DeleteProjectAsync(string projectName, FeastCoreRequestHeaders headers);

        Task<ProjectResponse> GetProjectAsync(string projectName, FeastCoreRequestHeaders headers);

        Task<List<ProjectResponse>> ListProjectsAsync(FeastCoreRequestHeaders headers);

        #endregion
    }
}
