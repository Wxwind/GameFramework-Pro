using GameFramework.Resource;

namespace GameFramework.Localization
{
    /// <summary>
    /// 本地化管理器接口。
    /// </summary>
    public interface ILocalizationManager : IDataProvider
    {
        /// <summary>
        /// 获取或设置本地化语言。
        /// </summary>
        Language Language { get; set; }

        /// <summary>
        /// 获取系统语言。
        /// </summary>
        Language SystemLanguage { get; }

        /// <summary>
        /// 获取字典数量。
        /// </summary>
        int DictionaryCount { get; }

        /// <summary>
        /// 设置资源管理器。
        /// </summary>
        /// <param name="resourceManager">资源管理器。</param>
        void SetResourceManager(IResourceManager resourceManager);

        /// <summary>
        /// 设置本地化数据提供者辅助器。
        /// </summary>
        /// <param name="dataProviderHelper">本地化数据提供者辅助器。</param>
        void SetDataProviderHelper(IDataProviderHelper<ILocalizationManager> dataProviderHelper);

        /// <summary>
        /// 设置本地化辅助器。
        /// </summary>
        /// <param name="localizationHelper">本地化辅助器。</param>
        void SetLocalizationHelper(ILocalizationHelper localizationHelper);

        /// <summary>
        /// 根据字典主键获取字典内容字符串。
        /// </summary>
        /// <param name="key">字典主键。</param>
        /// <param name="args">字典参数。</param>
        /// <returns>要获取的字典内容字符串。</returns>
        string GetString(string key, params object[] args);

        /// <summary>
        /// 是否存在字典。
        /// </summary>
        /// <param name="key">字典主键。</param>
        /// <returns>是否存在字典。</returns>
        bool HasRawString(string key);

        /// <summary>
        /// 根据字典主键获取字典值。
        /// </summary>
        /// <param name="key">字典主键。</param>
        /// <returns>字典值。</returns>
        string GetRawString(string key);

        /// <summary>
        /// 增加字典。
        /// </summary>
        /// <param name="key">字典主键。</param>
        /// <param name="value">字典内容。</param>
        /// <returns>是否增加字典成功。</returns>
        bool AddRawString(string key, string value);

        /// <summary>
        /// 移除字典。
        /// </summary>
        /// <param name="key">字典主键。</param>
        /// <returns>是否移除字典成功。</returns>
        bool RemoveRawString(string key);

        /// <summary>
        /// 清空所有字典。
        /// </summary>
        void RemoveAllRawStrings();
    }
}