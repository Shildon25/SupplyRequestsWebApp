using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SupplyManagement.Models
{
    public class ItemSupplyRequest
    {
        [Key]
        public int Id { get; set; }
        public int SupplyRequestId { get; set; }
        public int ItemId { get; set; }
        [ForeignKey(nameof(ItemId))]
        [InverseProperty(nameof(Item.RequestItems))]
        public Item Item { get; set; } = null!;
        [ForeignKey(nameof(SupplyRequestId))]
        [InverseProperty(nameof(SupplyRequest.RequestItems))]
        public SupplyRequest SupplyRequest { get; set; } = null!;
    }
}
