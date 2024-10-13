namespace GFPro
{
    public class ConsoleHandlerAttribute : BaseAttribute
    {
        public string Mode { get; }

        public ConsoleHandlerAttribute(string mode)
        {
            Mode = mode;
        }
    }
}