using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NatLib.EventsArgs
{
    public class ExceptionEventsArgs: EventArgs
    {
        public Exception Exception { get; set; }
    }
}
