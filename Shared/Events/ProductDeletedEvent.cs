using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Events
{
    public class ProductDeletedEvent
    {
        public Guid Id { get; set; }
    }
}
