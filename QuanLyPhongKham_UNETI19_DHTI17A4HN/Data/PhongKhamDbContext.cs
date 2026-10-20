// Họ và tên: Trần Phương Thảo (Nhóm trưởng)
// Mã sinh viên: 23103100226
// Nội dung thực hiện: Tạo DbContext, khai báo DbSet cho các Entity và nạp cấu hình Fluent API.
// Cấu hình chi tiết của từng Entity nằm trong thư mục Data/Configurations (mỗi Module một file).
using Microsoft.EntityFrameworkCore;
using QuanLyPhongKham_UNETI19_DHTI17A4HN.Models.Entities;

namespace QuanLyPhongKham_UNETI19_DHTI17A4HN.Data
{
    public class PhongKhamDbContext : DbContext
    {
        public PhongKhamDbContext(DbContextOptions<PhongKhamDbContext> options) : base(options)
        {
        }

        public DbSet<TaiKhoan> TaiKhoans { get; set; } = null!;
        public DbSet<ChuyenKhoa> ChuyenKhoas { get; set; } = null!;
        public DbSet<BacSi> BacSis { get; set; } = null!;
        public DbSet<BenhNhan> BenhNhans { get; set; } = null!;
        public DbSet<LichKham> LichKhams { get; set; } = null!;
        public DbSet<KetQuaKham> KetQuaKhams { get; set; } = null!;
        public DbSet<DichVuYTe> DichVuYTes { get; set; } = null!;
        public DbSet<ChiTietDichVu> ChiTietDichVus { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Tự động áp dụng mọi lớp IEntityTypeConfiguration<T> trong project (thư mục Data/Configurations)
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(PhongKhamDbContext).Assembly);
        }
    }
}
