using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MPP_STM
{
    public interface IStmTransaction<T> where T: struct
    {
        void Commit();
        void Rollback();
        object Read(IStmRef<T> source);
        void Write(IStmRef<T> target, object newValue);
    }
}
