namespace GFPro
{
    public class ResponseTypeAttribute : BaseAttribute
    {
        public string Type { get; }

        public ResponseTypeAttribute(string type)
        {
            Type = type;
        }
    }
}