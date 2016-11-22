namespace MPP_STM
{
    public class StmModified
    {
        public static object commitLock = new object();
        public static long commitLockObj = 0;

        public static bool UseLoggingStmTransaction { get; set; }

        public static void Do<T>(TransactionBlockModified<T> block) where T: struct
        {
            IStmTransaction<T> tx = GetStmTransaction<T>();
            block.SetTx(tx);
            bool commited = false;
            while (!commited)
            {
                block.Run();
                tx.Commit();
                commited = tx.IsCommited;
                if (!commited)
                {
                    tx.Rollback();
                }
            }
        }

        private static IStmTransaction<T> GetStmTransaction<T>() where T : struct
        {
            if (UseLoggingStmTransaction)
            {
                return new LoggingStmTransactionModified<T>(new StmTransactionModified<T>());
            }
            else
            {
                return new StmTransactionModified<T>();
            }
        }
    }
}
