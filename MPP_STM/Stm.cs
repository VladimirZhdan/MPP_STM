using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MPP_STM
{
    class Stm
    {
        public static object commitLock = new object();

        public static void Do<T>(TransactionBlock<T> block) where T: struct
        {
            LoggingStmTransaction<T> tx = new LoggingStmTransaction<T>(new StmTransaction<T>());
            block.SetTx(tx);
            bool commited = false;
            while(!commited)
            {                                
                block.Run();
                tx.Commit();
                commited = tx.IsCommited;
                if (!commited)
                {                    
                    tx.Rollback();                    
                }                
            }            
        }
    }
}
