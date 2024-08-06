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
        /// <param name="localizationManager"></param>
        /// <param name="dictionaryString">要解析的字典字符串。</param>
        /// <returns>是否解析字典成功。</returns>
        public override bool ParseData(ILocalizationManager localizationManager, string dictionaryString)
        {
            try
            {
                string currentLanguage = GameEntry.Localization.Language.ToString();
                var xmlDocument = new XmlDocument();
                xmlDocument.LoadXml(dictionaryString);
                var xmlRoot = xmlDocument.SelectSingleNode("Dictionaries");
                var xmlNodeDictionaryList = xmlRoot.ChildNodes;
                for (int i = 0; i < xmlNodeDictionaryList.Count; i++)
                {
                    var xmlNodeDictionary = xmlNodeDictionaryList.Item(i);
                    if (xmlNodeDictionary.Name != "Dictionary") continue;

                    string language = xmlNodeDictionary.Attributes.GetNamedItem("Language").Value;
                    if (language != currentLanguage) continue;

                    var xmlNodeStringList = xmlNodeDictionary.ChildNodes;
                    for (int j = 0; j < xmlNodeStringList.Count; j++)
                    {
                        var xmlNodeString = xmlNodeStringList.Item(j);
                        if (xmlNodeString.Name != "String") continue;

                        string key = xmlNodeString.Attributes.GetNamedItem("Key").Value;
                        string value = xmlNodeString.Attributes.GetNamedItem("Value").Value;
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