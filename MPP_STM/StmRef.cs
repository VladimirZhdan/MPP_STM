using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MPP_STM
{
    public class StmRef<T> : IStmRef<T> where T: struct
    {
        public RefTuple<T, long> content;
        public T value
        {
            get
            {
                return content.value;
            }
        }

        public long revision
        {
            get
            {
                return content.revision;
            }
        }
            

        public StmRef(T value)
        {
            content = RefTuple<T, long>.Get(value, 0);
        }

        public T Get(IStmTransaction<T> ctx)
        {
            return ctx.Read(this);
        }

        public void Set(T value, IStmTransaction<T> tx)
        {
            tx.Write(this, value);            
        }
    }
}
