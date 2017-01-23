﻿using System.Linq;
using System.Threading.Tasks;
using Discord.Addons.MpGame;
using Discord.Commands;

namespace Examples.MpGame
{
    public sealed class ExampleModule : MpGameModuleBase // Inherit MpGameModuleBase
        <ExampleService, // Specify the type of the service that will keep track of running games
        ExampleGame, Player> // Specify the type of the game and the type of its player
    {
        public ExampleModule(ExampleService gameService)
            : base(gameService)
        {
        }

        // You may have reasons to not annotate a particular method with [Command],
        // and you'll likely have to add MORE commands depending on the game
        [Command("opengame")]
        public override async Task OpenGameCmd()
        {
            if (GameInProgress)
            {
                await ReplyAsync("Another game already in progress.");
            }
            else if (OpenToJoin)
            {
                await ReplyAsync("There is already a game open to join.");
            }
            else
            {
                if (GameService.TryUpdateOpenToJoin(Context.Channel.Id, newValue: true, comparisonValue: false))
                {
                    if (GameService.MakeNewPlayerList(Context.Channel.Id))
                        await ReplyAsync("Opening for a game.");
                }
            }
        }

        // Note that we should always check if the channel already has a game going
        // or wants people to join before taking action
        [Command("join")]
        public override async Task JoinGameCmd()
        {
            if (GameInProgress)
            {
                await ReplyAsync("Cannot join a game already in progress.");
            }
            else if (!OpenToJoin)
            {
                await ReplyAsync("No game open to join.");
            }
            else
            {
                if (GameService.AddUser(Context.Channel.Id, Context.User))
                {
                    await ReplyAsync($"**{Context.User.Username}** has joined.");
                }
            }
        }

        [Command("leave")] // Users can leave if the game hasn't started yet
        public override async Task LeaveGameCmd()
        {
            if (GameInProgress)
            {
                await ReplyAsync("Cannot leave a game already in progress.");
            }
            else if (!OpenToJoin)
            {
                await ReplyAsync("No game open to leave.");
            }
            else
            {
                if (GameService.RemoveUser(Context.Channel.Id, Context.User))
                {
                    await ReplyAsync($"**{Context.User.Username}** has left.");
                }
            }
        }

        [Command("cancel")] // Cancel the game if it hasn't started yet
        public override async Task CancelGameCmd()
        {
            if (GameInProgress)
            {
                await ReplyAsync("Cannot cancel a game already in progress.");
            }
            else if (!OpenToJoin)
            {
                await ReplyAsync("No game open to cancel.");
            }
            else
            {
                if (GameService.CancelGame(Context.Channel.Id))
                {
                    await ReplyAsync("Game was canceled.");
                }
            }
        }

        [Command("start")] // Start the game
        public override async Task StartGameCmd()
        {
            if (GameInProgress)
            {
                await ReplyAsync("Another game already in progress.");
            }
            else if (!OpenToJoin)
            {
                await ReplyAsync("No game has been opened at this time.");
            }
            else if (PlayerList.Count < 4) // Example value if a game has a minimum player requirement
            {
                await ReplyAsync("Not enough players have joined.");
            }
            else
            {
                if (GameService.TryUpdateOpenToJoin(Context.Channel.Id, newValue: false, comparisonValue: true))
                {
                    // Tip: Shuffle the players before projecting them
                    var players = PlayerList.Select(u => new Player(u, Context.Channel));
                    // The Player class can also be extended for additional properties

                    var game = new ExampleGame(Context.Channel, players);
                    if (GameService.TryAddNewGame(Context.Channel.Id, game))
                    {
                        await game.SetupGame();
                        await game.StartGame();
                    }
                }
            }
        }

        [Command("turn")] // Advance to the next turn
        public override Task NextTurnCmd()
            => GameInProgress ? Game.NextTurn() : ReplyAsync("No game in progress.");

        // Post a message that represents the game's state
        [Command("state")] //Remember there's a 2000 character limit
        public override Task GameStateCmd()
           => GameInProgress ? ReplyAsync(Game.GetGameState()) : ReplyAsync("No game in progress.");

        // Command to end a game before a win-condition is met
        [Command("end")] //Should be restricted to mods/admins to prevent abuse
        public override Task EndGameCmd()
            => !GameInProgress ? ReplyAsync("No game in progress to end.") : Game.EndGame("Game ended early by moderator.");
    }
}
