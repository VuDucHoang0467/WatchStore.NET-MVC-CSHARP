namespace WebDongHo.Models
{
    public class ResetPasswordViewModel
    {
        public int UserId { get; set; }
        public string Token { get; set; }
        public string NewPassword { get; set; }
    }
}
