using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Azure.Feast.Utils;
using Azure.Feast.Clients.RBAC;
using Azure.Feast.Data;
using Azure.Feast.Clients.FeastCore;

namespace Azure.Feast.Services
{
    public class FeastCoreService
    {
        private readonly ILogger<FeastCoreService> _logger;
        private readonly IRBACClient _rbacClient;
        private readonly IFeastCoreClient _feastCoreClient;

        public FeastCoreService(IRBACClient rbacClient,
            IFeastCoreClient feastCoreClient,
            ILogger<FeastCoreService> logger)
        {
            this._rbacClient = rbacClient ?? throw new ArgumentNullException(nameof(rbacClient));
            this._feastCoreClient = feastCoreClient ?? throw new ArgumentNullException(nameof(feastCoreClient));
            this._logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        #region Feast Core Service

        [FunctionName("InitializeService")]
        public async Task<IActionResult> InitializeService(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "initialize")] HttpRequest req)
        {
            var headers = HttpRequestUtils.ParseHeaders(req);

            using (this._logger.BeginNamedScope(headers))
            {
                this._logger.LogMethodBegin(nameof(this.InitializeService));

                try
                {
                    // Add the current user as admin
                    var permission = new UserPermissionRequest
                    {
                        UserId = headers.UserId,
                        UserName = headers.UserName,
                        Role = (int)UserPermissionRole.ADMIN
                    };

                    await this._rbacClient.InitRBACAsync(permission, headers);

                    return new NoContentResult();
                }
                catch (Exception ex)
                {
                    return ExceptionUtils.HandleException(ex, this._logger, headers);
                }
                finally
                {
                    this._logger.LogMethodEnd(nameof(this.InitializeService));
                }
            }
        }

        #endregion

        #region feature services

        [FunctionName("CreateOrUpdateFeatureService")]
        public async Task<IActionResult> CreateOrUpdateFeatureService(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "projects/{projectName}/featureservices/{featureServiceName}/apply")] HttpRequest req,
            string projectName,
            string featureServiceName)
        {
            var headers = HttpRequestUtils.ParseHeaders(req);

            using (this._logger.BeginNamedScope(headers))
            {
                this._logger.LogMethodBegin(nameof(this.CreateOrUpdateFeatureService));

                try
                {
                    var request = await this.DeserializeRequestBodyAsync<FeatureServiceRequest>(req);
                    var response = await this._feastCoreClient.CreateOrUpdateFeatureServiceAsync(projectName, featureServiceName, request, headers);
                    return new OkObjectResult(response);
                }
                catch (Exception ex)
                {
                    return ExceptionUtils.HandleException(ex, this._logger, headers);
                }
                finally
                {
                    this._logger.LogMethodEnd(nameof(this.CreateOrUpdateFeatureService));
                }
            }
        }

        [FunctionName("CreateFeatureService")]
        public async Task<IActionResult> CreateFeatureService(
            [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "projects/{projectName}/featureservices/{featureServiceName}")] HttpRequest req,
            string projectName,
            string featureServiceName)
        {
            var headers = HttpRequestUtils.ParseHeaders(req);

            using (this._logger.BeginNamedScope(headers))
            {
                this._logger.LogMethodBegin(nameof(this.CreateFeatureService));

                try
                {
                    var request = await this.DeserializeRequestBodyAsync<FeatureServiceRequest>(req);
                    var response = await this._feastCoreClient.CreateFeatureServiceAsync(projectName, featureServiceName, request, headers);
                    return new OkObjectResult(response);
                }
                catch (Exception ex)
                {
                    return ExceptionUtils.HandleException(ex, this._logger, headers);
                }
                finally
                {
                    this._logger.LogMethodEnd(nameof(this.CreateFeatureService));
                }
            }
        }

        [FunctionName("UpdateFeatureService")]
        public async Task<IActionResult> UpdateFeatureService(
            [HttpTrigger(AuthorizationLevel.Anonymous, "patch", Route = "projects/{projectName}/featureservices/{featureServiceName}")] HttpRequest req,
            string projectName,
            string featureServiceName)
        {
            var headers = HttpRequestUtils.ParseHeaders(req);

            using (this._logger.BeginNamedScope(headers))
            {
                this._logger.LogMethodBegin(nameof(this.UpdateFeatureService));

                try
                {
                    var request = await this.DeserializeRequestBodyAsync<FeatureServiceRequest>(req);
                    var response = await this._feastCoreClient.UpdateFeatureServiceAsync(projectName, featureServiceName, request, headers);
                    return new OkObjectResult(response);
                }
                catch (Exception ex)
                {
                    return ExceptionUtils.HandleException(ex, this._logger, headers);
                }
                finally
                {
                    this._logger.LogMethodEnd(nameof(this.UpdateFeatureService));
                }
            }
        }

        [FunctionName("DeleteFeatureService")]
        public async Task<IActionResult> DeleteFeatureService(
            [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "projects/{projectName}/featureservices/{featureServiceName}")] HttpRequest req,
            string projectName,
            string featureServiceName)
        {
            var headers = HttpRequestUtils.ParseHeaders(req);

            using (this._logger.BeginNamedScope(headers))
            {
                this._logger.LogMethodBegin(nameof(this.DeleteFeatureService));

                try
                {
                    await this._feastCoreClient.DeleteFeatureServiceAsync(projectName, featureServiceName, headers);
                    return new NoContentResult();
                }
                catch (Exception ex)
                {
                    return ExceptionUtils.HandleException(ex, this._logger, headers);
                }
                finally
                {
                    this._logger.LogMethodEnd(nameof(this.DeleteFeatureService));
                }
            }

            return new NoContentResult();
        }

        [FunctionName("ListFeatureServices")]
        public async Task<IActionResult> ListFeatureServices(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "projects/{projectName}/featureservices")] HttpRequest req,
            string projectName)
        {
            var headers = HttpRequestUtils.ParseHeaders(req);

            using (this._logger.BeginNamedScope(headers))
            {
                this._logger.LogMethodBegin(nameof(this.ListFeatureServices));

                try
                {
                    var response = await this._feastCoreClient.ListFeatureServicesAsync(projectName, headers);
                    return new OkObjectResult(response);
                }
                catch (Exception ex)
                {
                    return ExceptionUtils.HandleException(ex, this._logger, headers);
                }
                finally
                {
                    this._logger.LogMethodEnd(nameof(this.ListFeatureServices));
                }
            }
        }


        [FunctionName("GetFeatureService")]
        public async Task<IActionResult> GetFeatureService(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "projects/{projectName}/featureservices/{featureServiceName}")] HttpRequest req,
            string projectName,
            string featureServiceName)
        {
            var headers = HttpRequestUtils.ParseHeaders(req);

            using (this._logger.BeginNamedScope(headers))
            {
                this._logger.LogMethodBegin(nameof(this.GetFeatureService));

                try
                {
                    var response = await this._feastCoreClient.GetFeatureServiceAsync(projectName, featureServiceName, headers);
                    return new OkObjectResult(response);
                }
                catch (Exception ex)
                {
                    return ExceptionUtils.HandleException(ex, this._logger, headers);
                }
                finally
                {
                    this._logger.LogMethodEnd(nameof(this.GetFeatureService));
                }
            }
        }

        #endregion

        #region feature views

        [FunctionName("CreateOrUpdateFeatureView")]
        public async Task<IActionResult> CreateOrUpdateFeatureView(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "projects/{projectName}/featureViews/{featureViewName}/apply")] HttpRequest req,
            string projectName,
            string featureViewName)
        {
            var headers = HttpRequestUtils.ParseHeaders(req);

            using (this._logger.BeginNamedScope(headers))
            {
                this._logger.LogMethodBegin(nameof(this.CreateOrUpdateFeatureView));

                try
                {
                    var request = await this.DeserializeRequestBodyAsync<FeatureViewRequest>(req);
                    var response = await this._feastCoreClient.CreateOrUpdateFeatureViewAsync(projectName, featureViewName, request, headers);
                    return new OkObjectResult(response);
                }
                catch (Exception ex)
                {
                    return ExceptionUtils.HandleException(ex, this._logger, headers);
                }
                finally
                {
                    this._logger.LogMethodEnd(nameof(this.CreateOrUpdateFeatureView));
                }
            }
        }

        [FunctionName("CreateFeatureView")]
        public async Task<IActionResult> CreateFeatureView(
            [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "projects/{projectName}/featureViews/{featureViewName}")] HttpRequest req,
            string projectName,
            string featureViewName)
        {
            var headers = HttpRequestUtils.ParseHeaders(req);

            using (this._logger.BeginNamedScope(headers))
            {
                this._logger.LogMethodBegin(nameof(this.CreateFeatureView));

                try
                {
                    var request = await this.DeserializeRequestBodyAsync<FeatureViewRequest>(req);
                    var response = await this._feastCoreClient.CreateFeatureViewAsync(projectName, featureViewName, request, headers);
                    return new OkObjectResult(response);
                }
                catch (Exception ex)
                {
                    return ExceptionUtils.HandleException(ex, this._logger, headers);
                }
                finally
                {
                    this._logger.LogMethodEnd(nameof(this.CreateFeatureView));
                }
            }
        }

        [FunctionName("UpdateFeatureView")]
        public async Task<IActionResult> UpdateFeatureView(
            [HttpTrigger(AuthorizationLevel.Anonymous, "patch", Route = "projects/{projectName}/featureViews/{featureViewName}")] HttpRequest req,
            string projectName,
            string featureViewName)
        {
            var headers = HttpRequestUtils.ParseHeaders(req);

            using (this._logger.BeginNamedScope(headers))
            {
                this._logger.LogMethodBegin(nameof(this.UpdateFeatureView));

                try
                {
                    var request = await this.DeserializeRequestBodyAsync<FeatureViewRequest>(req);
                    var response = await this._feastCoreClient.UpdateFeatureViewAsync(projectName, featureViewName, request, headers);
                    return new OkObjectResult(response);
                }
                catch (Exception ex)
                {
                    return ExceptionUtils.HandleException(ex, this._logger, headers);
                }
                finally
                {
                    this._logger.LogMethodEnd(nameof(this.UpdateFeatureView));
                }
            }
        }

        [FunctionName("DeleteFeatureView")]
        public async Task<IActionResult> DeleteFeatureView(
            [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "projects/{projectName}/featureViews/{featureViewName}")] HttpRequest req,
            string projectName,
            string featureViewName)
        {
            var headers = HttpRequestUtils.ParseHeaders(req);

            using (this._logger.BeginNamedScope(headers))
            {
                this._logger.LogMethodBegin(nameof(this.DeleteFeatureView));

                try
                {
                    await this._feastCoreClient.DeleteFeatureViewAsync(projectName, featureViewName, headers);
                    return new NoContentResult();
                }
                catch (Exception ex)
                {
                    return ExceptionUtils.HandleException(ex, this._logger, headers);
                }
                finally
                {
                    this._logger.LogMethodEnd(nameof(this.DeleteFeatureView));
                }
            }
        }

        [FunctionName("GetFeatureView")]
        public async Task<IActionResult> GetFeatureView(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "projects/{projectName}/featureViews/{featureViewName}")] HttpRequest req,
            string projectName,
            string featureViewName)
        {
            var headers = HttpRequestUtils.ParseHeaders(req);

            using (this._logger.BeginNamedScope(headers))
            {
                this._logger.LogMethodBegin(nameof(this.GetFeatureView));

                try
                {
                    var response = await this._feastCoreClient.GetFeatureViewAsync(projectName, featureViewName, headers);
                    return new OkObjectResult(response);
                }
                catch (Exception ex)
                {
                    return ExceptionUtils.HandleException(ex, this._logger, headers);
                }
                finally
                {
                    this._logger.LogMethodEnd(nameof(this.GetFeatureView));
                }
            }
        }

        [FunctionName("ListFeatureViews")]
        public async Task<IActionResult> ListFeatureViews(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "projects/{projectName}/featureViews")] HttpRequest req,
            string projectName)
        {
            var headers = HttpRequestUtils.ParseHeaders(req);

            using (this._logger.BeginNamedScope(headers))
            {
                this._logger.LogMethodBegin(nameof(this.ListFeatureViews));

                try
                {
                    var response = await this._feastCoreClient.ListFeatureViewsAsync(projectName, headers);
                    return new OkObjectResult(response);
                }
                catch (Exception ex)
                {
                    return ExceptionUtils.HandleException(ex, this._logger, headers);
                }
                finally
                {
                    this._logger.LogMethodEnd(nameof(this.ListFeatureViews));
                }
            }
        }

        #endregion

        #region entities
        [FunctionName("CreateOrUpdateEntity")]
        public async Task<IActionResult> CreateOrUpdateEntity(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "projects/{projectName}/entities/{entityName}/apply")] HttpRequest req,
            string projectName,
            string entityName)
        {
            var headers = HttpRequestUtils.ParseHeaders(req);

            using (this._logger.BeginNamedScope(headers))
            {
                this._logger.LogMethodBegin(nameof(this.CreateOrUpdateEntity));

                try
                {
                    var request = await this.DeserializeRequestBodyAsync<EntityRequest>(req);
                    var response = await this._feastCoreClient.CreateOrUpdateEntityAsync(projectName, entityName, request, headers);
                    return new OkObjectResult(response);
                }
                catch (Exception ex)
                {
                    return ExceptionUtils.HandleException(ex, this._logger, headers);
                }
                finally
                {
                    this._logger.LogMethodEnd(nameof(this.CreateOrUpdateEntity));
                }
            }
        }

        [FunctionName("CreateEntity")]
        public async Task<IActionResult> CreateEntity(
            [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "projects/{projectName}/entities/{entityName}")] HttpRequest req,
            string projectName,
            string entityName)
        {
            var headers = HttpRequestUtils.ParseHeaders(req);

            using (this._logger.BeginNamedScope(headers))
            {
                this._logger.LogMethodBegin(nameof(this.CreateEntity));

                try
                {
                    var request = await this.DeserializeRequestBodyAsync<EntityRequest>(req);
                    var response = await this._feastCoreClient.CreateEntityAsync(projectName, entityName, request, headers);
                    return new OkObjectResult(response);
                }
                catch (Exception ex)
                {
                    return ExceptionUtils.HandleException(ex, this._logger, headers);
                }
                finally
                {
                    this._logger.LogMethodEnd(nameof(this.CreateEntity));
                }
            }
        }

        [FunctionName("UpdateEntity")]
        public async Task<IActionResult> UpdateEntity(
            [HttpTrigger(AuthorizationLevel.Anonymous, "patch", Route = "projects/{projectName}/entities/{entityName}")] HttpRequest req,
            string projectName,
            string entityName)
        {
            var headers = HttpRequestUtils.ParseHeaders(req);

            using (this._logger.BeginNamedScope(headers))
            {
                this._logger.LogMethodBegin(nameof(this.UpdateEntity));

                try
                {
                    var request = await this.DeserializeRequestBodyAsync<EntityRequest>(req);
                    var response = await this._feastCoreClient.UpdateEntityAsync(projectName, entityName, request, headers);
                    return new OkObjectResult(response);
                }
                catch (Exception ex)
                {
                    return ExceptionUtils.HandleException(ex, this._logger, headers);
                }
                finally
                {
                    this._logger.LogMethodEnd(nameof(this.UpdateEntity));
                }
            }
        }

        [FunctionName("DeleteEntity")]
        public async Task<IActionResult> DeleteEntity(
            [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "projects/{projectName}/entities/{entityName}")] HttpRequest req,
            string projectName,
            string entityName)
        {
            var headers = HttpRequestUtils.ParseHeaders(req);

            using (this._logger.BeginNamedScope(headers))
            {
                this._logger.LogMethodBegin(nameof(this.DeleteEntity));

                try
                {
                    await this._feastCoreClient.DeleteEntityAsync(projectName, entityName, headers);
                    return new NoContentResult();
                }
                catch (Exception ex)
                {
                    return ExceptionUtils.HandleException(ex, this._logger, headers);
                }
                finally
                {
                    this._logger.LogMethodEnd(nameof(this.DeleteEntity));
                }
            }
        }

        [FunctionName("GetEntity")]
        public async Task<IActionResult> GetEntity(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "projects/{projectName}/entities/{entityName}")] HttpRequest req,
            string projectName,
            string entityName)
        {
            var headers = HttpRequestUtils.ParseHeaders(req);

            using (this._logger.BeginNamedScope(headers))
            {
                this._logger.LogMethodBegin(nameof(this.GetEntity));

                try
                {
                    var response = await this._feastCoreClient.GetEntityAsync(projectName, entityName, headers);
                    return new OkObjectResult(response);
                }
                catch (Exception ex)
                {
                    return ExceptionUtils.HandleException(ex, this._logger, headers);
                }
                finally
                {
                    this._logger.LogMethodEnd(nameof(this.GetEntity));
                }
            }
        }

        [FunctionName("ListEntities")]
        public async Task<IActionResult> ListEntities(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "projects/{projectName}/entities")] HttpRequest req, string projectName)
        {
            var headers = HttpRequestUtils.ParseHeaders(req);

            using (this._logger.BeginNamedScope(headers))
            {
                this._logger.LogMethodBegin(nameof(this.ListEntities));

                try
                {
                    var response = await this._feastCoreClient.ListEntitiesAsync(projectName, headers);
                    return new OkObjectResult(response);
                }
                catch (Exception ex)
                {
                    return ExceptionUtils.HandleException(ex, this._logger, headers);
                }
                finally
                {
                    this._logger.LogMethodEnd(nameof(this.ListEntities));
                }
            }
        }

        #endregion

        #region projects

        [FunctionName("CreateProject")]
        public async Task<IActionResult> CreateProject(
            [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "projects/{projectName}")] HttpRequest req,
            string projectName)
        {
            var headers = HttpRequestUtils.ParseHeaders(req);

            using (this._logger.BeginNamedScope(headers))
            {
                this._logger.LogMethodBegin(nameof(this.CreateProject));

                try
                {
                    var request = await this.DeserializeRequestBodyAsync<ProjectRequest>(req);
                    var response = await this._feastCoreClient.CreateProjectAsync(projectName, request, headers);
                    return new OkObjectResult(response);
                }
                catch (Exception ex)
                {
                    return ExceptionUtils.HandleException(ex, this._logger, headers);
                }
                finally
                {
                    this._logger.LogMethodEnd(nameof(this.CreateProject));
                }
            }
        }

        [FunctionName("UpdateProject")]
        public async Task<IActionResult> UpdateProject(
            [HttpTrigger(AuthorizationLevel.Anonymous, "patch", Route = "projects/{projectName}")] HttpRequest req,
            string projectName)
        {
            var headers = HttpRequestUtils.ParseHeaders(req);

            using (this._logger.BeginNamedScope(headers))
            {
                this._logger.LogMethodBegin(nameof(this.UpdateProject));

                try
                {
                    var request = await this.DeserializeRequestBodyAsync<ProjectRequest>(req);
                    var response = await this._feastCoreClient.UpdateProjectAsync(projectName, request, headers);
                    return new OkObjectResult(response);
                }
                catch (Exception ex)
                {
                    return ExceptionUtils.HandleException(ex, this._logger, headers);
                }
                finally
                {
                    this._logger.LogMethodEnd(nameof(this.UpdateProject));
                }
            }
        }

        [FunctionName("DeleteProject")]
        public async Task<IActionResult> DeleteProject(
            [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "projects/{projectName}")] HttpRequest req,
            string projectName)
        {
            var headers = HttpRequestUtils.ParseHeaders(req);

            using (this._logger.BeginNamedScope(headers))
            {
                this._logger.LogMethodBegin(nameof(this.DeleteProject));

                try
                {
                    await this._feastCoreClient.DeleteProjectAsync(projectName, headers);
                    return new NoContentResult();
                }
                catch (Exception ex)
                {
                    return ExceptionUtils.HandleException(ex, this._logger, headers);
                }
                finally
                {
                    this._logger.LogMethodEnd(nameof(this.DeleteProject));
                }
            }
        }

        [FunctionName("GetProject")]
        public async Task<IActionResult> GetProject(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "projects/{projectName}")] HttpRequest req,
            string projectName)
        {
            var headers = HttpRequestUtils.ParseHeaders(req);

            using (this._logger.BeginNamedScope(headers))
            {
                this._logger.LogMethodBegin(nameof(this.GetProject));

                try
                {
                    var response = await this._feastCoreClient.GetProjectAsync(projectName, headers);
                    return new OkObjectResult(response);
                }
                catch (Exception ex)
                {
                    return ExceptionUtils.HandleException(ex, this._logger, headers);
                }
                finally
                {
                    this._logger.LogMethodEnd(nameof(this.GetProject));
                }
            }
        }

        [FunctionName("ListProjects")]
        public async Task<IActionResult> ListProjects(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "projects")] HttpRequest req)
        {
            var headers = HttpRequestUtils.ParseHeaders(req);

            using (this._logger.BeginNamedScope(headers))
            {
                this._logger.LogMethodBegin(nameof(this.ListProjects));

                try
                {
                    var response = await this._feastCoreClient.ListProjectsAsync(headers);
                    return new OkObjectResult(response);
                }
                catch (Exception ex)
                {
                    return ExceptionUtils.HandleException(ex, this._logger, headers);
                }
                finally
                {
                    this._logger.LogMethodEnd(nameof(this.ListProjects));
                }
            }
        }

        #endregion

        #region RBAC

        [FunctionName("AddUserPermission")]
        public async Task<IActionResult> AddUserPermission(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "userpermissions/add")] HttpRequest req)
        {
            var headers = HttpRequestUtils.ParseHeaders(req);

            using (this._logger.BeginNamedScope(headers))
            {
                this._logger.LogMethodBegin(nameof(this.AddUserPermission));

                try
                {
                    var request = await this.DeserializeRequestBodyAsync<UserPermissionRequest>(req);
                    var response = await this._rbacClient.AddUserPermissionAsync(request, headers);
                    return new OkObjectResult(response);
                }
                catch (Exception ex)
                {
                    return ExceptionUtils.HandleException(ex, this._logger, headers);
                }
                finally
                {
                    this._logger.LogMethodEnd(nameof(this.AddUserPermission));
                }
            }
        }

        [FunctionName("UpdateUserPermission")]
        public async Task<IActionResult> UpdateUserPermission(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "userpermissions/update")] HttpRequest req)
        {
            var headers = HttpRequestUtils.ParseHeaders(req);

            using (this._logger.BeginNamedScope(headers))
            {
                this._logger.LogMethodBegin(nameof(this.UpdateUserPermission));

                try
                {
                    var request = await this.DeserializeRequestBodyAsync<UserPermissionRequest>(req);
                    var response = await this._rbacClient.UpdateUserPermissionAsync(request, headers);
                    return new OkObjectResult(response);
                }
                catch (Exception ex)
                {
                    return ExceptionUtils.HandleException(ex, this._logger, headers);
                }
                finally
                {
                    this._logger.LogMethodEnd(nameof(this.UpdateUserPermission));
                }
            }
        }

        [FunctionName("RemoveUserPermission")]
        public async Task<IActionResult> RemoveUserPermission(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "userpermissions/remove")] HttpRequest req)
        {
            var headers = HttpRequestUtils.ParseHeaders(req);

            using (this._logger.BeginNamedScope(headers))
            {
                this._logger.LogMethodBegin(nameof(this.RemoveUserPermission));

                try
                {
                    var request = await this.DeserializeRequestBodyAsync<UserPermissionRequest>(req);
                    var response = await this._rbacClient.RemoveUserPermissionAsync(request, headers);
                    return new OkObjectResult(response);
                }
                catch (Exception ex)
                {
                    return ExceptionUtils.HandleException(ex, this._logger, headers);
                }
                finally
                {
                    this._logger.LogMethodEnd(nameof(this.RemoveUserPermission));
                }
            }
        }

        [FunctionName("ListUserPermissions")]
        public async Task<IActionResult> ListUserPermissions(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "userpermissions/{userId}")] HttpRequest req,
            string userId)
        {
            var headers = HttpRequestUtils.ParseHeaders(req);

            using (this._logger.BeginNamedScope(headers))
            {
                this._logger.LogMethodBegin(nameof(this.ListUserPermissions));

                try
                {
                    var response = await this._rbacClient.ListUserPermissionsAsync(userId, headers);
                    return new OkObjectResult(response);
                }
                catch (Exception ex)
                {
                    return ExceptionUtils.HandleException(ex, this._logger, headers);
                }
                finally
                {
                    this._logger.LogMethodEnd(nameof(this.ListUserPermissions));
                }
            }
        }

        #endregion

        #region private methods

        private async Task<T> DeserializeRequestBodyAsync<T>(HttpRequest req)
        {
            string content = await new StreamReader(req.Body).ReadToEndAsync();
            T obj = JsonConvert.DeserializeObject<T>(content);
            return obj;
        }

        #endregion
    }
}
