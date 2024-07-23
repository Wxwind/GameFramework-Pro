namespace UnityGameFramework.Editor.ResourceTools
{
    public sealed partial class ResourceBuilderController
    {
        private sealed class VersionListData
        {
            public VersionListData(string path, int length, int hashCode, int compressedLength, int compressedHashCode)
            {
                Path = path;
                Length = length;
                HashCode = hashCode;
                CompressedLength = compressedLength;
                CompressedHashCode = compressedHashCode;
            }

            public string Path
            {
                get;
                private set;
            }

            public int Length
            {
                get;
                private set;
            }

            public int HashCode
            {
                get;
                private set;
            }

            public int CompressedLength
            {
                get;
                private set;
            }

            public int CompressedHashCode
            {
                get;
                private set;
            }
        }
    }
}
