// Họ và tên: Trần Hoài Nam
// Mã sinh viên: 23103100240
// Nội dung thực hiện: Bộ lọc phân quyền đặt trên Controller/Action.
// - Chưa đăng nhập: chuyển về trang Đăng nhập (kèm ReturnUrl).
// - Đăng nhập nhưng sai vai trò: chuyển tới trang Từ chối truy cập.
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
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
