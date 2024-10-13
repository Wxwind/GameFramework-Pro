
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
public sealed partial class Asteroid : Luban.BeanBase
{
    public Asteroid(JSONNode _buf) 
    {
        { if(!_buf["Id"].IsNumber) { throw new SerializationException(); }  Id = _buf["Id"]; }
        { if(!_buf["MaxHP"].IsNumber) { throw new SerializationException(); }  MaxHP = _buf["MaxHP"]; }
        { if(!_buf["Attack"].IsNumber) { throw new SerializationException(); }  Attack = _buf["Attack"]; }
        { if(!_buf["Speed"].IsNumber) { throw new SerializationException(); }  Speed = _buf["Speed"]; }
        { if(!_buf["AngularSpeed"].IsNumber) { throw new SerializationException(); }  AngularSpeed = _buf["AngularSpeed"]; }
        { if(!_buf["DeadEffectId"].IsNumber) { throw new SerializationException(); }  DeadEffectId = _buf["DeadEffectId"]; }
        { if(!_buf["DeadSoundId"].IsNumber) { throw new SerializationException(); }  DeadSoundId = _buf["DeadSoundId"]; }
        PostInit();
    }

    public static Asteroid DeserializeAsteroid(JSONNode _buf)
    {
        return new StarForce.Asteroid(_buf);
    }

    /// <summary>
    /// 小行星编号
    /// </summary>
    public readonly int Id;
    /// <summary>
    /// 最大生命
    /// </summary>
    public readonly int MaxHP;
    /// <summary>
    /// 冲击力
    /// </summary>
    public readonly int Attack;
    /// <summary>
    /// 速度
    /// </summary>
    public readonly float Speed;
    /// <summary>
    /// 角速度
    /// </summary>
    public readonly float AngularSpeed;
    /// <summary>
    /// 死亡特效编号
    /// </summary>
    public readonly int DeadEffectId;
    /// <summary>
    /// 死亡声音编号
    /// </summary>
    public readonly int DeadSoundId;
    public const int __ID__ = -405370992;
    public override int GetTypeId() => __ID__;

    public  void ResolveRef(Tables tables)
    {
    }

    public override string ToString()
    {
        return "{ "
        + "Id:" + Id + ","
        + "MaxHP:" + MaxHP + ","
        + "Attack:" + Attack + ","
        + "Speed:" + Speed + ","
        + "AngularSpeed:" + AngularSpeed + ","
        + "DeadEffectId:" + DeadEffectId + ","
        + "DeadSoundId:" + DeadSoundId + ","
        + "}";
    }

    partial void PostInit();
}
}
