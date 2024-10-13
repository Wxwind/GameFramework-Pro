using System.Reflection;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Serializers;

namespace GFPro
{
    public class StructBsonSerialize<TValue> : StructSerializerBase<TValue> where TValue : struct
    {
        public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, TValue value)
        {
            var nominalType = args.NominalType;

            var bsonWriter = context.Writer;

            bsonWriter.WriteStartDocument();

            var fields = nominalType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            foreach (var field in fields)
            {
                var bsonElement = field.GetCustomAttribute<BsonElementAttribute>();
                if (bsonElement == null && !field.IsPublic)
                {
                    continue;
                }

                bsonWriter.WriteName(field.Name);
                BsonSerializer.Serialize(bsonWriter, field.FieldType, field.GetValue(value));
            }

            bsonWriter.WriteEndDocument();
        }

        public override TValue Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            //boxing is required for SetValue to work
            object obj = new TValue();
            var actualType = args.NominalType;
            var bsonReader = context.Reader;

            bsonReader.ReadStartDocument();

            while (bsonReader.State != BsonReaderState.EndOfDocument)
            {
                switch (bsonReader.State)
                {
                    case BsonReaderState.Name:
                    {
                        var name = bsonReader.ReadName(Utf8NameDecoder.Instance);
                        var field = actualType.GetField(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                        if (field != null)
                        {
                            var value = BsonSerializer.Deserialize(bsonReader, field.FieldType);
                            field.SetValue(obj, value);
                        }

                        break;
                    }
                    case BsonReaderState.Type:
                    {
                        bsonReader.ReadBsonType();
                        break;
                    }
                    case BsonReaderState.Value:
                    {
                        bsonReader.SkipValue();
                        break;
                    }
                }
            }

            bsonReader.ReadEndDocument();

            return (TValue)obj;
        }
    }
}