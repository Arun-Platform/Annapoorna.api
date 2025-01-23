using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Annapurnaworld.entity
{
    public class BaseEntity
    {
        public Guid? CreateById { get; set; }
        public Guid? UpdatedById { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? UpdatedDate{ get; set; }

    }
}
