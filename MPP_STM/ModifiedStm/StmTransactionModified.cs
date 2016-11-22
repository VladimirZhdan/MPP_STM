using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace MPP_STM
{
    public class StmTransactionModified<T> : IStmTransaction<T> where T : struct
    {
        private IStmTransaction<T> parentTransaction;
        private List<IStmTransaction<T>> subTransactionList = new List<IStmTransaction<T>>();

        private Dictionary<StmRef<T>, object> inTxDict = new Dictionary<StmRef<T>, object>();
        private Dictionary<StmRef<T>, long> version = new Dictionary<StmRef<T>, long>();
        private Dictionary<StmRef<T>, long> parentVersion = new Dictionary<StmRef<T>, long>();
        private List<StmRef<T>> toUpdate = new List<StmRef<T>>();
        public long Revision { get; private set; }
        private static long transactionNum = 1;
        public bool IsCommited { get; private set; }
        private bool isLockedCommit = false;
        public bool IsParentConflict { get; private set; }            


        public StmTransactionModified()
        {
            Revision = TransactionNum;
            IsCommited = false;
        }

        public void SetParentTransaction(IStmTransaction<T> parentTransaction)
        {
            this.parentTransaction = parentTransaction;
            parentTransaction.AddSubTransaction(this);        
        }

        public void AddSubTransaction(IStmTransaction<T> subTransaction)
        {
            subTransactionList.Add(subTransaction);                
        }

        /// <summary>
        /// Возвращет значения свойства Revision родительской транзакции, при её наличие. В обратном случае возвращает -1
        /// </summary>
        /// <returns></returns>
        public long GetParentTransactionRevision()
        {
            if(parentTransaction != null)
            {
                return parentTransaction.Revision;
            }                
            else
            {
                return -1;
            }

        }

        private long TransactionNum
        {
            get
            {
                long result = transactionNum;
                transactionNum++;
                return result;
            }
        }

        public T Read(StmRef<T> stmRef)
        {
            if (!inTxDict.ContainsKey(stmRef))
            {
                RefTuple<T, long> tuple = stmRef.content;

                inTxDict.Add(stmRef, tuple.Value);
                if (!version.ContainsKey(stmRef))
                {
                    version.Add(stmRef, tuple.Version);
                    parentVersion.Add(stmRef, tuple.ParentVersion);                    
                }
            }
            return (T)inTxDict[stmRef];
        }

        public void Write(StmRef<T> stmRef, T value)
        {
            RefTuple<T, long> tuple = stmRef.content;

            if (!inTxDict.ContainsKey(stmRef))
            {
                inTxDict.Add(stmRef, value);
                toUpdate.Add(stmRef);
            }
            else
            {
                inTxDict[stmRef] = value;
                if (!toUpdate.Contains(stmRef))
                {
                    toUpdate.Add(stmRef);
                }
            }
            if (!version.ContainsKey(stmRef))
            {
                version.Add(stmRef, tuple.Version);
                parentVersion.Add(stmRef, tuple.ParentVersion);
            }
        }

        public void Commit()
        {
            lock(StmModified.commitLock)
            {
                Lock(inTxDict.Keys.ToArray());
                try
                {
                    bool isValid = CheckIsValid();
                    if (isValid)
                    {
                        if (parentTransaction == null)
                        {
                            foreach (StmRef<T> stmRef in toUpdate)
                            {
                                stmRef.content = RefTuple<T, long>.Get((T)inTxDict[stmRef], stmRef.Version, this.Revision);
                            }
                        }
                        else
                        {
                            foreach (StmRef<T> stmRef in toUpdate)
                            {
                                stmRef.content = RefTuple<T, long>.Get((T)inTxDict[stmRef], this.Revision, stmRef.ParentVersion);
                            }
                        }


                    }
                    IsCommited = (isValid || IsParentConflict);
                }
                finally
                {
                    UnLock(inTxDict.Keys.ToArray());
                }
            }            
        }

        private bool CheckIsValid()
        {
            if (parentTransaction == null)
            {
                bool isValid = true;
                foreach (StmRef<T> stmRef in inTxDict.Keys)
                {
                    if (stmRef.ParentVersion != parentVersion[stmRef])
                    {
                        isValid = false;
                        break;
                    }
                    if ((stmRef.Version != version[stmRef]) && (!CheckStmRefIsBelongToSubTransactions(stmRef)))
                    {
                        isValid = false;
                        break;
                    }
                }
                return isValid;
            }
            else
            {
                bool isValid = parentTransaction.CheckParentTransaction();
                if(isValid)
                {
                    foreach (StmRef<T> stmRef in inTxDict.Keys)
                    {
                        if (stmRef.ParentVersion != parentVersion[stmRef])
                        {
                            isValid = false;
                            break;
                        }
                        if ((stmRef.Version != version[stmRef]) && (!CheckStmRefIsBelongToSubTransactions(stmRef)))
                        {
                            isValid = false;
                            break;
                        }
                    }
                }        
                else
                {
                    IsParentConflict = true;
                }        
                return isValid;
            }            
        }

        public bool CheckParentTransaction()
        {
            bool isValid = true;
            foreach (StmRef<T> stmRef in inTxDict.Keys)
            {
                if (stmRef.ParentVersion != parentVersion[stmRef])
                {
                    isValid = false;
                    break;
                }
            }
            return isValid;
        }

        private bool CheckStmRefIsBelongToSubTransactions(StmRef<T> stmRef)
        {
            bool result = false;
            foreach(IStmTransaction<T> transaction in subTransactionList)
            {
                if(stmRef.Version == transaction.Revision)
                {
                    result = true;
                }
            }
            return result;
        }

        private void Lock(StmRef<T>[] stmRefArray)
        {
            if (!isLockedCommit)
            {
                foreach (StmRef<T> stmRef in stmRefArray)
                {
                    Monitor.Enter(stmRef.lockObj);
                }
            }
        }

        private void UnLock(StmRef<T>[] stmRefArray)
        {
            if (!isLockedCommit)
            {
                foreach (StmRef<T> stmRef in stmRefArray)
                {
                    Monitor.Exit(stmRef.lockObj);
                }
            }
        }

        public static void LockStatic(StmTransactionModified<T> stmTransaction)
        {
            foreach (StmRef<T> stmRef in stmTransaction.inTxDict.Keys)
            {
                Monitor.Enter(stmRef.lockObj);
            }
            stmTransaction.isLockedCommit = true;
        }

        public static void UnLockStatic(StmTransactionModified<T> stmTransaction)
        {
            foreach (StmRef<T> stmRef in stmTransaction.inTxDict.Keys)
            {
                Monitor.Exit(stmRef.lockObj);
            }
            stmTransaction.isLockedCommit = false;
        }


        public void Rollback()
        {
            inTxDict.Clear();
            version.Clear();
            parentVersion.Clear();
            toUpdate.Clear();
        }
    }
}
