using WebDongHo.Models;

namespace WebDongHo.Repository
{
    public class EFCategoryRepository : ICategoryRepository
    {
        public readonly QlwebDongHoContext _context;
        public EFCategoryRepository(QlwebDongHoContext context)
        {
            _context = context;
        }

        public Category Add(Category category)
        {
            _context.Categories.Add(category);
            _context.SaveChanges();
            return category;
        }

        public IEnumerable<Category> GetAllCategories() 
        {
            return _context.Categories;
        }

        public Category GetCategory(int categoryId)
        {
            return _context.Categories.Find(categoryId);
        }

        public Category Update(Category category)
        {
            _context.Categories.Update(category);
            _context.SaveChanges ();
            return category;
        }

        public Category Delete(int id) 
        { 
            throw new NotImplementedException();
        }
    }
}
