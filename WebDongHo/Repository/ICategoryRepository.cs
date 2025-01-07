using WebDongHo.Models;
namespace WebDongHo.Repository
{
    public interface ICategoryRepository
    {
        Category Add (Category category);
        Category Update (Category category);
        Category Delete (int id);
        Category GetCategory (int categoryId);
        IEnumerable<Category> GetAllCategories ();
    }
}
