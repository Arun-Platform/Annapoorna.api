using System.ComponentModel.DataAnnotations;
using System.Numerics;

namespace Annapurnaworld.entity
{
    public class PackageShipmentDetails
    {
        public long Id { get; set; }
        public string RunsheetId { get; set; }
        public DateTime ShipmentUpdateDateTime { get; set; }
        public string AgentName { get; set; }
        public string? FHRID { get; set; }
        public string? Vendor { get; set; }
        public string DeliveryHub { get; set; }
        public string State { get; set; }
        public string City { get; set; }
        public string Zone { get; set; }
        public string? RunSheetStatus { get; set; }
        public string ShipmentId { get; set; }
        public string ShipmentType { get; set; }
        public decimal ShipmentPrice { get; set; }
        public decimal ShipmentWeight { get; set; }
        public string? ShipmentStatus { get; set; }
        public string? SheetType { get; set; }
        public DateTime SheetCreateDateTime { get; set; }
        public string? CustomerFeedBackResponse { get; set; }
        public string? IsSellerReturn { get; set; }
        public string? RunsheetShipmentStatus { get; set; }
        public string? GTNL { get; set; }
    }
}
