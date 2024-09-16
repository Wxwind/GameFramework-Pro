namespace UnityGameFramework.Localization
{
    /// <summary>
    /// 本地化辅助器基类。
    /// </summary>
    public interface ILocalizationHelper : IDataProviderHelper<ILocalizationManager>
    {
        /// <summary>
        /// 获取系统语言。
        /// </summary>
        public Language SystemLanguage { get; }
    }
}