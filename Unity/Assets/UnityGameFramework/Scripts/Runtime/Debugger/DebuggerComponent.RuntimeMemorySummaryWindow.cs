using System;
using System.Collections.Generic;
using GameFramework;
using UnityEngine;
using UnityEngine.Profiling;
using Object = UnityEngine.Object;

namespace UnityGameFramework.Runtime
{
    public sealed partial class DebuggerComponent : GameFrameworkComponent
    {
        private sealed partial class RuntimeMemorySummaryWindow : ScrollableDebuggerWindowBase
        {
            private readonly List<Record>       m_Records        = new();
            private readonly Comparison<Record> m_RecordComparer = RecordComparer;
            private          DateTime           m_SampleTime     = DateTime.MinValue;
            private          int                m_SampleCount;
            private          long               m_SampleSize;

            protected override void OnDrawScrollableWindow()
            {
                GUILayout.Label("<b>Runtime Memory Summary</b>");
                GUILayout.BeginVertical("box");
                {
                    if (GUILayout.Button("Take Sample", GUILayout.Height(30f)))
                    {
                        TakeSample();
                    }

                    if (m_SampleTime <= DateTime.MinValue)
                    {
                        GUILayout.Label("<b>Please take sample first.</b>");
                    }
                    else
                    {
                        GUILayout.Label(Utility.Text.Format(
                            "<b>{0} Objects ({1}) obtained at {2:yyyy-MM-dd HH:mm:ss}.</b>", m_SampleCount,
                            GetByteLengthString(m_SampleSize), m_SampleTime.ToLocalTime()));

                        GUILayout.BeginHorizontal();
                        {
                            GUILayout.Label("<b>Type</b>");
                            GUILayout.Label("<b>Count</b>", GUILayout.Width(120f));
                            GUILayout.Label("<b>Size</b>", GUILayout.Width(120f));
                        }
                        GUILayout.EndHorizontal();

                        for (var i = 0; i < m_Records.Count; i++)
                        {
                            GUILayout.BeginHorizontal();
                            {
                                GUILayout.Label(m_Records[i].Name);
                                GUILayout.Label(m_Records[i].Count.ToString(), GUILayout.Width(120f));
                                GUILayout.Label(GetByteLengthString(m_Records[i].Size), GUILayout.Width(120f));
                            }
                            GUILayout.EndHorizontal();
                        }
                    }
                }
                GUILayout.EndVertical();
            }

            private void TakeSample()
            {
                m_Records.Clear();
                m_SampleTime = DateTime.UtcNow;
                m_SampleCount = 0;
                m_SampleSize = 0L;

                var samples = Resources.FindObjectsOfTypeAll<Object>();
                for (var i = 0; i < samples.Length; i++)
                {
                    var sampleSize = 0L;
                    sampleSize = Profiler.GetRuntimeMemorySizeLong(samples[i]);
                    var name = samples[i].GetType().Name;
                    m_SampleCount++;
                    m_SampleSize += sampleSize;

                    Record record = null;
                    foreach (var r in m_Records)
                    {
                        if (r.Name == name)
                        {
                            record = r;
                            break;
                        }
                    }

                    if (record == null)
                    {
                        record = new Record(name);
                        m_Records.Add(record);
                    }

                    record.Count++;
                    record.Size += sampleSize;
                }

                m_Records.Sort(m_RecordComparer);
            }

            private static int RecordComparer(Record a, Record b)
            {
                var result = b.Size.CompareTo(a.Size);
                if (result != 0)
                {
                    return result;
                }

                result = a.Count.CompareTo(b.Count);
                if (result != 0)
                {
                    return result;
                }

                return a.Name.CompareTo(b.Name);
            }
        }
    }
}