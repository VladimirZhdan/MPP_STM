using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MPP_STM.Tests
{
    public class Transaction
    {        
        public bool IsOnlyReadingTransaction { get; set; }
        public bool NeedRollback { get; set; }
        public bool IsCommited { get; set; }

        public Transaction()
        {
            IsOnlyReadingTransaction = true;
            NeedRollback = false;
            IsCommited = false;            
        }
    }
}
