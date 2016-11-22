namespace MPP_STM
{
    public class StmRef<T> : IStmRef<T> where T: struct
    {
        private int identifier;
        private static int identifierCounter = 1;

        public object lockObj = new object();
        public RefTuple<T, long> content;
        public T Value
        {
            get
            {
                return content.Value;
            }
        }

        public long Version
        {
            get
            {
                return content.Version;
            }
        }

        public long ParentVersion
        {
            get
            {
                return content.ParentVersion;
            }
        }
            

        public StmRef(T value)
        {
            content = RefTuple<T, long>.Get(value, 0, 0);
            identifier = GetNextIdentifier();
        }

        private int GetNextIdentifier()
        {
            int result = identifierCounter;
            ++identifierCounter;
            return result;
        }

        public T Get(IStmTransaction<T> ctx)
        {
            return ctx.Read(this);
        }

        public void Set(T value, IStmTransaction<T> tx)
        {
            tx.Write(this, value);            
        }

        public override string ToString()
        {
            string resultString = base.ToString();
            resultString += ("["+ identifier + "]{ value = " + Value + "; version = " + Version + "; parentVersion = " + ParentVersion + " }");
            return resultString;
        }
    }
}
