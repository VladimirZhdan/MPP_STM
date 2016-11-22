using System.Reflection;

namespace MPP_STM
{
    public class LoggingStmTransactionModified<T> : IStmTransaction<T> where T: struct
    {
        private StmTransactionModified<T> stmTransaction;
        private static LoggerModified logger = new LoggerModified();
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

        public LoggingStmTransactionModified(StmTransactionModified<T> stmTransaction)
        {
            this.stmTransaction = stmTransaction;
        }

        public void SetParentTransaction(IStmTransaction<T> parentTransaction)
        {
            stmTransaction.SetParentTransaction(parentTransaction);
        }

        public void AddSubTransaction(IStmTransaction<T> subTransaction)
        {
            stmTransaction.AddSubTransaction(subTransaction);
        }

        public void Commit()
        {
            //lock(StmModified.commitLock)
            //{                
                StmTransactionModified<T>.LockStatic(stmTransaction);
                try
                {
                    stmTransaction.Commit();
                    if(stmTransaction.IsParentConflict)
                    {
                        logger.Log(MethodBase.GetCurrentMethod(), stmTransaction.Revision, stmTransaction.GetParentTransactionRevision(), "_ParentConflict");
                    }
                    else
                    {
                        logger.Log(MethodBase.GetCurrentMethod(), stmTransaction.Revision, stmTransaction.GetParentTransactionRevision());
                    }                    
                }
                finally
                {
                    StmTransactionModified<T>.UnLockStatic(stmTransaction);
                }
            //}            
        }        

        public T Read(StmRef<T> source)
        {
            try
            {
                return stmTransaction.Read(source);
            }
            finally
            {
                logger.ReadLog<T>(MethodBase.GetCurrentMethod(), stmTransaction.Revision, stmTransaction.GetParentTransactionRevision(), source);
            }
        }

        public void Rollback()
        {
            stmTransaction.Rollback();
            logger.Log(MethodBase.GetCurrentMethod(), stmTransaction.Revision, stmTransaction.GetParentTransactionRevision());
        }

        public void Write(StmRef<T> target, T newValue)
        {                        
            stmTransaction.Write(target, newValue);
            logger.WriteLog(MethodBase.GetCurrentMethod(), stmTransaction.Revision, stmTransaction.GetParentTransactionRevision(), target, newValue);
        }

        public bool CheckParentTransaction()
        {
            return stmTransaction.CheckParentTransaction();
        }
    }
}
