namespace UnityGameFramework.Runtime
{
    /// <summary>
    ///     System.Object 变量类。
    /// </summary>
    public sealed class VarObject : Variable<object>
    {
        /// <summary>
        ///     初始化 System.Object 变量类的新实例。
        /// </summary>
        public VarObject()
        {
        }

        public static VarObject FromObject(object value)
        {
            var varValue = ReferencePool.Acquire<VarObject>();
            varValue.Value = value;
            return varValue;
        }

        public static object ToObject(VarObject value)
        {
            return value.Value;
        }
    }
}