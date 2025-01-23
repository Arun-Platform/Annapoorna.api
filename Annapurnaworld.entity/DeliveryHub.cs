using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Annapurnaworld.entity
{
    public class DeliveryHub
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class PackageShipmentHubResponse
    {
        public IEnumerable<string> DeliveryHubs { get; set; }
        public List<Dictionary<string, object>> KeyValuePairs { get; set; }
    }
}
