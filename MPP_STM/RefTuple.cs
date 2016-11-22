namespace MPP_STM
{
    public class RefTuple<V, R>
    {
        public V Value { get; private set; }
        public R Version { get; private set; }
        public R ParentVersion { get; private set; }

        public RefTuple(V value, R version, R parentVersion)
        {
            this.Value = value;
            this.Version = version;
            this.ParentVersion = parentVersion;
        }

        public static RefTuple<V, R> Get(V value, R version, R parentVersion)
        {
            return new RefTuple<V, R>(value, version, parentVersion);
        }
    }
}
