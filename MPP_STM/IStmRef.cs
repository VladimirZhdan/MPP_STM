namespace MPP_STM
{
    public interface IStmRef<T> where T: struct
    {        
        T Get(IStmTransaction<T> ctx);
        void Set(T value, IStmTransaction<T> tx);
    }
}
