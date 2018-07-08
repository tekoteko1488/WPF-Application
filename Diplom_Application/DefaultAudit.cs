using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Diplom_Application
{
    class DefaultAudit
    {
        public int is_state_enabled { get; set; }
        public int on_failure { get; set; }
        public int type { get; set; }
        public int queueDelay { get; set; }

    }
}
