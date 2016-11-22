using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace MPP_STM
{
    class StmTransaction<T> : IStmTransaction<T> where T : struct
    {
        private Dictionary<StmRef<T>, object> inTxDict = new Dictionary<StmRef<T>, object>();
        private Dictionary<StmRef<T>, long> version = new Dictionary<StmRef<T>, long>();
        private List<StmRef<T>> toUpdate = new List<StmRef<T>>();        
        public long Revision { get; private set; }
        public static long transactionNum = 1;       

        public bool IsCommited { get; private set; }

        private bool isLockedCommit = false;

        public StmTransaction()
        {
            Revision = TransactionNum;
            IsCommited = false;   
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
                if(!version.ContainsKey(stmRef))
                {
                    version.Add(stmRef, tuple.Version);
                }
            }
            return (T)inTxDict[stmRef];
        }

        public void Write(StmRef<T> stmRef, T value)
        {            
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
                version.Add(stmRef, stmRef.Version);
            }
        }

        public void Commit()
        {
            Lock(inTxDict.Keys.ToArray());            
            try
            {
                bool isValid = true;
                foreach (StmRef<T> stmRef in inTxDict.Keys)
                {
                    if (stmRef.Version != version[stmRef])
                    {
                        isValid = false;
                        break;
                    }
                }
                if (isValid)
                {
                    foreach (StmRef<T> stmRef in toUpdate)
                    {
                        stmRef.content = RefTuple<T, long>.Get((T)inTxDict[stmRef], this.Revision, 0);
                    }
                }
                IsCommited = isValid;
            }
            finally
            {
                UnLock(inTxDict.Keys.ToArray());
            }            
        }

        private void Lock(StmRef<T>[] stmRefArray)
        {
            if(!isLockedCommit)
            {
                foreach (StmRef<T> stmRef in stmRefArray)
                {
                    Monitor.Enter(stmRef.lockObj);
                }
            }                        
        }

        private void UnLock(StmRef<T>[] stmRefArray)
        {
            if(!isLockedCommit)
            {
                foreach (StmRef<T> stmRef in stmRefArray)
                {
                    Monitor.Exit(stmRef.lockObj);
                }
            }            
        }

        public static void LockStatic(StmTransaction<T> stmTransaction)
        {
            foreach (StmRef<T> stmRef in stmTransaction.inTxDict.Keys)
            {
                Monitor.Enter(stmRef.lockObj);
            }
            stmTransaction.isLockedCommit = true;
        }

        public static void UnLockStatic(StmTransaction<T> stmTransaction)
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
            toUpdate.Clear();
        }

        public void SetParentTransaction(IStmTransaction<T> parentTransaction)
        {
            
        }

        public void AddSubTransaction(IStmTransaction<T> subTransaction)
        {
           
        }

        public bool CheckParentTransaction()
        {
            throw new NotImplementedException();
        }
    }
}
