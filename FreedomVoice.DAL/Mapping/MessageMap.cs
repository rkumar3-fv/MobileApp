using FreedomVoice.DAL.DbEntities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FreedomVoice.DAL.Mapping
{
    public class MessageMap : IEntityTypeConfiguration<Message>
    {
        public void Configure(EntityTypeBuilder<Message> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).ValueGeneratedNever();
            builder.Property(x => x.Text).IsRequired();
            builder.Property(x => x.ReadAt);
            builder.Property(x => x.ReceivedAt);
            builder.Property(x => x.SentAt);

            builder.HasOne(x => x.From);
            builder.HasOne(x => x.To);
            builder.ToTable("Message");
        }
    }
}
