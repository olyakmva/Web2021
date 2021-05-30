using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace BookShop2021.Models
{
    public class OrderNumberViewModel
    {
        [Required(ErrorMessage = "Пожалуйста введите начальную дату")]
        [Display(Name = "Начало диапазона")]
        public DateTime Start { get; set; }
        [Required(ErrorMessage = "Пожалуйста введите конечную дату")]
        [Display(Name = "Конец диапазона")]
        public DateTime End { get; set; }
        [Display(Name = "Количество сделанных заказов")]
        public int Number { get; set; }
        [Display(Name = "Общая сумма заказов")]
        public int TotalSum { get; set; }
    }
}
