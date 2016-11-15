using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MPP_STM
{
    public interface IStmTransaction<T> where T: struct
    {
        bool IsCommited { get; }
        void Commit();
        void Rollback();
        T Read(StmRef<T> source);
        void Write(StmRef<T> target, T newValue);
    }
}
