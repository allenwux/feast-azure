using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Azure.Feast.Data
{
    public interface ISqlDbContext
    {
        DbSet<UserPermissionEntity> UserPermissions { get; set; }

        DbSet<FeatureViewEntity> FeatureViews { get; set; }

        DbSet<EntityEntity> Entities { get; set; }

        DbSet<FeatureServiceEntity> FeatureServices { get; set; }

        DbSet<ProjectEntity> Projects { get; set; }

        /// <summary>
        /// Save the changes to database
        /// </summary>
        /// <returns></returns>
        Task<int> _SaveChangesAsync();

        /// <summary>
        /// Begin a database transaction
        /// </summary>
        /// <returns>The database transaction</returns>
        Task<IDbContextTransaction> BeginTransactionAsync();
    }
}
