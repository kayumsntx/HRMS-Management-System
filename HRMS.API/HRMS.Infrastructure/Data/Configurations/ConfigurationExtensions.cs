using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HRMS.Infrastructure.Data.Configurations;

internal static class ConfigurationExtensions
{
    // Foreign key WITHOUT navigation property and WITHOUT cascade delete.
    public static void RestrictFk<TEntity, TPrincipal>(this EntityTypeBuilder<TEntity> builder, Expression<Func<TEntity, object?>> foreignKey)
        where TEntity : class where TPrincipal : class
        => builder.HasOne<TPrincipal>().WithMany().HasForeignKey(foreignKey).OnDelete(DeleteBehavior.Restrict);
}
