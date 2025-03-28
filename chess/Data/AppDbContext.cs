using chess.dto.user;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace chess.Data
{
    public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
    {
        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                entity.Property(e => e.UserName).IsRequired();
                entity.Property(e => e.Email).IsRequired();
                entity.HasIndex(e => e.Email).IsUnique();
                entity.Property(e => e.Password).IsRequired();
                entity.Property(e => e.CodeVerification).IsRequired(false);
                entity.Property(e => e.CodeVerificationExpiration).IsRequired(false);

                // Configuración para soft delete
                entity.Property(e => e.IsDeleted).HasDefaultValue(false);
                entity.Property(e => e.DeletedAt).IsRequired(false);
                entity.Property(e => e.DeletedBy).IsRequired(false);

                // Filtro global para excluir registros eliminados
                entity.HasQueryFilter(u => !u.IsDeleted);
            });
        }

        public override int SaveChanges()
        {
            UpdateSoftDeleteStatuses();
            return base.SaveChanges();
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            UpdateSoftDeleteStatuses();
            return base.SaveChangesAsync(cancellationToken);
        }

        private void UpdateSoftDeleteStatuses()
        {
            foreach (var entry in ChangeTracker.Entries<User>())
            {
                if (entry.State == EntityState.Deleted)
                {
                    entry.State = EntityState.Modified;
                    entry.Entity.IsDeleted = true;
                    entry.Entity.DeletedAt = DateTime.UtcNow;
                    // Aquí podrías setear el usuario que eliminó si tienes autenticación
                    // entry.Entity.DeletedBy = _currentUserService.UserId;
                }
            }
        }
    }
}