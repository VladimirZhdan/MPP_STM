using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MPP_STM
{
    public interface IStmRef<T> where T: struct
    {
        T Get();
        void Set(T value);
    }
}
