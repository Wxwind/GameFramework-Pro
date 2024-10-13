
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using Luban;
using SimpleJSON;

namespace Game.Config.StarForce
{
public sealed partial class Entity : Luban.BeanBase
{
    public Entity(JSONNode _buf) 
    {
        { if(!_buf["Id"].IsNumber) { throw new SerializationException(); }  Id = _buf["Id"]; }
        { if(!_buf["AssetName"].IsString) { throw new SerializationException(); }  AssetName = _buf["AssetName"]; }
        PostInit();
    }

    public static Entity DeserializeEntity(JSONNode _buf)
    {
        return new StarForce.Entity(_buf);
    }

    /// <summary>
    /// 实体编号
    /// </summary>
    public readonly int Id;
    /// <summary>
    /// 资源名称
    /// </summary>
    public readonly string AssetName;
    public const int __ID__ = 1731825656;
    public override int GetTypeId() => __ID__;

    public  void ResolveRef(Tables tables)
    {
    }

    public override string ToString()
    {
        return "{ "
        + "Id:" + Id + ","
        + "AssetName:" + AssetName + ","
        + "}";
    }

    partial void PostInit();
}
}
