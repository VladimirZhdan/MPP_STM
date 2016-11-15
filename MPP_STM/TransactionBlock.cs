using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MPP_STM
{
    class TransactionBlock<T> where T: struct
    {
        private IStmTransaction<T> tx;
        private Action<IStmTransaction<T> > operation;

        public TransactionBlock(Action<IStmTransaction<T> > operation)
        {
            this.operation = operation;                
        }

        public void SetTx(IStmTransaction<T> tx)
        {
            this.tx = tx;
        }

        public IStmTransaction<T> getTx()
        {
            return tx;
        }

        public virtual void Run()
        {
            operation.Invoke(this.getTx());
        }
    }
}
