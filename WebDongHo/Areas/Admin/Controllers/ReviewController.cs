using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebDongHo.Models;
using X.PagedList;
using WebDongHo.Models.Authentication;

namespace WebDongHo.Areas.Admin.Controllers
{
    [Area("admin")]
    [Route("admin")]
    [Route("admin/homeadmin")]
    [Authentication]
    public class ReviewController : Controller
    {
        QlwebDongHoContext DbContext = new QlwebDongHoContext();
        [Route("")]
        [Route("danhmucdanhgia")]
        public IActionResult DanhMucDanhGia(int? page)
        {
            var userRoleId = HttpContext.Session.GetInt32("RoleId");
            if (userRoleId == null || userRoleId != 1)
            {
                return View("AccessDenied");
            }
            int pageSize = 12;
            int pageNumber = page == null || page < 0 ? 1 : page.Value;
            var listDanhgia = DbContext.ProductReviews.AsNoTracking().OrderBy(x => x.ReviewDate).Include(p => p.Product);
            PagedList<ProductReview> listp = new PagedList<ProductReview>(listDanhgia, pageNumber, pageSize);
            return View(listp);
        }
    }
}
