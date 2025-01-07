﻿namespace WebDongHo.Models
{
    public class EmailChangeRequest
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string NewEmail { get; set; }
        public string Token { get; set; }
        public DateTime RequestDate { get; set; }
    }
}
