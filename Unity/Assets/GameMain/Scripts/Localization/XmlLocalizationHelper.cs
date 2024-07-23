using System;
using System.Xml;
using GameFramework.Localization;
using UnityGameFramework.Runtime;

namespace GameMain
{
    /// <summary>
    ///     XML 格式的本地化辅助器。
    /// </summary>
    public class XmlLocalizationHelper : DefaultLocalizationHelper
    {
        /// <summary>
        ///     解析字典。
        /// </summary>
        /// <param name="dictionaryString">要解析的字典字符串。</param>
        /// <param name="userData">用户自定义数据。</param>
        /// <returns>是否解析字典成功。</returns>
        public override bool ParseData(ILocalizationManager localizationManager, string dictionaryString,
            object userData)
        {
            try
            {
                var currentLanguage = GameEntry.Localization.Language.ToString();
                var xmlDocument = new XmlDocument();
                xmlDocument.LoadXml(dictionaryString);
                var xmlRoot = xmlDocument.SelectSingleNode("Dictionaries");
                var xmlNodeDictionaryList = xmlRoot.ChildNodes;
                for (var i = 0; i < xmlNodeDictionaryList.Count; i++)
                {
                    var xmlNodeDictionary = xmlNodeDictionaryList.Item(i);
                    if (xmlNodeDictionary.Name != "Dictionary") continue;

                    var language = xmlNodeDictionary.Attributes.GetNamedItem("Language").Value;
                    if (language != currentLanguage) continue;

                    var xmlNodeStringList = xmlNodeDictionary.ChildNodes;
                    for (var j = 0; j < xmlNodeStringList.Count; j++)
                    {
                        var xmlNodeString = xmlNodeStringList.Item(j);
                        if (xmlNodeString.Name != "String") continue;

                        var key = xmlNodeString.Attributes.GetNamedItem("Key").Value;
                        var value = xmlNodeString.Attributes.GetNamedItem("Value").Value;
                        if (!localizationManager.AddRawString(key, value))
                        {
                            Log.Warning("Can not add raw string with key '{0}' which may be invalid or duplicate.",
                                key);
                            return false;
                        }
                    }
                }

                return true;
            }
            catch (Exception exception)
            {
                Log.Warning("Can not parse dictionary data with exception '{0}'.", exception.ToString());
                return false;
            }
        }
    }
}