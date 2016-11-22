using System;
using System.Reflection;

namespace MPP_STM
{
    class LoggingStmTransaction<T> : IStmTransaction<T> where T: struct
    {                
        private StmTransaction<T> stmTransaction;
        private static Logger logger = new Logger();
        public bool IsCommited
        {
            get
            {
                return stmTransaction.IsCommited;
            }
        }

        public long Revision
        {
            get
            {
                return stmTransaction.Revision;
            }
        }

        public LoggingStmTransaction(StmTransaction<T> stmTransaction)
        {
            this.stmTransaction = stmTransaction;            
        }

        public void Commit()
        {
            StmTransaction<T>.LockStatic(stmTransaction);
            try
            {
                stmTransaction.Commit();
                logger.Log(MethodBase.GetCurrentMethod(), stmTransaction.Revision);
            }
            finally
            {
                StmTransaction<T>.UnLockStatic(stmTransaction);
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
                logger.ReadLog<T>(MethodBase.GetCurrentMethod(), stmTransaction.Revision, source);
            }            
        }

        public void Rollback()
        {            
            stmTransaction.Rollback();
            logger.Log(MethodBase.GetCurrentMethod(), stmTransaction.Revision);                            
        }

        public void Write(StmRef<T> target, T newValue)
        {                   
            stmTransaction.Write(target, newValue);
            logger.WriteLog(MethodBase.GetCurrentMethod(), stmTransaction.Revision, target, newValue);                          
        }

        public void SetParentTransaction(IStmTransaction<T> parentTransaction)
        {
            stmTransaction.SetParentTransaction(parentTransaction);
        }

        public void AddSubTransaction(IStmTransaction<T> subTransaction)
        {
            stmTransaction.AddSubTransaction(subTransaction);
        }

        public bool CheckParentTransaction()
        {
            throw new NotImplementedException();
        }
    }
}
