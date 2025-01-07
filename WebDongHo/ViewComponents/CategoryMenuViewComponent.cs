using WebDongHo.Models;
using Microsoft.AspNetCore.Mvc;
using WebDongHo.Repository;
namespace WebDongHo.ViewComponents
{
    public class CategoryMenuViewComponent : ViewComponent
    {
        private readonly ICategoryRepository _categoryMenuViewComponent;

        public CategoryMenuViewComponent(ICategoryRepository categoryRepository)
        {
            _categoryMenuViewComponent = categoryRepository;
        }

        public IViewComponentResult Invoke()
        {
            var category = _categoryMenuViewComponent.GetAllCategories().OrderBy(X => X.Name);
            return View(category);
        }
    }
}
