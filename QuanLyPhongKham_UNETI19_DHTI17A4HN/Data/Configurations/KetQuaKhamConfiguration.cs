// Họ và tên: Trần Phương Thảo
// Mã sinh viên: 23103100226
// Nội dung thực hiện: Cấu hình Fluent API cho KetQuaKham, DichVuYTe, ChiTietDichVu - LichKham (1) - (0..1) KetQuaKham.
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuanLyPhongKham_UNETI19_DHTI17A4HN.Models.Entities;

namespace QuanLyPhongKham_UNETI19_DHTI17A4HN.Data.Configurations
{
    public class KetQuaKhamConfiguration : IEntityTypeConfiguration<KetQuaKham>
    {
        public void Configure(EntityTypeBuilder<KetQuaKham> e)
        {
            e.ToTable("KetQuaKham");
            e.Property(k => k.TrangThai).HasConversion<int>();

            // LichKham (1) - (0..1) KetQuaKham: mỗi lịch chỉ có tối đa 1 kết quả
            e.HasOne(k => k.LichKham)
             .WithOne(l => l.KetQuaKham)
             .HasForeignKey<KetQuaKham>(k => k.MaLichKham)
             .OnDelete(DeleteBehavior.Cascade);
        }
    }

    public class DichVuYTeConfiguration : IEntityTypeConfiguration<DichVuYTe>
    {
        public void Configure(EntityTypeBuilder<DichVuYTe> e)
        {
            e.ToTable("DichVuYTe");
            e.HasIndex(d => d.TenDichVu).IsUnique();
        }
    }

    public class ChiTietDichVuConfiguration : IEntityTypeConfiguration<ChiTietDichVu>
    {
        public void Configure(EntityTypeBuilder<ChiTietDichVu> e)
        {
            e.ToTable("ChiTietDichVu");
            e.HasOne(c => c.KetQuaKham)
             .WithMany(k => k.ChiTietDichVus)
             .HasForeignKey(c => c.MaKetQua)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(c => c.DichVuYTe)
             .WithMany(d => d.ChiTietDichVus)
             .HasForeignKey(c => c.MaDichVu)
             .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
