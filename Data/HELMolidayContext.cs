using HELMoliday.Data.Configurations;
using HELMoliday.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace HELMoliday.Data;

public partial class HELMolidayContext : IdentityDbContext<User, Role, Guid>
{
    public HELMolidayContext(DbContextOptions<HELMolidayContext> options)
        : base(options)
    {
    }

    public DbSet<Activity> Activities { get; set; }

    public DbSet<Holiday> Holidays { get; set; }

    public DbSet<Invitation> Invitations { get; set; }

    public DbSet<ChatMessage> ChatMessages { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfiguration(new UserConfiguration());
        modelBuilder.ApplyConfiguration(new RoleConfiguration());
        modelBuilder.ApplyConfiguration(new InvitationConfiguration());
    }
}