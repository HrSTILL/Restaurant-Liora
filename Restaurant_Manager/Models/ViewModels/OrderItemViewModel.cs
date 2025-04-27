namespace Restaurant_Manager.ViewModels
{
    public class OrdersOnDayViewModel
    {
        public int OrderId { get; set; }
        public string FullName { get; set; }
        public List<OrderItemViewModel> Items { get; set; }
    }

    public class OrderItemViewModel
    {
        public string Name { get; set; }
        public decimal Price { get; set; }
    }

}
