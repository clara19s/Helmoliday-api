using HELMoliday.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HELMoliday.Data.Configurations;

public class UnfoldingConfiguration : IEntityTypeConfiguration<Unfolding>
{
    public void Configure(EntityTypeBuilder<Unfolding> builder)
    {
        builder.ToTable("Unfoldings");
        builder.HasKey(u => new { u.HolidayId, u.ActivityId });
    }
}
