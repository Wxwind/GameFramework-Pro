
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using Luban;
using SimpleJSON;


namespace cfg.StarForce
{
public partial class TbUISound
{
    private readonly System.Collections.Generic.Dictionary<int, UISound> _dataMap;
    private readonly System.Collections.Generic.List<UISound> _dataList;
    
    public TbUISound(JSONNode _buf)
    {
        _dataMap = new System.Collections.Generic.Dictionary<int, UISound>();
        _dataList = new System.Collections.Generic.List<UISound>();
        
        foreach(JSONNode _ele in _buf.Children)
        {
            UISound _v;
            { if(!_ele.IsObject) { throw new SerializationException(); }  _v = UISound.DeserializeUISound(_ele);  }
            _dataList.Add(_v);
            _dataMap.Add(_v.Id, _v);
        }
    }

    public System.Collections.Generic.Dictionary<int, UISound> DataMap => _dataMap;
    public System.Collections.Generic.List<UISound> DataList => _dataList;

    public UISound GetOrDefault(int key) => _dataMap.TryGetValue(key, out var v) ? v : null;
    public UISound Get(int key) => _dataMap[key];
    public UISound this[int key] => _dataMap[key];

    public void ResolveRef(Tables tables)
    {
        foreach(var _v in _dataList)
        {
            _v.ResolveRef(tables);
        }
    }

}

}

