namespace WebDongHo.Models
{
    public class ShoppingCart
    {
        public List<CartItem> Items { get; set; } = new List<CartItem>();
        public List<Product> Products { get; set; }
        public decimal TotalPrice { get; set; }
        public string CouponCode { get; set; }
        public decimal DiscountPercentage { get; set; }

        public ShoppingCart()
        {
            Products = new List<Product>();
        }

        public void AddItem(CartItem item)
        {
            var existingItem = Items.FirstOrDefault(i => i.ProductId ==
            item.ProductId);
            if (existingItem != null)
            {
                existingItem.Quantity += item.Quantity;
            }
            else
            {
                Items.Add(item);
            }
        }

        public void UpdateQuantity(int productId, int quantity)
        {
            var existingItem = Items.FirstOrDefault(item => item.ProductId == productId);
            if (existingItem != null)
            {
                existingItem.Quantity = quantity;
            }
        }

        public void ApplyDiscount(decimal discountPercentage)
        {
            if (discountPercentage <= 0 || discountPercentage >= 100 || CalculateTotal() <= 0)
            {
                return;
            }
            DiscountPercentage = discountPercentage;
        }

        public decimal CalculateTotal()
        {
            decimal total = 0;
            foreach (var item in Items)
            {
                total += item.Price * item.Quantity;
            }
            return total;
        }

        public double CalculateMomoTotal()
        {
            double total = 0;
            foreach (var item in Items)
            {
                total += (double)item.Price * (double)item.Quantity;
            }
            return total;
        }

        public decimal CalculateDiscountAmount()
        {
            decimal total = CalculateTotal();
            return total * (DiscountPercentage / 100m);
        }

        public decimal CalculateTotalAfterDiscount()
        {
            return CalculateTotal() - CalculateDiscountAmount();
        }

        public double CalculateMomoDiscountAmount()
        {
            double total = CalculateMomoTotal();
            return total * (double)(DiscountPercentage / 100m);
        }

        public double CalculateMomoTotalAfterDiscount()
        {
            return CalculateMomoTotal() - CalculateMomoDiscountAmount();
        }

        public void RemoveItem(int productId)
        {
            Items.RemoveAll(i => i.ProductId == productId);
        }
    }
}
