﻿using Services.GamesServices.Monopoly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Models.MultiplayerConnection;
using Enums.Monopoly;
using Models.Monopoly;
using Services.GamesServices.Monopoly.Board;

namespace UnitTests.MonopolyTests
{
    [TestClass]
    public class MonopolyClientsNumberIndependentTests
    {
        private MonopolyService Client;

        [TestInitialize]
        public void TestsSetup()
        {
            Client = MonopolyDataPrepare.InitClients(1)[0];
        }


        [TestMethod]
        public void TestBuyingOwnCell()
        {
            Client.ExecuteTurn(1);
            Client.BuyCellIfPossible();

            for (int i = 2; Client.GetBoard()[i].GetOwner() != PlayerKey.First ; i = (++i)%Client.GetBoard().Count)
            {
                MonopolyDataPrepare.ExecuteClientTestTurn(ref Client, i);
            }
            
            Assert.IsTrue(Client.ExecuteTurn(1) == MonopolyTurnResult.CannotBuyCell);
        }

        [TestMethod]
        public void NationMonopolTest()
        {
            int PolandCostsWithoutMonopol = Client.GetBoard()[1].GetCosts().Stay;
            
            for (int i = 1;  ; i++)
            {
                Client.ExecuteTurn(1);
                Client.BuyCellIfPossible();

                if (Client.GetBoard()[i].GetNation() != Client.GetBoard()[i + 1].GetNation())
                    break;
            }
            
            Assert.IsTrue(Client.GetBoard()[1].GetCosts().Stay == PolandCostsWithoutMonopol*Consts.Monopoly.MonopolMultiplayer);
        }

        [TestMethod]
        public void BeachMonopolTest_TwoBeachesOwned()
        {
            List<MonopolyCell> BeachCells = Client.GetBoard().FindAll(p => p.GetBeachName() != Beach.NoBeach);
            int FirstBeachStayCost = BeachCells[0].GetCosts().Stay;

            for (int i = 1; ; i++)
            {
                MonopolyDataPrepare.ExecuteClientTestTurn(ref Client, i);

                if (Client.GetBoard()[i].GetBeachName() != Beach.NoBeach)
                    Client.BuyCellIfPossible();

                if (Client.GetBoard()[i].GetBeachName() == BeachCells[1].GetBeachName())
                    break;
            }
            int ExpectedValue = (int)(FirstBeachStayCost * Consts.Monopoly.BeachesOwnedMultiplayer[2]);
            int ActualValue = Client.GetBoard().FirstOrDefault(
                b => b.GetBeachName() == BeachCells[0].GetBeachName()
            ).GetCosts().Stay;

            Assert.IsTrue(ActualValue == ExpectedValue);
        }

        [TestMethod]
        public void BeachMonopolTest_ThreeBeachesOwned()
        {
            List<MonopolyCell> BeachCells = Client.GetBoard().FindAll(p => p.GetBeachName() != Beach.NoBeach);
            int FirstBeachStayCost = BeachCells[0].GetCosts().Stay;

            for (int i = 1; ; i++)
            {
                MonopolyDataPrepare.ExecuteClientTestTurn(ref Client, i);
                
                if (Client.GetBoard()[i].GetBeachName() != Beach.NoBeach)
                    Client.BuyCellIfPossible();

                if (Client.GetBoard()[i].GetBeachName() == BeachCells[2].GetBeachName())
                    break;
            }
            int ExpectedValue = (int)(FirstBeachStayCost * Consts.Monopoly.BeachesOwnedMultiplayer[3]);
            int ActualValue = Client.GetBoard().FirstOrDefault(
                b => b.GetBeachName() == BeachCells[0].GetBeachName()
            ).GetCosts().Stay;

            Assert.IsTrue(ActualValue == ExpectedValue);
        }

        [TestMethod]
        public void IsAbleToBuyStartCell_ShouldBeNo()
        {
            for (int i = 1; i < Client.GetBoard().Count; i++)
            {
                MonopolyDataPrepare.ExecuteClientTestTurn(ref Client, i);
            }

            Assert.IsTrue(Client.ExecuteTurn(1) == MonopolyTurnResult.CannotBuyCell);
        }

        [TestMethod]
        public void IsntAbleToMoveAfterStepingOnIsland()
        {
            int BoardSize = Client.GetBoard().Count;

            for (int i = 1; i < Client.GetBoard().Count; i++)
            {
                Client.ExecuteTurn(1);
                if (Client.GetBoard()[i].OnDisplay() == Consts.Monopoly.IslandDiaplsy)
                {
                    Client.ModalResponse("Wait");
                    break;
                }
            }
            Client.ExecuteTurn(BoardSize - 2);

            Assert.IsTrue(Client.GetUpdatedData().PlayersData[0].Money == Consts.Monopoly.StartMoneyAmount);
        }

        [TestMethod]
        public void IsAbleToMoveFromIslandAfter3Turns()
        {
            int BoardSize = Client.GetBoard().Count;

            for (int i = 1; i < Client.GetBoard().Count; i++)
            {
                Client.ExecuteTurn(1);
                if (Client.GetBoard()[i] is MonopolyIslandCell)
                {
                    break;
                }
            }

            Client.ExecuteTurn(1);
            Client.ExecuteTurn(1);
            Client.ExecuteTurn(1);

            Client.ExecuteTurn(BoardSize - 2);

            Assert.IsTrue(Client.GetUpdatedData().PlayersData[0].Money == Consts.Monopoly.StartMoneyAmount + Consts.Monopoly.OnStartCrossedMoneyGiven);
        }
    }

}
