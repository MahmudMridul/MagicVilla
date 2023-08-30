using MagicVilla_VillaAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace MagicVilla_VillaAPI.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Villa> Villas { get; set; }
        public DbSet<VillaNumber> VillaNumbers { get; set; }
        public DbSet<LocalUser> LocalUsers { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Villa>()
                .HasData(
                    new Villa
                    {
                        Id = 1,
                        Name = "Villa 66",
                        Description = "Villa description",
                        ImageUrl = "https://picsum.photos/id/66/200/300",
                        Occupancy = 2,
                        Rate = 100,
                        Sqft = 200,
                        Amenity = "Nice Amenity",
                        CreatedDate = DateTime.Now
                    },
                    new Villa
                    {
                        Id = 2,
                        Name = "Villa 67",
                        Description = "Villa description",
                        ImageUrl = "https://picsum.photos/id/67/200/300",
                        Occupancy = 3,
                        Rate = 150,
                        Sqft = 300,
                        Amenity = "Nice Amenity",
                        CreatedDate = DateTime.Now
                    },
                    new Villa
                    {
                        Id = 3,
                        Name = "Villa 68",
                        Description = "Villa description",
                        ImageUrl = "https://picsum.photos/id/68/200/300",
                        Occupancy = 4,
                        Rate = 200,
                        Sqft = 400,
                        Amenity = "Nice Amenity",
                        CreatedDate = DateTime.Now
                    }
                );
        }
    }
}
