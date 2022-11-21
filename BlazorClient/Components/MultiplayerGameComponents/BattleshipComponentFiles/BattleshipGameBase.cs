﻿using Enums;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using Models;
using Services.GamesServices.Battleships;

namespace BlazorClient.Components.MultiplayerGameComponents.BattleshipComponentFiles
{
    public class BattleshipGameBase : ComponentBase
    {
        [Inject]
        public NavigationManager NavManager { get; set; }

        [Inject]
        public BattleshipService BattleshipLogic { get; set; }

        [Parameter]
        public string LoggedUserName { get; set; }

        private HubConnection BattleshipHubConn;

        public string UserMessage { get; private set; }

        public bool IsEnemyFound { get; set; }
        public bool IsYourTurn { get; set; }

        protected override async Task OnInitializedAsync()
        {
            UserMessage = "";
            BattleshipHubConn = new HubConnectionBuilder().WithUrl(NavManager.ToAbsoluteUri($"{Constants.ServerURL}/battleshiphub")).WithAutomaticReconnect().Build();

            BattleshipHubConn.On<bool, bool>("IsEnemyFound", (isEnemyFound, IsYourTurn) =>
            {
                IsEnemyFound = isEnemyFound;
                this.IsYourTurn = IsYourTurn;
                InvokeAsync(StateHasChanged);
            });

            BattleshipHubConn.On<Point2D>("EnemyAttack", (OnPoint) =>
            {
                BattleshipLogic.EnemyAttack(OnPoint);
                InvokeAsync(StateHasChanged);
            });

            await BattleshipHubConn.StartAsync();
            await BattleshipHubConn.SendAsync("OnUserConnected", LoggedUserName, BattleshipHubConn.ConnectionId);
        }

        protected bool IsGameStarted() { return IsEnemyFound == true; }


        protected async Task FindEnemy()
        {
            if (BattleshipLogic.IsUserBoardCorrect())
            {
                await BattleshipHubConn.SendAsync("FindEnemyForUser", LoggedUserName);
                UserMessage = "";
            }
            else
                UserMessage = "Ships distribution is not correct";
        }

        protected void UserBoardClicked(Point2D OnPoint)
        {
            BattleshipLogic.UserBoardClicked(OnPoint);
        }

        protected async Task EnemyBoardClicked(Point2D OnPoint)
        {
            await BattleshipHubConn.SendAsync("UserAttack", OnPoint, LoggedUserName);
        }
    }
}
