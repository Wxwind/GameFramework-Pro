using System;
using System.IO;
using System.Text;
using GameFramework.Localization;
using SimpleJSON;
using UnityGameFramework.Runtime;

namespace GameMain
{
    /// <summary>
    ///     XML 格式的本地化辅助器。
    /// </summary>
    public class JsonLocalizationHelper : DefaultLocalizationHelper
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
                var jsonObject = (JSONObject)JSONNode.Parse(dictionaryString);
                foreach (var pair in jsonObject)
                {
                    var dictionaryKey = pair.Key;
                    string dictionaryValue = pair.Value;
                    if (!localizationManager.AddRawString(dictionaryKey, dictionaryValue))
                    {
                        Log.Warning("Can not add raw string with key '{0}' which may be invalid or duplicate.", dictionaryKey);
                        return false;
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

        /// <summary>
        /// 解析字典。
        /// </summary>
        /// <param name="localizationManager">本地化管理器。</param>
        /// <param name="dictionaryBytes">要解析的字典二进制流。</param>
        /// <param name="startIndex">字典二进制流的起始位置。</param>
        /// <param name="length">字典二进制流的长度。</param>
        /// <returns>是否解析字典成功。</returns>
        public override bool ParseData(ILocalizationManager localizationManager, byte[] dictionaryBytes, int startIndex, int length)
        {
            try
            {
                using (var memoryStream = new MemoryStream(dictionaryBytes, startIndex, length, false))
                {
                    using (var binaryReader = new BinaryReader(memoryStream, Encoding.UTF8))
                    {
                        while (binaryReader.BaseStream.Position < binaryReader.BaseStream.Length)
                        {
                            var dictionaryKey = binaryReader.ReadString();
                            var dictionaryValue = binaryReader.ReadString();
                            if (!localizationManager.AddRawString(dictionaryKey, dictionaryValue))
                            {
                                Log.Warning("Can not add raw string with dictionary key '{0}' which may be invalid or duplicate.", dictionaryKey);
                                return false;
                            }
                        }
                    }
                }

                return true;
            }
            catch (Exception exception)
            {
                Log.Warning("Can not parse dictionary bytes with exception '{0}'.", exception);
                return false;
            }
        }
    }
}