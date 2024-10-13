using GFPro.ObjectPool;
using YooAsset;

namespace GFPro
{
    public partial class ResourceComponent
    {
        /// <summary>
        /// 资源对象。
        /// </summary>
        private sealed class AssetObject : ObjectBase
        {
            private AssetHandle m_AssetHandle;

            public AssetObject()
            {
                m_AssetHandle = null;
            }

            public static AssetObject Create(string name, object target, object assetHandle)
            {
                if (assetHandle == null)
                {
                    throw new GameFrameworkException("Resource is invalid.");
                }


                var assetObject = ReferencePool.Acquire<AssetObject>();
                assetObject.Initialize(name, target);
                assetObject.m_AssetHandle = (AssetHandle)assetHandle;
                return assetObject;
            }

            public override void Clear()
            {
                base.Clear();
                m_AssetHandle = null;
            }


            protected internal override void Release(bool isShutdown)
            {
                if (!isShutdown)
                {
                    var handle = m_AssetHandle;
                    if (handle is { IsValid: true })
                    {
                        handle.Release();
                    }
                }
            }
        }
    }
}