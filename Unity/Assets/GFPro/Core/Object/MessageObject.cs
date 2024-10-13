using System;
using MongoDB.Bson.Serialization.Attributes;

namespace GFPro
{
    public abstract class MessageObject : ProtoObject, IMessage, IDisposable, IPool
    {
        public virtual void Dispose()
        {
        }

        [BsonIgnore] public bool IsFromPool { get; set; }
    }
}