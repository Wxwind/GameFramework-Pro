﻿namespace GFPro
{
    /// <summary>
    /// System.UInt16 变量类。
    /// </summary>
    public sealed class VarUInt16 : Variable<ushort>
    {
        /// <summary>
        /// 初始化 System.UInt16 变量类的新实例。
        /// </summary>
        public VarUInt16()
        {
        }

        /// <summary>
        /// 从 System.UInt16 到 System.UInt16 变量类的隐式转换。
        /// </summary>
        /// <param name="value">值。</param>
        public static implicit operator VarUInt16(ushort value)
        {
            var varValue = ReferencePool.Acquire<VarUInt16>();
            varValue.Value = value;
            return varValue;
        }

        /// <summary>
        /// 从 System.UInt16 变量类到 System.UInt16 的隐式转换。
        /// </summary>
        /// <param name="value">值。</param>
        public static implicit operator ushort(VarUInt16 value)
        {
            return value.Value;
        }
    }
}