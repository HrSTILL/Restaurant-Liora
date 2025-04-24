using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Restaurant_Manager.ViewModels
{
    public class MenuItemFormViewModel
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string Description { get; set; }

        [Required]
        public decimal Price { get; set; }

        public IFormFile? ImageFile { get; set; }

        public string? ExistingImageUrl { get; set; }


        [Required]
        public string Category { get; set; }

        public List<string> Allergens { get; set; }

        [Required]
        public int Calories { get; set; }

        public bool IsGlutenFree { get; set; }

        [Required]
        public int PrepTime { get; set; }

        public string Tags { get; set; }

        public List<SelectListItem> CategoryOptions { get; set; } = new()
        {
            new SelectListItem { Value = "salad", Text = "Salads and Appetizers" },
            new SelectListItem { Value = "main", Text = "Main Courses" },
            new SelectListItem { Value = "dessert", Text = "Desserts" },
            new SelectListItem { Value = "drink", Text = "Drinks" },
        };

        public List<SelectListItem> AllergenOptions { get; set; } = new()
{
    new SelectListItem { Value = "Gluten", Text = "Gluten" },
    new SelectListItem { Value = "Nuts", Text = "Nuts" },
    new SelectListItem { Value = "Dairy", Text = "Dairy" },
    new SelectListItem { Value = "Egg", Text = "Egg" },
    new SelectListItem { Value = "Soy", Text = "Soy" },
    new SelectListItem { Value = "Fish", Text = "Fish" },
    new SelectListItem { Value = "Peanuts", Text = "Peanuts" },
    new SelectListItem { Value = "Sesame", Text = "Sesame" },
};

    }
}
