namespace GFPro
{
    public sealed partial class DebuggerComponent : GameFrameworkComponent
    {
        private sealed partial class RuntimeMemorySummaryWindow : ScrollableDebuggerWindowBase
        {
            private sealed class Record
            {
                private readonly string m_Name;
                private          int    m_Count;
                private          long   m_Size;

                public Record(string name)
                {
                    m_Name = name;
                    m_Count = 0;
                    m_Size = 0L;
                }

                public string Name => m_Name;

                public int Count
                {
                    get => m_Count;
                    set => m_Count = value;
                }

                public long Size
                {
                    get => m_Size;
                    set => m_Size = value;
                }
            }
        }
    }
}