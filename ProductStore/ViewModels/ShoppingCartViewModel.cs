using MvcAffableBean.Models;
using System.Collections.Generic;

namespace MvcAffableBean.ViewModels
{
    public class ShoppingCartViewModel
    {
        public List<Cart> CartItems { get; set; }
        public decimal CartTotal { get; set; }
    }
}