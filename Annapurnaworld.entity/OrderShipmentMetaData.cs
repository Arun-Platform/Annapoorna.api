using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Annapurnaworld.entity
{
    /// <summary>
    /// OrderShipmentMetaData Entity
    /// </summary>
    public class OrderShipmentMetaData
    {
        public int KeyId { get; set; }
        public int RecordCount { get; set; }
        public string FileName { get; set; }
        public string? SheetType { get; set; }
        public string? Status { get; set; }
        public DateOnly FileDate { get; set; }

    }
}
