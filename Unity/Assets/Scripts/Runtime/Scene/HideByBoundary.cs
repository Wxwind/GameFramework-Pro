using GFPro;
using UnityEngine;

namespace Game
{
    public class HideByBoundary : MonoBehaviour
    {
        private void OnTriggerExit(Collider other)
        {
            var go = other.gameObject;
            var entity = go.GetComponent<GameEntity>();
            if (entity == null)
            {
                Log.Warning($"Unknown GameObject '{go.name}', you must use entity only.");
                Destroy(go);
                return;
            }

            GameEntry.Entity.HideEntity(entity);
        }
    }
}