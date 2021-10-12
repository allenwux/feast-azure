using Azure.Feast.Data;
using Azure.Feast.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Azure.Feast.Clients.RBAC
{
    public class RBACClient : IRBACClient
    {
        private readonly ISqlDbContext _dbContext;
        private readonly IDataMapper<UserPermissionRequest, UserPermissionEntity, UserPermissionResponse> _userPermissionMapper;
        private readonly ILogger<RBACClient> _logger;

        public RBACClient(ISqlDbContext dbContext,
            IDataMapper<UserPermissionRequest, UserPermissionEntity, UserPermissionResponse> userPermissionMapper,
            ILogger<RBACClient> logger)
        {
            this._dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            this._userPermissionMapper = userPermissionMapper ?? throw new ArgumentNullException(nameof(userPermissionMapper));
            this._logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<CanAccessResult> CanAccessAsync(CanAccessQuery query)
        {
            throw new NotImplementedException();
        }

        public async Task<UserPermissionResponse> InitRBACAsync(UserPermissionRequest request, FeastCoreRequestHeaders header)
        {
            // Can initialize the RBAC only there's no existing admin
            if (await this._dbContext.UserPermissions.AnyAsync(x => x.Role == (int)UserPermissionRole.ADMIN))
            {
                throw new FeastCoreConflictUserException(
                    ErrorUtils.FormatErrorMessage(ErrorMessages.RBAC_ALREADY_INITIALIZED),
                    (int)ErrorCode.OBJECT_ALREDAY_EXIST,
                    header.TraceId);
            }

            UserPermissionEntity userEntity = this._userPermissionMapper.Map(request);

            this._dbContext.UserPermissions.Add(userEntity);
            userEntity.CreatedTime = DateTime.UtcNow;
            userEntity.LastUpdatedTime = userEntity.CreatedTime;
            await this._dbContext._SaveChangesAsync();

            return this._userPermissionMapper.Map(userEntity);
        }

        public async Task<UserPermissionResponse> AddUserPermissionAsync(UserPermissionRequest request, FeastCoreRequestHeaders header)
        {
            if (await this._dbContext.UserPermissions.AnyAsync(x => x.UserId == request.UserId && x.ProjectName == request.ProjectName && x.Role == request.Role))
            {
                throw new FeastCoreConflictUserException(
                    ErrorUtils.FormatErrorMessage(ErrorMessages.USER_PERMISSION_ALREADY_EXIST,
                        request.UserId,
                        request.ProjectName == null ? "NA" : request.ProjectName,
                        request.Role),
                    (int)ErrorCode.OBJECT_ALREDAY_EXIST, 
                    header.TraceId);
            }

            UserPermissionEntity userEntity = this._userPermissionMapper.Map(request);

            this._dbContext.UserPermissions.Add(userEntity);
            userEntity.CreatedTime = DateTime.UtcNow;
            userEntity.LastUpdatedTime = userEntity.CreatedTime;
            await this._dbContext._SaveChangesAsync();

            return this._userPermissionMapper.Map(userEntity);

        }

        public async Task<UserPermissionResponse> UpdateUserPermissionAsync(UserPermissionRequest request, FeastCoreRequestHeaders header)
        {
            var userEntity = await this._dbContext.UserPermissions.SingleOrDefaultAsync(x => x.UserId == request.UserId && x.ProjectName == request.ProjectName && x.Role == request.Role);

            if (userEntity == null)
            {
                throw new FeastCoreNotFoundUserException(
                    ErrorUtils.FormatErrorMessage(ErrorMessages.USER_PERMISSION_NOT_FOUND,
                        request.UserId,
                        request.ProjectName == null ? "NA" : request.ProjectName,
                        request.Role),
                    (int)ErrorCode.OBJECT_NOT_FOUND, 
                    header.TraceId);
            }

            userEntity.UpdateFrom(this._userPermissionMapper.Map(request));
            userEntity.LastUpdatedTime = DateTime.UtcNow;
            this._dbContext.UserPermissions.Update(userEntity);
            await this._dbContext._SaveChangesAsync();

            return this._userPermissionMapper.Map(userEntity);
        }

        public async Task<UserPermissionResponse> RemoveUserPermissionAsync(UserPermissionRequest request, FeastCoreRequestHeaders header)
        {
            var userEntity = await this._dbContext.UserPermissions.SingleOrDefaultAsync(x => x.UserId == request.UserId && x.ProjectName == request.ProjectName && x.Role == request.Role);

            if (userEntity == null)
            {
                throw new FeastCoreNotFoundUserException(
                    ErrorUtils.FormatErrorMessage(ErrorMessages.USER_PERMISSION_NOT_FOUND,
                        request.UserId,
                        request.ProjectName == null ? "NA" : request.ProjectName,
                        request.Role),
                    (int)ErrorCode.OBJECT_NOT_FOUND,
                    header.TraceId);
            }

            this._dbContext.UserPermissions.Remove(userEntity);
            await this._dbContext._SaveChangesAsync();

            return this._userPermissionMapper.Map(userEntity);
        }

        public async Task<List<UserPermissionResponse>> ListUserPermissionsAsync(string userId, FeastCoreRequestHeaders header)
        {
            var result = await this._dbContext.UserPermissions.Where(x => x.UserId == userId).
                Select(x => this._userPermissionMapper.Map(x)).
                ToListAsync();

            return result;
        }

    }
}
