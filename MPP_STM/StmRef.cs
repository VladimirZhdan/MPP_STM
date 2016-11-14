using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MPP_STM
{
    public class StmRef<T> : IStmRef<T> where T: struct
    {
        private T stmElement;

        public T Get()
        {
            return stmElement;
        }

        public void Set(T value)
        {
            stmElement = value;
        }
    }
}
