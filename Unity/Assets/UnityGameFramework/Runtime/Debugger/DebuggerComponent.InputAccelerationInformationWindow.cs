﻿using UnityEngine;

namespace GFPro
{
    public sealed partial class DebuggerComponent : GameFrameworkComponent
    {
        private sealed class InputAccelerationInformationWindow : ScrollableDebuggerWindowBase
        {
            protected override void OnDrawScrollableWindow()
            {
                GUILayout.Label("<b>Input Acceleration Information</b>");
                GUILayout.BeginVertical("box");
                {
                    DrawItem("Acceleration", Input.acceleration.ToString());
                    DrawItem("Acceleration Event Count", Input.accelerationEventCount.ToString());
                    DrawItem("Acceleration Events", GetAccelerationEventsString(Input.accelerationEvents));
                }
                GUILayout.EndVertical();
            }

            private string GetAccelerationEventString(AccelerationEvent accelerationEvent)
            {
                return Utility.Text.Format("{0}, {1}", accelerationEvent.acceleration, accelerationEvent.deltaTime);
            }

            private string GetAccelerationEventsString(AccelerationEvent[] accelerationEvents)
            {
                var accelerationEventStrings = new string[accelerationEvents.Length];
                for (var i = 0; i < accelerationEvents.Length; i++)
                {
                    accelerationEventStrings[i] = GetAccelerationEventString(accelerationEvents[i]);
                }

                return string.Join("; ", accelerationEventStrings);
            }
        }
    }
}