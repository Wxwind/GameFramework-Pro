using System.ComponentModel;

namespace GFPro
{
    public abstract class ProtoObject : object, ISupportInitialize
    {
        public object Clone()
        {
            var bytes = MongoHelper.Serialize(this);
            return MongoHelper.Deserialize(GetType(), bytes, 0, bytes.Length);
        }

        public virtual void BeginInit()
        {
        }


        public virtual void EndInit()
        {
        }
    }
}