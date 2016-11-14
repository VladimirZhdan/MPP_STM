using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MPP_STM
{
    class StmTransaction<T> : IStmTransaction<T> where T : struct
    {
        public void Commit()
        {
            throw new NotImplementedException();
        }

        public object Read(IStmRef<T> source)
        {
            throw new NotImplementedException();
        }

        public void Rollback()
        {
            throw new NotImplementedException();
        }

        public void Write(IStmRef<T> target, object newValue)
        {
            throw new NotImplementedException();
        }
    }
}
