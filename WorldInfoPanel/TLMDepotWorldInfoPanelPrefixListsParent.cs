﻿using ColossalFramework.UI;
using Klyte.TransportLinesManager.Extensors.TransportTypeExt;
using Klyte.TransportLinesManager.Interfaces;
using Klyte.TransportLinesManager.Utils;
using Klyte.TransportLinesManager.WorldInfoPanelExt.Components;
using System;
using UnityEngine;

namespace Klyte.TransportLinesManager.WorldInfoPanelExt
{
    public class TLMDepotWorldInfoPanelPrefixListsParent : UIPanel, LateralListSelectParentInterface
    {
        public UIPanel mainPanel => this;
        public event OnWIPOpen eventWipOpen;

        public void WipOpen(ref InstanceID instance)
        {
            eventWipOpen?.Invoke(ref instance);
        }

        public override void Start()
        {
            base.Start();
            relativePosition = new Vector3(parent.width, 0);
            autoLayout = true;
            clipChildren = false;
            autoLayoutDirection = LayoutDirection.Horizontal;
            wrapLayout = false;
            height = parent.height;
            foreach (var kv in TransportSystemDefinition.sysDefinitions)
            {
                Type[] components;
                Type targetType;
                try
                {
                    targetType = TLMUtils.GetImplementationForGenericType(typeof(TLMDepotPrefixSelection<>), kv.Value);
                    components = new Type[] { targetType };
                }
                catch
                {
                    continue;
                }
                TLMUtils.createElement(targetType, transform);
            }
        }

    }
}