
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
public partial class TbThruster : IDataTable
{
    private readonly System.Collections.Generic.Dictionary<int, StarForce.Thruster> _dataMap;
    private readonly System.Collections.Generic.List<StarForce.Thruster> _dataList;
    private readonly System.Func<Cysharp.Threading.Tasks.UniTask<JSONNode>> _loadFunc;

    public TbThruster(System.Func<Cysharp.Threading.Tasks.UniTask<JSONNode>> loadFunc)
    {
        _loadFunc = loadFunc;
        _dataMap = new System.Collections.Generic.Dictionary<int, StarForce.Thruster>();
        _dataList = new System.Collections.Generic.List<StarForce.Thruster>();
    }

    public async Cysharp.Threading.Tasks.UniTask LoadAsync()
    {
        JSONNode _json = await _loadFunc();
        _dataMap.Clear();
        _dataList.Clear();
        foreach(JSONNode _ele in _json.Children)
        {
            StarForce.Thruster _v;
            { if(!_ele.IsObject) { throw new SerializationException(); }  _v = StarForce.Thruster.DeserializeThruster(_ele);  }
            _dataList.Add(_v);
            _dataMap.Add(_v.Id, _v);
        }
        PostInit();
    }

    public System.Collections.Generic.Dictionary<int, StarForce.Thruster> DataMap => _dataMap;
    public System.Collections.Generic.List<StarForce.Thruster> DataList => _dataList;
    public StarForce.Thruster GetOrDefault(int key) => _dataMap.TryGetValue(key, out var v) ? v : null;
    public StarForce.Thruster Get(int key) => _dataMap[key];
    public StarForce.Thruster this[int key] => _dataMap[key];

    public void ResolveRef(Tables tables)
    {
        foreach(var _v in _dataList)
        {
            _v.ResolveRef(tables);
        }
    }


    partial void PostInit();
}
}

