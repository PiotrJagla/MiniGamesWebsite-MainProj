﻿using Enums.Monopoly;
using Google.Protobuf.Collections;
using Microsoft.Extensions.Logging.Abstractions;
using Models;
using Models.Monopoly;
using MySql.Data.MySqlClient;
using Org.BouncyCastle.Pkcs;
using Services.GamesServices.Monopoly.Board.Behaviours;
using Services.GamesServices.Monopoly.Board.Behaviours.Buying;
using Services.GamesServices.Monopoly.Board.Behaviours.Monopol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Services.GamesServices.Monopoly.Board.Cells;

public class MonopolyNationCell : MonopolyCell
{
    private Nation OfNation { get; set; }

    private CellBuyingBehaviour BuyingBehaviour;
    private MonopolBehaviour monopolBehaviour;

    private Dictionary<string, Costs> BuildingCosts;

    private string CurrentBuilding;
   

    public MonopolyNationCell(Dictionary<string,Costs> BuildingToCostsMap, Nation nation)
    {
        OfNation = nation;
        BuildingCosts = BuildingToCostsMap;
        BuyingBehaviour = new CellAbleToBuyBehaviour(BuildingCosts[Consts.Monopoly.Field]);
        monopolBehaviour = new MonopolNationCellBehaviour();
        CurrentBuilding = "";
    }

    public Nation GetNation()
    {
        return OfNation;
    }
    public string OnDisplay()
    {
        string result = "";
        result += $" Owner: {BuyingBehaviour.GetOwner().ToString()} |";
        result += $" Nation: {OfNation.ToString()} |";
        if(BuyingBehaviour.GetCosts().Stay != 0)
            result += $" Stay Cost: {BuyingBehaviour.GetCosts().Stay}| ";
        result += $" Building: {CurrentBuilding} ";
        if (BuyingBehaviour.IsThereChampionship() == true)
            result += Consts.Monopoly.ChampionshipInfo;
        return result;
    }
    public Beach GetBeachName()
    {
        return Beach.NoBeach;
    }
    public MonopolyModalParameters GetModalParameters(DataToGetModalParameters Data)
    {
        if (Data.Board[Data.MainPlayer.OnCellIndex].GetBuyingBehavior().GetOwner() == PlayerKey.NoOne)
            return GetModalBuyingCell(Data);
        else if (Data.Board[Data.MainPlayer.OnCellIndex].GetBuyingBehavior().GetOwner() == Data.MainPlayer.Key)
            return GetModalEnhancingCell(Data);

        return new MonopolyModalParameters(new StringModalParameters(), ModalShow.Never);
    }

    private MonopolyModalParameters GetModalBuyingCell(DataToGetModalParameters Data)
    {
        StringModalParameters Parameters = new StringModalParameters();

        Parameters.Title = "What Do You wanna build?";
        Parameters.ButtonsContent.Add(Consts.Monopoly.NoBuildingBought);

        List<string> PossibleBuildingsToBuy = new List<string>();
        PossibleBuildingsToBuy.Add(Consts.Monopoly.Field);
        PossibleBuildingsToBuy.Add(Consts.Monopoly.OneHouse);
        PossibleBuildingsToBuy.Add(Consts.Monopoly.TwoHouses);
        PossibleBuildingsToBuy.Add(Consts.Monopoly.ThreeHouses);

        foreach (var building in PossibleBuildingsToBuy)
        {
            if (IsAbleToBuy(building, Data))
            {
                string ButtonToAdd = building;
                Parameters.Title += $"|{building} Buy: {BuildingCosts[building].Buy} Stay: {BuildingCosts[building].Stay}|";
                //ButtonToAdd += $"|Buy: {BuildingCosts[building].Buy} Stay: {BuildingCosts[building].Stay}";
                Parameters.ButtonsContent.Add(ButtonToAdd);
            }
            
        }
        
        return new MonopolyModalParameters(Parameters, ModalShow.AfterMove);
    }

    private bool IsAbleToBuy(string Building, DataToGetModalParameters Data)
    {
        bool Result = Data.MainPlayer.MoneyOwned >= BuildingCosts[Consts.Monopoly.OneHouse].Buy;

        if (Building == Consts.Monopoly.ThreeHouses && Data.IsThisFirstLap == true)
            return false;

        return Result;
    }

    private MonopolyModalParameters GetModalEnhancingCell(DataToGetModalParameters Data)
    {
        StringModalParameters Parameters = new StringModalParameters();

        Parameters.Title = "What Do You wanna build?";
        Parameters.ButtonsContent.Add(Consts.Monopoly.NoBuildingBought);

        int CurrentBuildingTier = BuyingTiers.GetBuyTierNumber(CurrentBuilding);
        List<string> PossibleEnhanceBuildings = new List<string>();
        PossibleEnhanceBuildings.Add(Consts.Monopoly.OneHouse);
        PossibleEnhanceBuildings.Add(Consts.Monopoly.TwoHouses);
        PossibleEnhanceBuildings.Add(Consts.Monopoly.ThreeHouses);
        PossibleEnhanceBuildings.Add(Consts.Monopoly.Hotel);

        foreach (var building in PossibleEnhanceBuildings)
        {
            if (IsAbleToEnhance(Data, building))
                Parameters.ButtonsContent.Add(building);
        }

        return new MonopolyModalParameters(Parameters, ModalShow.AfterMove);
    }

    private bool IsAbleToEnhance(DataToGetModalParameters Data, string WhatIsBought)
    {
        bool Result = Data.MainPlayer.MoneyOwned >= BuildingCosts[Consts.Monopoly.OneHouse].Buy &&
                BuyingTiers.GetBuyTierNumber(WhatIsBought) > BuyingTiers.GetBuyTierNumber(CurrentBuilding);

        if (WhatIsBought == Consts.Monopoly.Hotel)
            Result = Result && CurrentBuilding == Consts.Monopoly.ThreeHouses;

        return Result;
    }

    public CellBuyingBehaviour GetBuyingBehavior()
    {
        return BuyingBehaviour;
    }

    public int CellBought(MonopolyPlayer MainPlayer, string WhatIsBought,ref List<MonopolyCell> CheckMonopol)
    {
        if (WhatIsBought != Consts.Monopoly.NoBuildingBought &&
            string.IsNullOrEmpty(WhatIsBought) == false)
        {
            BuyingBehaviour.SetOwner(MainPlayer.Key);
            BuyingBehaviour.SetBaseCosts(BuildingCosts[WhatIsBought]);
            CheckMonopol = monopolBehaviour.UpdateBoardMonopol(CheckMonopol, MainPlayer.OnCellIndex);
            CurrentBuilding = WhatIsBought;
            return BuyingBehaviour.GetCosts().Buy;
        }

        return 0;
    }

    public void CellSold(ref List<MonopolyCell> MonopolChanges)
    {
        int CellIndex = MonopolChanges.IndexOf(this);
        BuyingBehaviour.SetOwner(PlayerKey.NoOne);
        MonopolChanges = monopolBehaviour.GetMonopolOff(MonopolChanges, CellIndex);
    }

    public ModalResponseUpdate OnModalResponse(ModalResponseData Data)
    {
        ModalResponseUpdate UpdatedData = new ModalResponseUpdate();
        UpdatedData.BoardService = Data.BoardService;
        UpdatedData.PlayersService = Data.PlayersService;

        MonopolyPlayer MainPlayer = UpdatedData.PlayersService.GetMainPlayer();
        int BuyCost = UpdatedData.BoardService.BuyCell(MainPlayer, Data.ModalResponse);
        UpdatedData.PlayersService.ChargeMainPlayer(BuyCost);

        return UpdatedData;
    }
}
