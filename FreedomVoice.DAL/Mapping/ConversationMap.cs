using FreedomVoice.DAL.DbEntities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FreedomVoice.DAL.Mapping
{
    public class ConversationMap : IEntityTypeConfiguration<Conversation>
    {
        public void Configure(EntityTypeBuilder<Conversation> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id).ValueGeneratedNever();
            builder.Property(x => x.LastSyncDate);

            builder.HasOne(x => x.SystemPhone);
            builder.HasOne(x => x.ToPhone);
            builder.HasMany(x => x.Messages).WithOne(x => x.Conversation).IsRequired();

            builder.ToTable("Conversation");
        }
    }
}
