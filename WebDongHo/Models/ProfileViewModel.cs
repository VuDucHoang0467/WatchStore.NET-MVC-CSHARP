namespace WebDongHo.Models
{
    public class ProfileViewModel
    {
        public string UserName { get; set; }
        public string Email { get; set; }
        public ProfileViewModel Profile { get; set; }
        public List<Order> Orders { get; set; }
    }
}
