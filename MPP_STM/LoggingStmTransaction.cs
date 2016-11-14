using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MPP_STM
{
    class LoggingStmTransaction<T> : IStmTransaction<T> where T: struct
    {
        private StmTransaction<T> stmTransaction;

        public LoggingStmTransaction(StmTransaction<T> stmTransaction)
        {
            this.stmTransaction = stmTransaction;
        }

        public void Commit()
        {
            stmTransaction.Commit();
            //Log            
        }

        public object Read(IStmRef<T> source)
        {
            //Log
            return stmTransaction.Read(source);                        
        }

        public void Rollback()
        {
            stmTransaction.Rollback();
            //Log            
        }

        public void Write(IStmRef<T> target, object newValue)
        {
            stmTransaction.Write(target, newValue);
            //Log            
        }
    }
}
