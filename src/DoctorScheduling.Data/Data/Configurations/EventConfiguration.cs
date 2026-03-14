using DoctorScheduling.Models.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DoctorScheduling.Data.Configurations;

public class EventConfiguration : IEntityTypeConfiguration<Event>
{
    public void Configure(EntityTypeBuilder<Event> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");

        builder.Property(e => e.Title).IsRequired().HasMaxLength(200);
        builder.Property(e => e.Description).HasMaxLength(2000);
        builder.Property(e => e.StartTime).IsRequired();
        builder.Property(e => e.EndTime).IsRequired();
        builder.Property(e => e.Location).HasMaxLength(500);
        builder.Property(e => e.CancellationReason).HasMaxLength(500);

        builder.Property(e => e.RowVersion).IsConcurrencyToken();

        builder.HasIndex(e => new { e.StartTime, e.EndTime });
        builder.HasIndex(e => e.IsCancelled);

        builder.HasMany(e => e.Attendees)
            .WithOne(a => a.Event)
            .HasForeignKey(a => a.EventId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasData(SeedData.Events);
    }
}
