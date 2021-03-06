﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Klyte.TransportLinesManager.Interfaces
{
    internal interface IBudgetableExtension
    {
        uint[] GetBudgetsMultiplier(uint prefix);
        uint GetBudgetMultiplierForHour(uint prefix, int hour);
        void SetBudgetMultiplier(uint prefix, uint[] multipliers);
    }

    internal interface INameableExtension
    {
        string GetName(uint prefix);
        void SetName(uint prefix, string name);
    }

    internal interface ITicketPriceExtension
    {
        uint GetTicketPrice(uint rel);
        uint GetDefaultTicketPrice(uint rel);
        void SetTicketPrice(uint rel, uint price);
    }

    internal interface IAssetSelectorExtension
    {
        List<string> GetAssetList(uint rel);
        Dictionary<string, string> GetSelectedBasicAssets(uint rel);
        Dictionary<string, string> GetAllBasicAssets(uint rel);
        void AddAsset(uint rel, string assetId);
        void RemoveAsset(uint rel, string assetId);
        void UseDefaultAssets(uint rel);
        VehicleInfo GetAModel(ushort lineId);
    }
}
