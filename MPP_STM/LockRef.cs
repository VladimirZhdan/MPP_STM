﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MPP_STM
{
    public class LockRef<T> : ILockRef<T> where T: struct
    {
        private T value;

        public LockRef(T value)
        {
            this.value = value;
        }

        public T Get()
        {
            return value;
        }

        public void Set(T value)
        {
            this.value = value;
        }
    }
}
