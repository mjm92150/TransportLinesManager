﻿using ColossalFramework;
using ColossalFramework.Globalization;
using ColossalFramework.UI;
using ICities;
using Klyte.Commons.Extensors;
using Klyte.Extensions;
using Klyte.Harmony;
using Klyte.TransportLinesManager.Extensors;
using Klyte.TransportLinesManager.Extensors.TransportLineExt;
using Klyte.TransportLinesManager.Extensors.TransportTypeExt;
using Klyte.TransportLinesManager.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using TLMCW = Klyte.TransportLinesManager.TLMConfigWarehouse;

namespace Klyte.TransportLinesManager.Overrides
{
    class TransportLineOverrides : Redirector<TransportLineOverrides>
    {
        #region Hooking

        private static bool preventDefault()
        {
            return false;
        }

        public override void Awake()
        {
            MethodInfo preventDefault = typeof(TransportLineOverrides).GetMethod("preventDefault", allFlags);

            #region Automation Hooks
            MethodInfo doAutomation = typeof(TransportLineOverrides).GetMethod("doAutomation", allFlags);
            MethodInfo preDoAutomation = typeof(TransportLineOverrides).GetMethod("preDoAutomation", allFlags);

            TLMUtils.doLog("Loading AutoColor & AutoName Hook");
            AddRedirect(typeof(TransportLine).GetMethod("AddStop", allFlags), preDoAutomation, doAutomation);
            #endregion

            #region Ticket Override Hooks
            if (!TLMSingleton.isIPTLoaded)
            {
                MethodInfo GetTicketPricePost_PassengerPlaneAI = typeof(TransportLineOverrides).GetMethod("GetTicketPricePost_PassengerPlaneAI", allFlags);
                MethodInfo GetTicketPricePost_PassengerShipAI = typeof(TransportLineOverrides).GetMethod("GetTicketPricePost_PassengerShipAI", allFlags);
                MethodInfo GetTicketPricePost_TramAI = typeof(TransportLineOverrides).GetMethod("GetTicketPricePost_TramAI", allFlags);
                MethodInfo GetTicketPricePost_PassengerTrainAI = typeof(TransportLineOverrides).GetMethod("GetTicketPricePost_PassengerTrainAI", allFlags);
                MethodInfo GetTicketPricePost_PassengerBlimpAI = typeof(TransportLineOverrides).GetMethod("GetTicketPricePost_PassengerBlimpAI", allFlags);
                MethodInfo GetTicketPricePost_PassengerFerryAI = typeof(TransportLineOverrides).GetMethod("GetTicketPricePost_PassengerFerryAI", allFlags);
                MethodInfo GetTicketPricePost_BusAI = typeof(TransportLineOverrides).GetMethod("GetTicketPricePost_BusAI", allFlags);
                MethodInfo GetTicketPricePost_CableCarAI = typeof(TransportLineOverrides).GetMethod("GetTicketPricePost_CableCarAI", allFlags);

                TLMUtils.doLog("Loading Ticket Override Hooks");
                AddRedirect(typeof(PassengerPlaneAI).GetMethod("GetTicketPrice", allFlags), null, GetTicketPricePost_PassengerPlaneAI);
                AddRedirect(typeof(PassengerShipAI).GetMethod("GetTicketPrice", allFlags), null, GetTicketPricePost_PassengerShipAI);
                AddRedirect(typeof(TramAI).GetMethod("GetTicketPrice", allFlags), null, GetTicketPricePost_TramAI);
                AddRedirect(typeof(PassengerTrainAI).GetMethod("GetTicketPrice", allFlags), null, GetTicketPricePost_PassengerTrainAI);
                AddRedirect(typeof(PassengerBlimpAI).GetMethod("GetTicketPrice", allFlags), null, GetTicketPricePost_PassengerBlimpAI);
                AddRedirect(typeof(PassengerFerryAI).GetMethod("GetTicketPrice", allFlags), null, GetTicketPricePost_PassengerFerryAI);
                AddRedirect(typeof(BusAI).GetMethod("GetTicketPrice", allFlags), null, GetTicketPricePost_BusAI);
                AddRedirect(typeof(CableCarAI).GetMethod("GetTicketPrice", allFlags), null, GetTicketPricePost_CableCarAI);
            }
            #endregion

            #region Budget Override Hooks

            if (!TLMSingleton.isIPTLoaded)
            {
                MethodInfo SimulationStepPre = typeof(TransportLineOverrides).GetMethod("SimulationStepPre", allFlags);

                TLMUtils.doLog("Loading SimulationStepPre Hook");
                AddRedirect(typeof(TransportLine).GetMethod("SimulationStep", allFlags), SimulationStepPre);
            }
            #endregion

        }
        #endregion

        #region On Line Create

        public static void preDoAutomation(ushort lineID, ref TransportLine.Flags __state)
        {
            __state = Singleton<TransportManager>.instance.m_lines.m_buffer[lineID].m_flags;
        }

        public static void doAutomation(ushort lineID, TransportLine.Flags __state)
        {
            TLMUtils.doLog("OLD: " + __state + " ||| NEW: " + Singleton<TransportManager>.instance.m_lines.m_buffer[lineID].m_flags);
            if (lineID > 0 && (__state & TransportLine.Flags.Complete) == TransportLine.Flags.None && (__state & TransportLine.Flags.Temporary) == TransportLine.Flags.None)
            {
                if ((Singleton<TransportManager>.instance.m_lines.m_buffer[lineID].m_flags & TransportLine.Flags.Complete) != TransportLine.Flags.None
                    && (Singleton<TransportManager>.instance.m_lines.m_buffer[lineID].m_flags & (TransportLine.Flags.Temporary)) == TransportLine.Flags.None)
                {
                    if (TLMConfigWarehouse.getCurrentConfigBool(TLMConfigWarehouse.ConfigIndex.AUTO_COLOR_ENABLED))
                    {
                        TLMController.instance.AutoColor(lineID);
                    }
                    if (TLMConfigWarehouse.getCurrentConfigBool(TLMConfigWarehouse.ConfigIndex.AUTO_NAME_ENABLED))
                    {
                        TLMController.instance.AutoName(lineID);
                    }
                    TLMController.instance.LineCreationToolbox.incrementNumber();
                    TLMTransportLineExtension.instance.SafeCleanEntry(lineID);
                }
            }
            if ((Singleton<TransportManager>.instance.m_lines.m_buffer[lineID].m_flags & TransportLine.Flags.Complete) == TransportLine.Flags.None &&
                (Singleton<TransportManager>.instance.m_lines.m_buffer[lineID].m_flags & TransportLine.Flags.CustomColor) != TransportLine.Flags.None
                )
            {
                Singleton<TransportManager>.instance.m_lines.m_buffer[lineID].m_flags &= ~TransportLine.Flags.CustomColor;
            }

        }
        #endregion

        #region Budget Override
        public static bool SimulationStepPre(ushort lineID)
        {
            try
            {
                TransportLine t = Singleton<TransportManager>.instance.m_lines.m_buffer[lineID];
                bool activeDayNightManagedByTLM = TLMLineUtils.isPerHourBudget(lineID);
                if (t.m_lineNumber != 0 && t.m_stops != 0)
                {
                    Singleton<TransportManager>.instance.m_lines.m_buffer[lineID].m_budget = (ushort)(TLMLineUtils.getBudgetMultiplierLine(lineID) * 100);
                }

                unchecked
                {
                    TLMLineUtils.getLineActive(ref Singleton<TransportManager>.instance.m_lines.m_buffer[lineID], out bool day, out bool night);
                    bool isNight = Singleton<SimulationManager>.instance.m_isNightTime;
                    bool zeroed = false;
                    if ((activeDayNightManagedByTLM || (isNight && night) || (!isNight && day)) && Singleton<TransportManager>.instance.m_lines.m_buffer[lineID].m_budget == 0)
                    {
                        Singleton<TransportManager>.instance.m_lines.m_buffer[lineID].m_flags |= (TransportLine.Flags)TLMTransportLineFlags.ZERO_BUDGET_CURRENT;
                        zeroed = true;
                    }
                    else
                    {
                        Singleton<TransportManager>.instance.m_lines.m_buffer[lineID].m_flags &= ~(TransportLine.Flags)TLMTransportLineFlags.ZERO_BUDGET_CURRENT;
                    }
                    TLMUtils.doLog("activeDayNightManagedByTLM = {0}; zeroed = {1}", activeDayNightManagedByTLM, zeroed);
                    if (activeDayNightManagedByTLM)
                    {
                        if (zeroed)
                        {
                            TLMLineUtils.setLineActive(ref Singleton<TransportManager>.instance.m_lines.m_buffer[lineID], false, false);
                        }
                        else
                        {
                            TLMLineUtils.setLineActive(ref Singleton<TransportManager>.instance.m_lines.m_buffer[lineID], true, true);
                        }
                    }
                }

            }
            catch (Exception e)
            {
                TLMUtils.doErrorLog("Error processing budget for line: {0}\n{1}", lineID, e);
            }
            return true;
        }
        #endregion

        #region Ticket Override
        public static void GetTicketPricePost_PassengerPlaneAI(ushort vehicleID, ref Vehicle vehicleData, PassengerPlaneAI __instance, ref int __result)
        {
            if (__instance.m_ticketPrice == 0) __result = 0;
            else __result = ticketPriceForPrefix(vehicleID, ref vehicleData, __instance.m_ticketPrice) * __result / __instance.m_ticketPrice;
        }
        public static void GetTicketPricePost_PassengerShipAI(ushort vehicleID, ref Vehicle vehicleData, PassengerShipAI __instance, ref int __result)
        {
            if (__instance.m_ticketPrice == 0) __result = 0;
            else __result = ticketPriceForPrefix(vehicleID, ref vehicleData, __instance.m_ticketPrice) * __result / __instance.m_ticketPrice;
        }
        public static void GetTicketPricePost_TramAI(ushort vehicleID, ref Vehicle vehicleData, TramAI __instance, ref int __result)
        {
            if (__instance.m_ticketPrice == 0) __result = 0;
            else __result = ticketPriceForPrefix(vehicleID, ref vehicleData, __instance.m_ticketPrice) * __result / __instance.m_ticketPrice;
        }
        public static void GetTicketPricePost_PassengerTrainAI(ushort vehicleID, ref Vehicle vehicleData, PassengerTrainAI __instance, ref int __result)
        {
            if (__instance.m_ticketPrice == 0) __result = 0;
            else __result = ticketPriceForPrefix(vehicleID, ref vehicleData, __instance.m_ticketPrice) * __result / __instance.m_ticketPrice;
        }
        public static void GetTicketPricePost_PassengerBlimpAI(ushort vehicleID, ref Vehicle vehicleData, PassengerBlimpAI __instance, ref int __result)
        {
            if (__instance.m_ticketPrice == 0) __result = 0;
            else __result = ticketPriceForPrefix(vehicleID, ref vehicleData, __instance.m_ticketPrice) * __result / __instance.m_ticketPrice;
        }
        public static void GetTicketPricePost_PassengerFerryAI(ushort vehicleID, ref Vehicle vehicleData, PassengerFerryAI __instance, ref int __result)
        {
            if (__instance.m_ticketPrice == 0) __result = 0;
            else __result = ticketPriceForPrefix(vehicleID, ref vehicleData, __instance.m_ticketPrice) * __result / __instance.m_ticketPrice;
        }
        public static void GetTicketPricePost_BusAI(ushort vehicleID, ref Vehicle vehicleData, BusAI __instance, ref int __result)
        {
            if (__instance.m_ticketPrice == 0) __result = 0;
            else __result = ticketPriceForPrefix(vehicleID, ref vehicleData, __instance.m_ticketPrice) * __result / __instance.m_ticketPrice;
        }
        public static void GetTicketPricePost_CableCarAI(ushort vehicleID, ref Vehicle vehicleData, CableCarAI __instance, ref int __result)
        {
            if (__instance.m_ticketPrice == 0) __result = 0;
            else __result = ticketPriceForPrefix(vehicleID, ref vehicleData, __instance.m_ticketPrice) * __result / __instance.m_ticketPrice;
        }

        private static int ticketPriceForPrefix(ushort vehicleID, ref Vehicle vehicleData, int defaultPrice)
        {
            var def = TransportSystemDefinition.from(vehicleData.Info);

            if (def == default(TransportSystemDefinition))
            {
                if (TLMSingleton.instance != null && TLMSingleton.debugMode)
                    TLMUtils.doLog("NULL TSysDef! {0}+{1}+{2}", vehicleData.Info.GetAI().GetType(), vehicleData.Info.m_class.m_subService, vehicleData.Info.m_vehicleType);
                return defaultPrice;
            }
            if (vehicleData.m_transportLine == 0)
            {
                var value = (int)def.GetTransportExtension().GetDefaultTicketPrice(0);
                return value;
            }
            else
            {
                if (TLMTransportLineExtension.instance.GetUseCustomConfig(vehicleData.m_transportLine))
                {
                    return (int)TLMTransportLineExtension.instance.GetTicketPrice(vehicleData.m_transportLine);
                }
                else
                {
                    return (int)def.GetTransportExtension().GetTicketPrice(TLMLineUtils.getPrefix(vehicleData.m_transportLine));
                }

            }
        }
        #endregion
    }
}
