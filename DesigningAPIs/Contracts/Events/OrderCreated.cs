using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contracts.Events
{
    public class OrderCreated
    {
        public int OrderId { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
