#if ENABLE_VIEW && UNITY_EDITOR
using UnityEngine;

namespace GFPro
{
    public class ComponentView: MonoBehaviour
    {
        public Entity Component
        {
            get;
            set;
        }
    }
}
#endif