using Microsoft.EntityFrameworkCore;
using VAC_T.Models;

namespace VAC_T.Data
{
    public interface IVact_TDbContext
    {
        DbSet<Company> Company { get; set; }
        DbSet<JobOffer> JobOffer { get; set; }
        DbSet<Solicitation> Solicitation { get; set; }
        DbSet<UserDetailsModel> UserDetailsModel { get; set; }
        DbSet<VAC_TUser> Users { get; set; }

        /// <summary>
        /// Call to async save changes method in DbContext
        /// </summary>
        /// <returns></returns>
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}