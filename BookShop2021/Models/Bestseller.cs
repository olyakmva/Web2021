using System.ComponentModel.DataAnnotations;

namespace BookShop2021.Models
{
    public class Bestseller
    {
        public Book TheBook { get; set; }
        [Display(Name = "Количество")]
        public int Quantity { get; set; }
    }
}