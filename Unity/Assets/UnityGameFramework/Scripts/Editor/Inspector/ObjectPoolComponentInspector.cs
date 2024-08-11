using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using GameFramework;
using GameFramework.ObjectPool;
using UnityEditor;
using UnityEngine;
using UnityGameFramework.Runtime;

namespace UnityGameFramework.Editor
{
    [CustomEditor(typeof(ObjectPoolComponent))]
    internal sealed class ObjectPoolComponentInspector : GameFrameworkInspector
    {
        private readonly HashSet<string> m_OpenedItems = new();

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (!EditorApplication.isPlaying)
            {
                EditorGUILayout.HelpBox("Available during runtime only.", MessageType.Info);
                return;
            }

            var t = (ObjectPoolComponent)target;

            if (IsPrefabInHierarchy(t.gameObject))
            {
                EditorGUILayout.LabelField("Object Pool Count", t.Count.ToString());

                var objectPools = t.GetAllObjectPools(true);
                foreach (var objectPool in objectPools)
                {
                    DrawObjectPool(objectPool);
                }
            }

            Repaint();
        }

        private void OnEnable()
        {
        }

        private void DrawObjectPool(ObjectPoolBase objectPool)
        {
            var lastState = m_OpenedItems.Contains(objectPool.FullName);
            var currentState = EditorGUILayout.Foldout(lastState, objectPool.FullName);
            if (currentState != lastState)
            {
                if (currentState)
                {
                    m_OpenedItems.Add(objectPool.FullName);
                }
                else
                {
                    m_OpenedItems.Remove(objectPool.FullName);
                }
            }

            if (currentState)
            {
                EditorGUILayout.BeginVertical("box");
                {
                    EditorGUILayout.LabelField("Name", objectPool.Name);
                    EditorGUILayout.LabelField("Type", objectPool.ObjectType.FullName);
                    EditorGUILayout.LabelField("Auto Release Interval", objectPool.AutoReleaseInterval.ToString());
                    EditorGUILayout.LabelField("Capacity", objectPool.Capacity.ToString());
                    EditorGUILayout.LabelField("Used Count", objectPool.Count.ToString());
                    EditorGUILayout.LabelField("Can Release Count", objectPool.CanReleaseCount.ToString());
                    EditorGUILayout.LabelField("Expire Time", objectPool.ExpireTime.ToString());
                    EditorGUILayout.LabelField("Priority", objectPool.Priority.ToString());
                    var objectInfos = objectPool.GetAllObjectInfos();
                    if (objectInfos.Length > 0)
                    {
                        EditorGUILayout.LabelField("Name",
                            objectPool.AllowMultiSpawn ? "Locked\tCount\tFlag\tPriority\tLast Use Time" : "Locked\tIn Use\tFlag\tPriority\tLast Use Time");
                        foreach (var objectInfo in objectInfos)
                        {
                            EditorGUILayout.LabelField(string.IsNullOrEmpty(objectInfo.Name) ? "<None>" : objectInfo.Name,
                                objectPool.AllowMultiSpawn
                                    ? Utility.Text.Format("{0}\t{1}\t{2}\t{3}\t{4:yyyy-MM-dd HH:mm:ss}", objectInfo.Locked, objectInfo.SpawnCount, objectInfo.CustomCanReleaseFlag,
                                        objectInfo.Priority, objectInfo.LastUseTime.ToLocalTime())
                                    : Utility.Text.Format("{0}\t{1}\t{2}\t{3}\t{4:yyyy-MM-dd HH:mm:ss}", objectInfo.Locked, objectInfo.IsInUse, objectInfo.CustomCanReleaseFlag,
                                        objectInfo.Priority, objectInfo.LastUseTime.ToLocalTime()));
                        }

                        if (GUILayout.Button("Release"))
                        {
                            objectPool.Release();
                        }

                        if (GUILayout.Button("Release All Unused"))
                        {
                            objectPool.ReleaseAllUnused();
                        }

                        if (GUILayout.Button("Export CSV Data"))
                        {
                            var exportFileName = EditorUtility.SaveFilePanel("Export CSV Data", string.Empty, Utility.Text.Format("Object Pool Data - {0}.csv", objectPool.Name),
                                string.Empty);
                            if (!string.IsNullOrEmpty(exportFileName))
                            {
                                try
                                {
                                    var index = 0;
                                    var data = new string[objectInfos.Length + 1];
                                    data[index++] = Utility.Text.Format("Name,Locked,{0},Custom Can Release Flag,Priority,Last Use Time",
                                        objectPool.AllowMultiSpawn ? "Count" : "In Use");
                                    foreach (var objectInfo in objectInfos)
                                    {
                                        data[index++] = objectPool.AllowMultiSpawn
                                            ? Utility.Text.Format("{0},{1},{2},{3},{4},{5:yyyy-MM-dd HH:mm:ss}", objectInfo.Name, objectInfo.Locked, objectInfo.SpawnCount,
                                                objectInfo.CustomCanReleaseFlag, objectInfo.Priority, objectInfo.LastUseTime.ToLocalTime())
                                            : Utility.Text.Format("{0},{1},{2},{3},{4},{5:yyyy-MM-dd HH:mm:ss}", objectInfo.Name, objectInfo.Locked, objectInfo.IsInUse,
                                                objectInfo.CustomCanReleaseFlag, objectInfo.Priority, objectInfo.LastUseTime.ToLocalTime());
                                    }

                                    File.WriteAllLines(exportFileName, data, Encoding.UTF8);
                                    Debug.Log(Utility.Text.Format("Export object pool CSV data to '{0}' success.", exportFileName));
                                }
                                catch (Exception exception)
                                {
                                    Debug.LogError(Utility.Text.Format("Export object pool CSV data to '{0}' failure, exception is '{1}'.", exportFileName, exception));
                                }
                            }
                        }
                    }
                    else
                    {
                        GUILayout.Label("Object Pool is Empty ...");
                    }
                }
                EditorGUILayout.EndVertical();

                EditorGUILayout.Separator();
            }
        }
    }
}