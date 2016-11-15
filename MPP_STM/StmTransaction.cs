using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MPP_STM
{
    class StmTransaction<T> : IStmTransaction<T> where T : struct
    {
        private Dictionary<StmRef<T>, object> inTxDict = new Dictionary<StmRef<T>, object>();
        private Dictionary<StmRef<T>, long> version = new Dictionary<StmRef<T>, long>();
        private List<StmRef<T>> toUpdate = new List<StmRef<T>>();        
        public long revision { get; private set; }
        private static long transactionNum = 1;

        public bool IsCommited { get; private set; }

        public StmTransaction()
        {
            revision = TransactionNum;
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

                inTxDict.Add(stmRef, tuple.value);
                if(!version.ContainsKey(stmRef))
                {
                    version.Add(stmRef, tuple.revision);
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
            }
            if (!version.ContainsKey(stmRef))
            {
                version.Add(stmRef, stmRef.revision);
            }
        }

        public void Commit()
        {
            lock(Stm.commitLock)
            {
                bool isValid = true;
                foreach(StmRef<T> stmRef in inTxDict.Keys)
                {
                    if(stmRef.revision != version[stmRef])
                    {
                        isValid = false;
                        break;
                    }
                }
                if(isValid)
                {
                    foreach (StmRef<T> stmRef in toUpdate)
                    {
                        stmRef.content = RefTuple<T, long>.Get((T)inTxDict[stmRef], this.revision);                         
                    }
                }
                IsCommited = isValid;                               
            }            
        }                

        public void Rollback()
        {
            inTxDict.Clear();
            version.Clear();
            toUpdate.Clear();
        }                
    }
}
