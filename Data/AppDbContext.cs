using Hotel_API.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Hotel_API.Data
{
    public class AppDbContext : IdentityDbContext<ApplicationUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<BaiDang> BaiDang { get; set; }
        public DbSet<BinhLuan> BinhLuan { get; set; }
        public DbSet<BaiDang_LuotThich> BaiDang_LuotThich { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Cấu hình relationships
            builder.Entity<BaiDang>()
                .HasOne(b => b.ApplicationUser)
                .WithMany()
                .HasForeignKey(b => b.NguoiDungId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<BinhLuan>()
                .HasOne(c => c.BaiDang)
                .WithMany()
                .HasForeignKey(c => c.BaiDangId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<BinhLuan>()
                .HasOne(c => c.ApplicationUser)
                .WithMany()
                .HasForeignKey(c => c.NguoiDungId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<BaiDang_LuotThich>()
                .HasOne(l => l.BaiDang)
                .WithMany()
                .HasForeignKey(l => l.baidang_id)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<BaiDang_LuotThich>()
                .HasOne(l => l.ApplicationUser)
                .WithMany()
                .HasForeignKey(l => l.nguoidung_id)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
