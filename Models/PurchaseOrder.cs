using System.ComponentModel.DataAnnotations;

namespace FougeraClub1.Models
{
    public class PurchaseOrder
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int OrderNumber { get; set; }

        [Required]
        public DateTime OrderDate { get; set; }

        public bool WithVAT { get; set; }

        public string Status { get; set; } = "Pending";

        [Required]
        public int SupplierId { get; set; }
        public Supplier? Supplier { get; set; }

        public string CreatedByUserId { get; set; }
        public ApplicationUser? CreatedByUser { get; set; }

        public string? ApprovedByUserId { get; set; }
        public ApplicationUser? ApprovedByUser { get; set; }

        public List<PurchaseOrderItem> Items { get; set; } = new();

        public DateTime? ApprovedDate { get; set; }
    }
}