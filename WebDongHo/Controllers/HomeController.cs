using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Diagnostics;
using System.Drawing.Printing;
using System.Security.Claims;
using WebDongHo.Models;
using WebDongHo.Models.Authentication;
using X.PagedList;

namespace WebDongHo.Controllers
{
    public class HomeController : Controller
    {
        QlwebDongHoContext DbContext = new QlwebDongHoContext();
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index(int? page)
        {
            int pageSize = 8;
            int pageNumber = page == null || page<0 ? 1 : page.Value;
            var listSanpham = DbContext.Products.AsNoTracking().OrderBy(x => x.Name);
            PagedList<Product> listp = new PagedList<Product>(listSanpham, pageNumber, pageSize);
            return View(listp);
        }

        [HttpPost]
        public IActionResult Index(string keyword, int? page)
        {
            int pageSize = 8;
            int pageNumber = page ?? 1;
            string[] keywords = keyword.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            var danhSachSanPham = DbContext.Products
                .AsNoTracking()
                .Where(x => keywords.All(k => x.Name.Contains(k)))
                .OrderBy(x => x.Name);
            var pagedList = new PagedList<Product>(danhSachSanPham, pageNumber, pageSize);
            ViewBag.Keyword = keyword;
            return View(pagedList);
        }

        public IActionResult FilterByPrice()
        {
            return View();
        }

        [HttpPost]
        public IActionResult FilterByPrice(decimal minPrice, decimal maxPrice, int? page)
        {
            int pageSize = 8;
            int pageNumber = page ?? 1;
            List<Product> filteredProducts = FilterProductsByPrice(minPrice, maxPrice);
            IPagedList<Product> pagedList = new PagedList<Product>(filteredProducts, pageNumber, pageSize);
            return View("Index", pagedList);
        }

        private List<Product> GetProductsFromDatabase()
        {
            return DbContext.Products.ToList();
        }

        private List<Product> FilterProductsByPrice(decimal minPrice, decimal maxPrice)
        {
            return DbContext.Products.Where(p => p.Price >= minPrice && p.Price <= maxPrice).ToList();
        }

        [HttpPost]
        public IActionResult FilterByBrand(int brandId, int? page)
        {
            int pageSize = 8;
            int pageNumber = page ?? 1;
            var products = DbContext.Products.Where(p => p.BrandId == brandId).ToList();
            IPagedList<Product> pagedList = products.ToPagedList(pageNumber, pageSize);
            return View("Index", pagedList);
        }

        public IActionResult SortByPriceAscending(int? page)
        {
            int pageSize = 8;
            int pageNumber = page ?? 1;
            var products = DbContext.Products.OrderBy(p => p.Price).ToList();
            var pagedList = products.ToPagedList(pageNumber, pageSize);
            return View("Index", pagedList);
        }

        public IActionResult SortByPriceDescending(int? page)
        {
            int pageSize = 8;
            int pageNumber = page ?? 1;
            var products = DbContext.Products.OrderByDescending(p => p.Price).ToList();
            var pagedList = products.ToPagedList(pageNumber, pageSize);
            return View("Index", pagedList);
        }

        public IActionResult SanPhamTheoCategory(int categoryId, int? page)
        {
            int pageSize = 8;
            int pageNumber = page == null || page < 0 ? 1 : page.Value;
            var listSanpham = DbContext.Products.AsNoTracking().Where(x => x.CategoryId == categoryId).OrderBy(x => x.Name);
            PagedList<Product> listp = new PagedList<Product>(listSanpham, pageNumber, pageSize);
            ViewBag.categoryId = categoryId;
            return View(listp);
        }

        public IActionResult ChiTietSanPham(int productId)
        {
            var sanPham = DbContext.Products.SingleOrDefault(x => x.ProductId == productId);
            var anhSanPham = DbContext.ProductImages.Where(x => x.ProductId == productId);
            ViewBag.anhSanPham = anhSanPham;
            var reviews = DbContext.ProductReviews.Where(r => r.ProductId == productId).ToList();
            ViewBag.Reviews = reviews;
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId != null)
            {
                var userReview = DbContext.ProductReviews.FirstOrDefault(r => r.ProductId == productId && r.UserId == userId);
                ViewBag.UserReview = userReview;
            }
            return View(sanPham);
        }

        public IActionResult SanPhamTheoBrand(int brandId, int? page)
        {
            int pageSize = 8;
            int pageNumber = page == null || page < 0 ? 1 : page.Value;
            var watchTypes = DbContext.Products.AsNoTracking().Where(x => x.BrandId == brandId).OrderBy(x => x.Name);
            PagedList<Product> listp = new PagedList<Product>(watchTypes, pageNumber, pageSize);
            return View(listp);
        }

        [HttpPost]
        public IActionResult AddReview(int productId, int rating, string reviewText)
        {
            var userId = HttpContext.Session.GetInt32("UserId").Value;
            var userEmail = HttpContext.Session.GetString("Email");
            if (!IsUserPurchasedProduct(userId, productId))
            {
                ViewBag.ErrorMessage = "Bạn phải nhận hàng trước khi để lại đánh giá.";
                var sanPham = DbContext.Products.SingleOrDefault(x => x.ProductId == productId);
                var anhSanPham = DbContext.ProductImages.Where(x => x.ProductId == productId);
                ViewBag.anhSanPham = anhSanPham;
                var reviews = DbContext.ProductReviews.Where(r => r.ProductId == productId).ToList();
                ViewBag.Reviews = reviews;
                return View("ChiTietSanPham", sanPham);
            }
            var existingReview = DbContext.ProductReviews.FirstOrDefault(r => r.ProductId == productId && r.UserId == userId);
            if (existingReview != null)
            {
                existingReview.Rating = rating;
                existingReview.ReviewText = reviewText;
                existingReview.ReviewDate = DateTime.Now;
            }
            else
            {
                var review = new ProductReview
                {
                    ProductId = productId,
                    UserId = userId,
                    Email = userEmail,
                    Rating = rating,
                    ReviewText = reviewText,
                    ReviewDate = DateTime.Now
                };
                DbContext.ProductReviews.Add(review);
            }

            DbContext.SaveChanges();
            return RedirectToAction("ChiTietSanPham", new { productId = productId });
        }

        [HttpPost]
        public IActionResult AddAdminResponse(int reviewId, string adminResponse)
        {
            var review = DbContext.ProductReviews.FirstOrDefault(r => r.Id == reviewId);
            if (review != null)
            {
                review.AdminResponse = adminResponse;
                review.AdminResponseDate = DateTime.Now;
                DbContext.SaveChanges();
            }

            return RedirectToAction("ChiTietSanPham", new { productId = review.ProductId });
        }

        public bool IsUserPurchasedProduct(int userId, int productId)
        {
            return DbContext.Orders.Any(o => o.UserId == userId && o.Status != 0 && o.OrderDetails.Any(od => od.ProductId == productId));
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
