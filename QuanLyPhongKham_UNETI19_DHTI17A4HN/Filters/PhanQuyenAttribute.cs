// Họ và tên: Trần Hoài Nam
// Mã sinh viên: 23103100240
// Nội dung thực hiện: Bộ lọc phân quyền đặt trên Controller/Action.
// - Chưa đăng nhập: chuyển về trang Đăng nhập (kèm ReturnUrl).
// - Đăng nhập nhưng sai vai trò: chuyển tới trang Từ chối truy cập.
// - Kiểm tra lại tài khoản trong CSDL ở mỗi lần truy cập: tài khoản đã bị khóa/xóa thì đăng xuất ngay;
//   vai trò bị Admin đổi thì cập nhật lại Session.
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using QuanLyPhongKham_UNETI19_DHTI17A4HN.Data;
using QuanLyPhongKham_UNETI19_DHTI17A4HN.Helpers;
using QuanLyPhongKham_UNETI19_DHTI17A4HN.Models.Enums;

namespace QuanLyPhongKham_UNETI19_DHTI17A4HN.Filters
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public class PhanQuyenAttribute : Attribute, IAuthorizationFilter
    {
        private readonly VaiTro[] _vaiTroChoPhep;

        // Không truyền vai trò = chỉ cần đăng nhập
        public PhanQuyenAttribute(params VaiTro[] vaiTroChoPhep)
        {
            _vaiTroChoPhep = vaiTroChoPhep;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            // Action có [BoQuaPhanQuyen] thì cho qua (VD: trang công khai trong Controller quản trị)
            if (context.ActionDescriptor.EndpointMetadata.OfType<BoQuaPhanQuyenAttribute>().Any())
                return;

            var session = context.HttpContext.Session;
            if (!session.DaDangNhap())
            {
                var returnUrl = context.HttpContext.Request.Path + context.HttpContext.Request.QueryString;
                context.Result = new RedirectToActionResult("DangNhap", "TaiKhoan", new { returnUrl });
                return;
            }

            // Tài khoản đang đăng nhập có thể đã bị Admin khóa/xóa sau khi đăng nhập
            var db = context.HttpContext.RequestServices.GetRequiredService<PhongKhamDbContext>();
            var maTaiKhoan = session.LayMaTaiKhoan();
            var taiKhoan = db.TaiKhoans
                .Where(t => t.MaTaiKhoan == maTaiKhoan)
                .Select(t => new { t.TrangThai, t.VaiTro })
                .FirstOrDefault();
            if (taiKhoan == null || !taiKhoan.TrangThai)
            {
                session.Clear();
                var tempData = context.HttpContext.RequestServices
                    .GetRequiredService<ITempDataDictionaryFactory>().GetTempData(context.HttpContext);
                tempData["Loi"] = "Tài khoản của bạn đã bị khóa hoặc không còn tồn tại. Vui lòng liên hệ quản trị viên.";
                context.Result = new RedirectToActionResult("DangNhap", "TaiKhoan", null);
                return;
            }
            // Vai trò đã được Admin thay đổi -> cập nhật Session để phân quyền đúng
            if (session.LayVaiTro() != taiKhoan.VaiTro)
                session.SetInt32(SessionKeys.VaiTro, (int)taiKhoan.VaiTro);

            if (_vaiTroChoPhep.Length > 0 && !session.CoVaiTro(_vaiTroChoPhep))
            {
                context.Result = new RedirectToActionResult("TuChoiTruyCap", "TaiKhoan", null);
            }
        }
    }

    // Đánh dấu action không cần kiểm tra quyền của Controller
    [AttributeUsage(AttributeTargets.Method)]
    public class BoQuaPhanQuyenAttribute : Attribute
    {
    }
}
