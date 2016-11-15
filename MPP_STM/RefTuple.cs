using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MPP_STM
{
    public class RefTuple<V, R>
    {
        public V value;
        public R revision;

        public RefTuple(V value, R revision)
        {
            this.value = value;
            this.revision = revision;
        }

        public static RefTuple<V, R> Get(V value, R revision)
        {
            return new RefTuple<V, R>(value, revision);
        }
    }
}
