using UnityEngine;
using UnityGameFramework;
using UnityGameFramework.Entity;
using UnityGameFramework.Runtime;

namespace Game
{
    /// <summary>
    ///     装甲类。
    /// </summary>
    public class Armor : Entity
    {
        private const string AttachPoint = "Armor Point";

        [SerializeField] private ArmorData m_ArmorData;


        protected override void OnShow(object userData)
        {
            base.OnShow(userData);

            m_ArmorData = userData as ArmorData;
            if (m_ArmorData == null)
            {
                Log.Error("Armor data is invalid.");
                return;
            }

            GameEntry.Entity.AttachEntity(Entity.Id, m_ArmorData.OwnerId, AttachPoint);
        }


        protected override void OnAttachTo(EntityLogic parentEntity, Transform parentTransform, object userData)
        {
            base.OnAttachTo(parentEntity, parentTransform, userData);

            Name = Utility.Text.Format("Armor of {0}", parentEntity.Name);
            CachedTransform.localPosition = Vector3.zero;
        }
    }
}