using System;

namespace MPP_STM
{
    public class LockManager
    {
        private static object lockObj = new object();

        public static void Do(Action operation)
        {
            lock(lockObj)
            {
                operation.Invoke();
            }
        }
    }
}
