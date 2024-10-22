﻿using UnityEngine;

namespace Game
{
    public static partial class Constant
    {
        /// <summary>
        ///     层。
        /// </summary>
        public static class Layer
        {
            public const string DefaultLayerName = "Default";

            public const string UILayerName = "UI";

            public const           string TargetableObjectLayerName = "Targetable Object";
            public static readonly int    DefaultLayerId            = LayerMask.NameToLayer(DefaultLayerName);
            public static readonly int    UILayerId                 = LayerMask.NameToLayer(UILayerName);
            public static readonly int    TargetableObjectLayerId   = LayerMask.NameToLayer(TargetableObjectLayerName);
        }
    }
}