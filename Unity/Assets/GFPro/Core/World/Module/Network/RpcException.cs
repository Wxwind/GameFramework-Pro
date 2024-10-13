using System;

namespace GFPro
{
    /// <summary>
    /// RPC异常,带ErrorCode
    /// </summary>
    public class RpcException : Exception
    {
        public int Error { get; }

        public RpcException(int error, string message) : base(message)
        {
            Error = error;
        }

        public override string ToString()
        {
            return $"Error: {Error}\n{base.ToString()}";
        }
    }
}