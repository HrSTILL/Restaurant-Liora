using Restaurant_Manager.Models;
using System.Collections.Generic;

namespace Restaurant_Manager.ViewModels
{
    public class CustomerOrdersViewModel
    {
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public List<Order> Pending { get; set; } = new();
        public List<Order> Preparing { get; set; } = new();
        public List<Order> Served { get; set; } = new();
        public List<Order> Completed { get; set; } = new();
        public List<Order> Cancelled { get; set; } = new();
    }
}
