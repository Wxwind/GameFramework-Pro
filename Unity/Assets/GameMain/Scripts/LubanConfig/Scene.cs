
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using Luban;
using SimpleJSON;


namespace cfg
{
public sealed partial class Scene : Luban.BeanBase
{
    public Scene(JSONNode _buf) 
    {
        { if(!_buf["Id"].IsNumber) { throw new SerializationException(); }  Id = _buf["Id"]; }
        { if(!_buf["AssetName"].IsString) { throw new SerializationException(); }  AssetName = _buf["AssetName"]; }
        { if(!_buf["BackgroundMusicId"].IsNumber) { throw new SerializationException(); }  BackgroundMusicId = _buf["BackgroundMusicId"]; }
    }

    public static Scene DeserializeScene(JSONNode _buf)
    {
        return new Scene(_buf);
    }

    /// <summary>
    /// 场景编号
    /// </summary>
    public readonly int Id;
    /// <summary>
    /// 资源名称
    /// </summary>
    public readonly string AssetName;
    /// <summary>
    /// 背景音乐编号
    /// </summary>
    public readonly int BackgroundMusicId;
   
    public const int __ID__ = 79702124;
    public override int GetTypeId() => __ID__;

    public  void ResolveRef(Tables tables)
    {
    }

    public override string ToString()
    {
        return "{ "
        + "Id:" + Id + ","
        + "AssetName:" + AssetName + ","
        + "BackgroundMusicId:" + BackgroundMusicId + ","
        + "}";
    }
}

}

