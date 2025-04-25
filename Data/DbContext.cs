using API.Models;
using Microsoft.EntityFrameworkCore;

namespace API.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users { get; set; }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>()
        .Property<string>("Password")
        .HasColumnName("Password")
        .IsRequired();

        base.OnModelCreating(modelBuilder);
    }
}