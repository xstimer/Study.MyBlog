using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Study.Net.Model;

namespace Study.Net.EFCoreEnvironment.Configs;

public class UserConfig : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasMany(x => x.Articles).WithOne(x=>x.User).HasForeignKey(x=>x.UserId);

        builder.HasQueryFilter(x => x.isDeleted == false);
    }
}
