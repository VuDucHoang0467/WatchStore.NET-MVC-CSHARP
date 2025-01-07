using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
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
    public class HomeAdminController : Controller
    {
        QlwebDongHoContext DbContext = new QlwebDongHoContext();
        [Route("")]
        [Route("index")]
        public IActionResult Index()
        {
            return View();
        }

        [Route("danhmucsanpham")]
        public IActionResult DanhMucSanPham(int? page)
        {
            int pageSize = 12;
            int pageNumber = page == null || page < 0 ? 1 : page.Value;
            var listSanpham = DbContext.Products.AsNoTracking().OrderBy(x => x.Name).Include(p => p.Brand).Include(p => p.Category);
            PagedList<Product> listp = new PagedList<Product>(listSanpham, pageNumber, pageSize);
            return View(listp);         
        }

        [Route("danhmuchang")]
        public IActionResult DanhMucHang(int? page)
        {
            int pageSize = 12;
            int pageNumber = page == null || page < 0 ? 1 : page.Value;
            var listHang = DbContext.Brands.AsNoTracking().OrderBy(x => x.BrandId);
            PagedList<Brand> listp = new PagedList<Brand>(listHang, pageNumber, pageSize);
            return View(listp);
        }

        [Route("danhmucloai")]
        public IActionResult DanhMucLoai(int? page)
        {
            int pageSize = 12;
            int pageNumber = page == null || page < 0 ? 1 : page.Value;
            var listLoai = DbContext.Categories.AsNoTracking().OrderBy(x => x.CategoryId);
            PagedList<Category> listp = new PagedList<Category>(listLoai, pageNumber, pageSize);
            return View(listp);
        }

        [Route("danhmuccoupon")]
        public IActionResult DanhMucCoupon(int? page)
        {
            int pageSize = 12;
            int pageNumber = page == null || page < 0 ? 1 : page.Value;
            var listCoupon = DbContext.DiscountCodes.AsNoTracking().OrderBy(x => x.Id);
            PagedList<DiscountCode> listp = new PagedList<DiscountCode>(listCoupon, pageNumber, pageSize);
            return View(listp);
        }

        [Route("danhmucdonhang")]
        public IActionResult DanhMucDonHang(int? page)
        {
            List<SelectListItem> statusList = new List<SelectListItem>
            {
                new SelectListItem { Value = "0", Text = "Đã đặt" },
                new SelectListItem { Value = "1", Text = "Đã giao" }
            };
            ViewBag.StatusList = statusList;
            int pageSize = 12;
            int pageNumber = page == null || page < 0 ? 1 : page.Value;
            var listDonhang = DbContext.Orders.AsNoTracking().OrderBy(x => x.Id);
            PagedList<Order> listp = new PagedList<Order>(listDonhang, pageNumber, pageSize);
            return View(listp);
        }

        [Route("danhmucctdonhang")]
        public IActionResult DanhMucCTDH(int? page)
        {
            int pageSize = 12;
            int pageNumber = page == null || page < 0 ? 1 : page.Value;
            var listCTDonhang = DbContext.OrderDetails.AsNoTracking().OrderBy(x => x.Id).Include(p => p.Product);
            PagedList<OrderDetail> listp = new PagedList<OrderDetail>(listCTDonhang, pageNumber, pageSize);
            return View(listp);
        }

        [Route("danhmucanhsanpham")]
        public IActionResult DanhMucAnhSanPham(int? page)
        {
            int pageSize = 12;
            int pageNumber = page == null || page < 0 ? 1 : page.Value;
            var listanhSanpham = DbContext.ProductImages.AsNoTracking().OrderBy(x => x.ProductId).Include(p => p.Product);
            PagedList<ProductImage> listp = new PagedList<ProductImage>(listanhSanpham, pageNumber, pageSize);
            return View(listp);
        }

        //QUAN LY SAN PHAM BEGIN
        [Route("ThemSanPham")]
        public IActionResult ThemSanPham()
        {
            ViewBag.CategoryId = new SelectList(DbContext.Categories.ToList(), "CategoryId", "Name");
            ViewBag.BrandId = new SelectList(DbContext.Brands.ToList(), "BrandId", "Name");
            return View();
        }

        private async Task<string> SaveImage(IFormFile image)
        {
            var imageName = Path.GetFileName(image.FileName);
            var imagePath = Path.Combine("wwwroot/SLayoutWatch/img", imageName);
            using (var fileStream = new FileStream(imagePath, FileMode.Create))
            {
                await image.CopyToAsync(fileStream);
            }
            return "" + imageName;
        }

        [Route("ThemSanPham")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ThemSanPham(Product product, IFormFile image)
        {
            if (ModelState.IsValid)
            {
                if (image != null && image.Length > 0)
                {
                    product.Image = await SaveImage(image);
                }

                DbContext.Products.Add(product);
                await DbContext.SaveChangesAsync();
                return RedirectToAction("DanhMucSanPham");
            }
            ViewBag.CategoryId = new SelectList(DbContext.Categories.ToList(), "CategoryId", "Name");
            ViewBag.BrandId = new SelectList(DbContext.Brands.ToList(), "BrandId", "Name");
            return View(product);
        }

        [Route("SuaSanPham")]
        public IActionResult SuaSanPham(int productId)
        {
            ViewBag.CategoryId = new SelectList(DbContext.Categories.ToList(), "CategoryId", "Name");
            ViewBag.BrandId = new SelectList(DbContext.Brands.ToList(), "BrandId", "Name");
            var sanPham = DbContext.Products.Find(productId);
            return View(sanPham);
        }

        [Route("SuaSanPham")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SuaSanPham(Product product, IFormFile image)
        {
            if (ModelState.IsValid)
            {
                if (image != null && image.Length > 0)
                {
                    product.Image = await SaveImage(image);
                }
                DbContext.Products.Update(product);
                await DbContext.SaveChangesAsync();
                return RedirectToAction("DanhMucSanPham");
            }
            ViewBag.CategoryId = new SelectList(DbContext.Categories.ToList(), "CategoryId", "Name");
            ViewBag.BrandId = new SelectList(DbContext.Brands.ToList(), "BrandId", "Name");
            return View(product);
        }

        [Route("XemChiTietSanPham")]
        public async Task<IActionResult> XemChiTietSanPham(int productId)
        {
            var sanPham = DbContext.Products.Find(productId);
            if (sanPham == null)
            {
                return NotFound();
            }
            return View(sanPham);
        }

        [Route("XoaSanPham")]
        public async Task<IActionResult> XoaSanPham(int productId)
        {
            var sanPham = DbContext.Products.Find(productId);
            if (sanPham == null)
            {
                return NotFound();
            }
            return View(sanPham);
        }

        [HttpPost, ActionName("XoaSanPham")]
        [Route("XoaSanPham")]
        public async Task<IActionResult> XacNhanXoaSanPham(int productId)
        {
            TempData["Message"] = "";
            var sanPham = DbContext.Products.Find(productId);
            var chiTietSanPham = DbContext.OrderDetails.Where(x => x.ProductId == productId).ToList();
            if (chiTietSanPham.Count() > 0)
            {
                TempData["Message"] = "Không được xóa sản phẩm này";
                return RedirectToAction("DanhMucSanPham", "HomeAdmin");
            }
            if (!string.IsNullOrEmpty(sanPham.Image))
            {
                var imagePath = Path.Combine("wwwroot/SLayoutWatch/img", sanPham.Image);
                if (System.IO.File.Exists(imagePath))
                {
                    System.IO.File.Delete(imagePath);
                }
            }
            DbContext.Products.Remove(sanPham);
            DbContext.SaveChanges();
            TempData["Message"] = "Sản phẩm được xóa thành công";
            return RedirectToAction("DanhMucSanPham", "HomeAdmin");
        }
        //QUAN LY SAN PHAM END

        //QUAN LY HANG BEGIN
        [Route("ThemHang")]
        public IActionResult ThemHang()
        {
            return View();
        }

        [Route("ThemHang")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ThemHang(Brand brand)
        {
            if (ModelState.IsValid)
            {
                DbContext.Brands.Add(brand);
                await DbContext.SaveChangesAsync();
                return RedirectToAction(nameof(DanhMucHang));
            }
            return View(brand);
        }

        [Route("SuaHang")]
        public IActionResult SuaHang(int brandId)
        {
            var hang = DbContext.Brands.Find(brandId);
            if (hang == null)
            {
                return NotFound();
            }
            return View(hang);
        }

        [Route("SuaHang")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SuaHang(Brand brand)
        {
            if (ModelState.IsValid)
            {
                DbContext.Brands.Update(brand);
                await DbContext.SaveChangesAsync();
                return RedirectToAction("DanhMucHang");
            }
            return View(brand);
        }

        [Route("XemChiTietHang")]
        public async Task<IActionResult> XemChiTietHang(int brandId)
        {
            var hang = DbContext.Brands.Find(brandId);
            if (hang == null)
            {
                return NotFound();
            }
            return View(hang);
        }

        [Route("XoaHang")]
        public async Task<IActionResult> XoaHang(int brandId)
        {
            var hang = DbContext.Brands.Find(brandId);
            if (hang == null)
            {
                return NotFound();
            }
            return View(hang);
        }

        [HttpPost, ActionName("XoaHang")]
        [Route("XoaHang")]
        public async Task<IActionResult> XacNhanXoaHang(int brandId)
        {
            var hang = DbContext.Brands.Find(brandId);
            DbContext.Brands.Remove(hang);
            DbContext.SaveChanges();
            return RedirectToAction("DanhMucHang", "HomeAdmin");
        }
        //QUAN LY HANG END

        //QUAN LY LOAI BEGIN
        [Route("ThemLoai")]
        public IActionResult ThemLoai()
        {
            return View();
        }

        [Route("ThemLoai")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ThemLoai(Category category)
        {
            if (ModelState.IsValid)
            {
                DbContext.Categories.Add(category);
                await DbContext.SaveChangesAsync();
                return RedirectToAction(nameof(DanhMucLoai));
            }
            return View(category);
        }

        [Route("SuaLoai")]
        public IActionResult SuaLoai(int categoryId)
        {
            var loai = DbContext.Categories.Find(categoryId);
            if (loai == null)
            {
                return NotFound();
            }
            return View(loai);
        }

        [Route("SuaLoai")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SuaLoai(Category category)
        {
            if (ModelState.IsValid)
            {
                DbContext.Categories.Update(category);
                await DbContext.SaveChangesAsync();
                return RedirectToAction("DanhMucLoai");
            }
            return View(category);
        }

        [Route("XemChiTietLoai")]
        public async Task<IActionResult> XemChiTietLoai(int categoryId)
        {
            var loai = DbContext.Categories.Find(categoryId);
            if (loai == null)
            {
                return NotFound();
            }
            return View(loai);
        }

        [Route("XoaLoai")]
        public async Task<IActionResult> XoaLoai(int categoryId)
        {
            var loai = DbContext.Categories.Find(categoryId);
            if (loai == null)
            {
                return NotFound();
            }
            return View(loai);
        }

        [HttpPost, ActionName("XoaLoai")]
        [Route("XoaLoai")]
        public async Task<IActionResult> XacNhanXoaLoai(int categoryId)
        {
            var loai = DbContext.Categories.Find(categoryId);
            DbContext.Categories.Remove(loai);
            DbContext.SaveChanges();
            return RedirectToAction("DanhMucLoai", "HomeAdmin");
        }
        //QUAN LY LOAI END

        //QUAN LY COUPON BEGIN
        [Route("ThemCoupon")]
        public IActionResult ThemCoupon()
        {
            return View();
        }

        [Route("ThemCoupon")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ThemCoupon(DiscountCode discountCode)
        {
            if (ModelState.IsValid)
            {
                DbContext.DiscountCodes.Add(discountCode);
                await DbContext.SaveChangesAsync();
                return RedirectToAction(nameof(DanhMucCoupon));
            }
            return View(discountCode);
        }

        [Route("SuaCoupon")]
        public IActionResult SuaCoupon(int discountId)
        {
            var coupon = DbContext.DiscountCodes.Find(discountId);
            if (coupon == null)
            {
                return NotFound();
            }
            return View(coupon);
        }

        [Route("SuaCoupon")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SuaCoupon(DiscountCode discountCode)
        {
            if (ModelState.IsValid)
            {
                DbContext.DiscountCodes.Update(discountCode);
                await DbContext.SaveChangesAsync();
                return RedirectToAction("DanhMucCoupon");
            }
            return View(discountCode);
        }

        [Route("XemChiTietCoupon")]
        public async Task<IActionResult> XemChiTietCoupon(int discountId)
        {
            var coupon = DbContext.DiscountCodes.Find(discountId);
            if (coupon == null)
            {
                return NotFound();
            }
            return View(coupon);
        }

        [Route("XoaCoupon")]
        public async Task<IActionResult> XoaCoupon(int discountId)
        {
            var coupon = DbContext.DiscountCodes.Find(discountId);
            if (coupon == null)
            {
                return NotFound();
            }
            return View(coupon);
        }

        [HttpPost, ActionName("XoaCoupon")]
        [Route("XoaCoupon")]
        public async Task<IActionResult> XacNhanXoaCoupon(int discountId)
        {
            var coupon = DbContext.DiscountCodes.Find(discountId);
            DbContext.DiscountCodes.Remove(coupon);
            DbContext.SaveChanges();
            return RedirectToAction("DanhMucCoupon", "HomeAdmin");
        }
        //QUAN LY COUPON END

        //QUAN LY DONHANG BEGIN
        [Route("SuaDonHang")]
        public IActionResult SuaDonHang(int orderId)
        {
            var donHang = DbContext.Orders.Find(orderId);
            if (donHang == null)
            {
                return NotFound();
            }
            return View(donHang);
        }

        [Route("SuaDonHang")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SuaDonHang(Order order)
        {
            if (ModelState.IsValid)
            {
                DbContext.Orders.Update(order);
                await DbContext.SaveChangesAsync();
                return RedirectToAction("DanhMucDonHang");
            }
            return View(order);
        }

        [Route("XemChiTietDonHang")]
        public async Task<IActionResult> XemChiTietDonHang(int orderId)
        {
            var donHang = DbContext.Orders.Find(orderId);
            if (donHang == null)
            {
                return NotFound();
            }
            return View(donHang);
        }

        [HttpPost]
        public IActionResult UpdateOrderStatus(int orderId, int newStatus)
        {
            var order = DbContext.Orders.Find(orderId);
            if (order != null)
            {
                order.Status = newStatus;
                if (newStatus == 1)
                {
                    order.PaymentStatus = "Đã thanh toán";
                    if (order.PaymentTime == null)
                    {
                        order.PaymentTime = DateTime.Now;
                    }
                    if (order.ReceiveTime == null)
                    {
                        order.ReceiveTime = DateTime.Now;
                    }
                }
                else
                {                 
                    order.ReceiveTime = null;
                }     
                DbContext.SaveChanges();
                return RedirectToAction("DanhMucSanPham");
            }
            return Json(new { success = false });
        }

        [Route("XoaDonHang")]
        public async Task<IActionResult> XoaDonHang(int orderId)
        {
            var donHang = DbContext.Orders.Find(orderId);
            if (donHang == null)
            {
                return NotFound();
            }
            return View(donHang);
        }

        [HttpPost, ActionName("XoaDonHang")]
        [Route("XoaDonHang")]
        public async Task<IActionResult> XacNhanXoaDonHang(int orderId)
        {
            TempData["Message"] = "";
            var donHang = DbContext.Orders.Find(orderId);
            var chiTietSanPham = DbContext.OrderDetails.Where(x => x.OrderId == orderId).ToList();
            if (chiTietSanPham.Count() > 0)
            {
                TempData["Message"] = "Không được xóa đơn hàng này";
                return RedirectToAction("DanhMucDonHang", "HomeAdmin");
            }
            DbContext.Orders.Remove(donHang);
            DbContext.SaveChanges();
            TempData["Message"] = "Đơn hàng được xóa thành công";
            return RedirectToAction("DanhMucDonHang", "HomeAdmin");
        }
        //QUAN LY DONHANG END

        //QUAN LY CHITIETDONHANG BEGIN
        [Route("XemChiTietCTDonHang")]
        public async Task<IActionResult> XemChiTietCTDonHang(int orderdetailId)
        {
            var ctdonHang = DbContext.OrderDetails.Find(orderdetailId);
            if (ctdonHang == null)
            {
                return NotFound();
            }
            return View(ctdonHang);
        }

        [Route("XoaCTDonHang")]
        public async Task<IActionResult> XoaCTDonHang(int orderdetailId)
        {
            var ctdonHang = DbContext.OrderDetails.Find(orderdetailId);
            if (ctdonHang == null)
            {
                return NotFound();
            }
            return View(ctdonHang);
        }

        [HttpPost, ActionName("XoaCTDonHang")]
        [Route("XoaCTDonHang")]
        public async Task<IActionResult> XacNhanXoaCTDonHang(int orderdetailId)
        {
            var ctdonHang = DbContext.OrderDetails.Find(orderdetailId);
            DbContext.OrderDetails.Remove(ctdonHang);
            DbContext.SaveChanges();
            return RedirectToAction("DanhMucCTDH", "HomeAdmin");
        }
        //QUAN LY CHITIETDONHANG END

        //QUAN LY ANHSANPHAM BEGIN
        [Route("ThemAnhSanPham")]
        public IActionResult ThemAnhSanPham()
        {
            return View();
        }

        private async Task<string> SaveImagesSanPham(IFormFile image)
        {
            var savePath = Path.Combine("wwwroot/SLayoutWatch/img/anhsanpham", image.FileName);
            using (var fileStream = new FileStream(savePath, FileMode.Create))
            {
                await image.CopyToAsync(fileStream);
            }
            return "" + image.FileName;
        }

        [Route("ThemAnhSanPham")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ThemAnhSanPham(ProductImage productImage, IFormFile image)
        {
            if (ModelState.IsValid)
            {
                if (image != null && image.Length > 0)
                {
                    productImage.Url = await SaveImagesSanPham(image);
                }
                DbContext.ProductImages.Add(productImage);
                await DbContext.SaveChangesAsync();
                return RedirectToAction("DanhMucAnhSanPham");
            }
            return View(productImage);
        }

        [Route("SuaAnhSanPham")]
        public IActionResult SuaAnhSanPham(int pictureId)
        {
            var anhsanPham = DbContext.ProductImages.Find(pictureId);
            return View(anhsanPham);
        }

        [Route("SuaAnhSanPham")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SuaAnhSanPham(ProductImage productImage, IFormFile image)
        {
            if (ModelState.IsValid)
            {
                if (image != null && image.Length > 0)
                {
                    productImage.Url = await SaveImagesSanPham(image);
                }
                DbContext.ProductImages.Update(productImage);
                await DbContext.SaveChangesAsync();
                return RedirectToAction("DanhMucAnhSanPham");
            }
            return View(productImage);
        }   

        [Route("XemChiTietAnhSanPham")]
        public async Task<IActionResult> XemChiTietAnhSanPham(int pictureId)
        {
            var anhsanPham = DbContext.ProductImages.Find(pictureId);
            if (anhsanPham == null)
            {
                return NotFound();
            }
            return View(anhsanPham);
        }

        [Route("XoaAnhSanPham")]
        public async Task<IActionResult> XoaAnhSanPham(int pictureId)
        {
            var anhsanPham = DbContext.ProductImages.Find(pictureId);
            if (anhsanPham == null)
            {
                return NotFound();
            }
            return View(anhsanPham);
        }

        [HttpPost, ActionName("XoaAnhSanPham")]
        [Route("XoaAnhSanPham")]
        public async Task<IActionResult> XacNhanAnhXoaSanPham(int pictureId)
        {
            TempData["Message"] = "";
            var anhsanPham = DbContext.ProductImages.Find(pictureId);
            if (!string.IsNullOrEmpty(anhsanPham.Url))
            {
                var imagePath = Path.Combine("wwwroot/SLayoutWatch/img/anhsanpham", anhsanPham.Url);
                if (System.IO.File.Exists(imagePath))
                {
                    System.IO.File.Delete(imagePath);
                }
            }
            DbContext.ProductImages.Remove(anhsanPham);
            DbContext.SaveChanges();
            TempData["Message"] = "Sản phẩm được xóa thành công";
            return RedirectToAction("DanhMucAnhSanPham", "HomeAdmin");
        }
        //QUAN LY ANHSANPHAM END
    }
}
