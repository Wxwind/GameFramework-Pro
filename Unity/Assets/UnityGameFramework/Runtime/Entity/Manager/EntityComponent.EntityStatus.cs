namespace UnityGameFramework.Entity
{
    public sealed partial class EntityComponent
    {
        /// <summary>
        /// 实体状态。
        /// </summary>
        private enum EntityStatus : byte
        {
            Unknown = 0,
            WillInit,
            Inited,
            WillShow,
            Showed,
            WillHide,
            Hidden,
            WillRecycle,
            Recycled
        }
    }
}