using System.ComponentModel.DataAnnotations;

namespace FougeraClub1.Models
{
    public class CreatePurchaseOrderViewModel
    {
        [Required]
        public int SupplierId { get; set; }

        [Required]
        public DateTime OrderDate { get; set; } = DateTime.Now;

        public bool WithVAT { get; set; }

        public List<CreateItemViewModel> Items { get; set; } = new();
    }

    public class CreateItemViewModel
    {
        public string Description { get; set; } = "";

        public int Quantity { get; set; } = 1;

        public decimal UnitPrice { get; set; }
    }
}