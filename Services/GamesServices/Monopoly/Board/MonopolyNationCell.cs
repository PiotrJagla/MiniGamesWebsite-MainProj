﻿using Enums.Monopoly;
using Models.Monopoly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.GamesServices.Monopoly.Board
{
    public class MonopolyNationCell : MonopolyCell
    {
        public PlayerKey OwnedBy { get; set; }

        public Costs ActualCosts { get; set; }
        public Costs BaseCosts { get; set; }

        public Nation OfNation { get; set; }


        public MonopolyNationCell(Costs costs = null, Nation nation = Nation.NoNation)
        {
            ActualCosts = new Costs(costs.Buy, costs.Stay);
            BaseCosts = new Costs(costs.Buy, costs.Stay);
            OfNation = nation;
            OwnedBy = PlayerKey.NoOne;
        }

        public Nation GetNation()
        {
            return OfNation;
        }

        public PlayerKey GetOwner()
        {
            return OwnedBy;
        }

        public Costs GetCosts()
        {
            return ActualCosts;
        }

        public void SetCosts(Costs costs)
        {
            ActualCosts.Stay = costs.Stay;
            ActualCosts.Buy = costs.Buy;
        }

        public string OnDisplay()
        {
            string result = "";
            result += $" Owner: {OwnedBy.ToString()} |";
            result += $" Nation: {OfNation.ToString()} |";
            result += $" Buy For: {ActualCosts.Buy} |";
            result += $" Stay Cost: {ActualCosts.Stay} ";
            return result;
        }

        public void SetOwner(PlayerKey NewOwner)
        {
            OwnedBy = NewOwner;
        }
    }
}
