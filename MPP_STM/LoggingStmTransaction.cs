using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace MPP_STM
{
    class LoggingStmTransaction<T> : IStmTransaction<T> where T: struct
    {                
        private StmTransaction<T> stmTransaction;
        private Logger logger;
        public bool IsCommited
        {
            get
            {
                return stmTransaction.IsCommited;
            }
        }

        public LoggingStmTransaction(StmTransaction<T> stmTransaction)
        {
            this.stmTransaction = stmTransaction;
            logger = new Logger();
        }

        public void Commit()
        {
            lock(Stm.commitLock)
            {                
                stmTransaction.Commit();
                logger.Log(MethodBase.GetCurrentMethod(), stmTransaction.revision);
            }            
        }

        public T Read(StmRef<T> source)
        {
            try
            {
                return stmTransaction.Read(source);                
            }
            finally
            {
                logger.ReadLog<T>(MethodBase.GetCurrentMethod(), stmTransaction.revision, source);
            }                        
        }

        public void Rollback()
        {                        
            stmTransaction.Rollback();
            logger.Log(MethodBase.GetCurrentMethod(), stmTransaction.revision);
        }

        public void Write(StmRef<T> target, T newValue)
        {            
            stmTransaction.Write(target, newValue);
            logger.WriteLog(MethodBase.GetCurrentMethod(), stmTransaction.revision, target, newValue);
        }
    }
}
