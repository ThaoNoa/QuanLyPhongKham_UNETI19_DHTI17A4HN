// Họ và tên: Trần Phương Thảo (Nhóm trưởng)
// Mã sinh viên: 23103100226
// Nội dung thực hiện: Hàm tiện ích dùng chung - lấy tên hiển thị [Display] của enum và định dạng tiền.
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Reflection;

namespace QuanLyPhongKham_UNETI19_DHTI17A4HN.Helpers
{
    public static class EnumHelper
    {
        public static string TenHienThi(this Enum giaTri)
        {
            var thuocTinh = giaTri.GetType().GetMember(giaTri.ToString()).FirstOrDefault()
                ?.GetCustomAttribute<DisplayAttribute>();
            return thuocTinh?.GetName() ?? giaTri.ToString();
        }

        public static string TenVietTat(this Enum giaTri)
        {
            var thuocTinh = giaTri.GetType().GetMember(giaTri.ToString()).FirstOrDefault()
                ?.GetCustomAttribute<DisplayAttribute>();
            return thuocTinh?.GetShortName() ?? giaTri.TenHienThi();
        }

        private static readonly CultureInfo _vi = new("vi-VN");

        public static string DinhDangTien(this decimal soTien) => soTien.ToString("#,##0", _vi) + " đ";
    }
}
