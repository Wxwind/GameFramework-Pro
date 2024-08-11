using UnityEditor;
using UnityGameFramework.Runtime;

namespace UnityGameFramework.Editor
{
    [CustomEditor(typeof(UIComponent))]
    internal sealed class UIComponentInspector : GameFrameworkInspector
    {
        private SerializedProperty m_InstanceAutoReleaseInterval = null;
        private SerializedProperty m_InstanceCapacity            = null;
        private SerializedProperty m_InstanceExpireTime          = null;
        private SerializedProperty m_InstancePriority            = null;
        private SerializedProperty m_InstanceRoot                = null;
        private SerializedProperty m_UIGroups                    = null;


        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            serializedObject.Update();

            var t = (UIComponent)target;


            var instanceAutoReleaseInterval = EditorGUILayout.DelayedFloatField("Instance Auto Release Interval", m_InstanceAutoReleaseInterval.floatValue);
            if (instanceAutoReleaseInterval != m_InstanceAutoReleaseInterval.floatValue)
            {
                if (EditorApplication.isPlaying)
                {
                    t.InstanceAutoReleaseInterval = instanceAutoReleaseInterval;
                }
                else
                {
                    m_InstanceAutoReleaseInterval.floatValue = instanceAutoReleaseInterval;
                }
            }

            var instanceCapacity = EditorGUILayout.DelayedIntField("Instance Capacity", m_InstanceCapacity.intValue);
            if (instanceCapacity != m_InstanceCapacity.intValue)
            {
                if (EditorApplication.isPlaying)
                {
                    t.InstanceCapacity = instanceCapacity;
                }
                else
                {
                    m_InstanceCapacity.intValue = instanceCapacity;
                }
            }

            var instanceExpireTime = EditorGUILayout.DelayedFloatField("Instance Expire Time", m_InstanceExpireTime.floatValue);
            if (instanceExpireTime != m_InstanceExpireTime.floatValue)
            {
                if (EditorApplication.isPlaying)
                {
                    t.InstanceExpireTime = instanceExpireTime;
                }
                else
                {
                    m_InstanceExpireTime.floatValue = instanceExpireTime;
                }
            }

            var instancePriority = EditorGUILayout.DelayedIntField("Instance Priority", m_InstancePriority.intValue);
            if (instancePriority != m_InstancePriority.intValue)
            {
                if (EditorApplication.isPlaying)
                {
                    t.InstancePriority = instancePriority;
                }
                else
                {
                    m_InstancePriority.intValue = instancePriority;
                }
            }

            EditorGUI.BeginDisabledGroup(EditorApplication.isPlayingOrWillChangePlaymode);
            {
                EditorGUILayout.PropertyField(m_InstanceRoot);

                EditorGUILayout.PropertyField(m_UIGroups, true);
            }
            EditorGUI.EndDisabledGroup();

            if (EditorApplication.isPlaying && IsPrefabInHierarchy(t.gameObject))
            {
                EditorGUILayout.LabelField("UI Group Count", t.UIGroupCount.ToString());
            }

            serializedObject.ApplyModifiedProperties();

            Repaint();
        }

        protected override void OnCompileComplete()
        {
            base.OnCompileComplete();

            RefreshTypeNames();
        }

        private void OnEnable()
        {
            m_InstanceAutoReleaseInterval = serializedObject.FindProperty("m_InstanceAutoReleaseInterval");
            m_InstanceCapacity = serializedObject.FindProperty("m_InstanceCapacity");
            m_InstanceExpireTime = serializedObject.FindProperty("m_InstanceExpireTime");
            m_InstancePriority = serializedObject.FindProperty("m_InstancePriority");
            m_InstanceRoot = serializedObject.FindProperty("m_InstanceRoot");
            m_UIGroups = serializedObject.FindProperty("m_UIGroups");

            RefreshTypeNames();
        }

        private void RefreshTypeNames()
        {
            serializedObject.ApplyModifiedProperties();
        }
    }
}