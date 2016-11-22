namespace MPP_STM
{
    public interface IStmTransaction<T> where T: struct
    {
        bool IsCommited { get; }
        long Revision { get; }
        void Commit();
        void Rollback();
        T Read(StmRef<T> source);
        void Write(StmRef<T> target, T newValue);
        void SetParentTransaction(IStmTransaction<T> parentTransaction);
        void AddSubTransaction(IStmTransaction<T> subTransaction);
        bool CheckParentTransaction();
    }
}
