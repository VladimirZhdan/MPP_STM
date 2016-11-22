namespace MPP_STM
{
    public interface ILockRef<T> where T: struct
    {        
        T Get();
        void Set(T value);
    }
}
