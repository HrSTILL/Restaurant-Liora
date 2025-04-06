namespace Restaurant_Manager.ViewModels
{
    public class CartItem
    {
        public int MenuItemId { get; set; }
        public string Name { get; set; }
        public string ImageUrl { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public string Tags { get; set; }

        public decimal Subtotal => Price * Quantity;

        public MenuItem MenuItem { get; set; }
    }
}
