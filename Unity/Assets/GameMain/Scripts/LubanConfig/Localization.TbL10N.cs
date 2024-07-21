
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using Luban;
using SimpleJSON;


namespace cfg.Localization
{
public partial class TbL10N
{
    private readonly System.Collections.Generic.Dictionary<string, L10N> _dataMap;
    private readonly System.Collections.Generic.List<L10N> _dataList;
    
    public TbL10N(JSONNode _buf)
    {
        _dataMap = new System.Collections.Generic.Dictionary<string, L10N>();
        _dataList = new System.Collections.Generic.List<L10N>();
        
        foreach(JSONNode _ele in _buf.Children)
        {
            L10N _v;
            { if(!_ele.IsObject) { throw new SerializationException(); }  _v = L10N.DeserializeL10N(_ele);  }
            _dataList.Add(_v);
            _dataMap.Add(_v.Key, _v);
        }
    }

    public System.Collections.Generic.Dictionary<string, L10N> DataMap => _dataMap;
    public System.Collections.Generic.List<L10N> DataList => _dataList;

    public L10N GetOrDefault(string key) => _dataMap.TryGetValue(key, out var v) ? v : null;
    public L10N Get(string key) => _dataMap[key];
    public L10N this[string key] => _dataMap[key];

    public void ResolveRef(Tables tables)
    {
        foreach(var _v in _dataList)
        {
            _v.ResolveRef(tables);
        }
    }

}

}

