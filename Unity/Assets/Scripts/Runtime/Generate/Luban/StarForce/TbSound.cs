
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
public partial class TbSound : IDataTable
{
    private readonly System.Collections.Generic.Dictionary<int, StarForce.Sound> _dataMap;
    private readonly System.Collections.Generic.List<StarForce.Sound> _dataList;
    private readonly System.Func<Cysharp.Threading.Tasks.UniTask<JSONNode>> _loadFunc;

    public TbSound(System.Func<Cysharp.Threading.Tasks.UniTask<JSONNode>> loadFunc)
    {
        _loadFunc = loadFunc;
        _dataMap = new System.Collections.Generic.Dictionary<int, StarForce.Sound>();
        _dataList = new System.Collections.Generic.List<StarForce.Sound>();
    }

    public async Cysharp.Threading.Tasks.UniTask LoadAsync()
    {
        JSONNode _json = await _loadFunc();
        _dataMap.Clear();
        _dataList.Clear();
        foreach(JSONNode _ele in _json.Children)
        {
            StarForce.Sound _v;
            { if(!_ele.IsObject) { throw new SerializationException(); }  _v = StarForce.Sound.DeserializeSound(_ele);  }
            _dataList.Add(_v);
            _dataMap.Add(_v.Id, _v);
        }
        PostInit();
    }

    public System.Collections.Generic.Dictionary<int, StarForce.Sound> DataMap => _dataMap;
    public System.Collections.Generic.List<StarForce.Sound> DataList => _dataList;
    public StarForce.Sound GetOrDefault(int key) => _dataMap.TryGetValue(key, out var v) ? v : null;
    public StarForce.Sound Get(int key) => _dataMap[key];
    public StarForce.Sound this[int key] => _dataMap[key];

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

