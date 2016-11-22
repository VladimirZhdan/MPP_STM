namespace MPP_STM
{
    public class Stm
    {        
        public static bool UseLoggingStmTransaction { get; set; }        

        public static void Do<T>(TransactionBlock<T> block) where T: struct
        {
            IStmTransaction<T> tx = GetStmTransaction<T>();            
            block.SetTx(tx);
            bool commited = false;
            while(!commited)
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

        private static IStmTransaction<T> GetStmTransaction<T>() where T:struct
        {
            if (UseLoggingStmTransaction)
            {
                return new LoggingStmTransaction<T>(new StmTransaction<T>());
            }
            else
            {
                return new StmTransaction<T>();
            }                            
        }
    }
}
