﻿using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Discord.Addons.MpGame
{
    /// <summary>
    /// Service managing games of type <see cref="MpGameModuleBase{TService, TGame, TPlayer}"/>.
    /// </summary>
    /// <typeparam name="TGame">The type of game to manage.</typeparam>
    /// <typeparam name="TPlayer">The type of the <see cref="Player"/> object.</typeparam>
    public class MpGameService<TGame, TPlayer>
        where TGame : GameBase<TPlayer>
        where TPlayer : Player
    {
        /// <summary>
        /// A cached <see cref="IEqualityComparer{IUser}"/> instance to use when
        /// instantiating the <see cref="PlayerList"/>'s <see cref="HashSet{IUser}"/>.
        /// </summary>
        private static readonly IEqualityComparer<IUser> UserComparer = new EntityEqualityComparer<ulong>();

        private readonly ConcurrentDictionary<ulong, TGame> _gameList
            = new ConcurrentDictionary<ulong, TGame>();

        private readonly ConcurrentDictionary<ulong, HashSet<IUser>> _playerList
            = new ConcurrentDictionary<ulong, HashSet<IUser>>();

        private readonly ConcurrentDictionary<ulong, bool> _openToJoin
            = new ConcurrentDictionary<ulong, bool>();

        /// <summary>
        /// The instance of a game being played, keyed by channel ID.
        /// </summary>
        internal IReadOnlyDictionary<ulong, TGame> GameList => _gameList;

        /// <summary>
        /// The list of users scheduled to join game, keyed by channel ID.
        /// </summary>
        internal IReadOnlyDictionary<ulong, HashSet<IUser>> PlayerList => _playerList;


        /// <summary>
        /// Indicates whether the users can join a game about to start, keyed by channel ID.
        /// </summary>
        internal IReadOnlyDictionary<ulong, bool> OpenToJoin => _openToJoin;


        /// <summary>
        /// Add a new game to the list of active games.
        /// </summary>
        /// <param name="channelId">Public facing channel of this game.</param>
        /// <param name="game">Instance of the game.</param>
        /// <returns>true if the operation succeeded, otherwise false.</returns>
        public bool TryAddNewGame(ulong channelId, TGame game)
        {
            var success = _gameList.TryAdd(channelId, game);
            if (success)
                game.GameEnd += _onGameEnd;

            return success;
        }

        private Task _onGameEnd(ulong channelId)
        {
            TGame game;
            HashSet<IUser> users;
            if (_gameList.TryRemove(channelId, out game))
            {
                _playerList.TryRemove(channelId, out users);
                game.GameEnd -= _onGameEnd;
            }
            return Task.CompletedTask;
        }

        /// <summary>
        /// Sets a new Player List for the specified channel.
        /// </summary>
        /// <param name="channelId">The Channel ID.</param>
        public void MakeNewPlayerList(ulong channelId)
            => _playerList[channelId] = new HashSet<IUser>(UserComparer);

        /// <summary>
        /// Updates the flag indicating if a game can be joined or not.
        /// </summary>
        /// <param name="channelId">The Channel ID.</param>
        /// <param name="newValue">The new value.</param>
        /// <param name="comparisonValue">The value that should be compared against.</param>
        /// <returns>true if the value was updated, otherwise false.</returns>
        public bool TryUpdateOpenToJoin(ulong channelId, bool newValue, bool comparisonValue)
            => _openToJoin.TryUpdate(channelId, newValue, comparisonValue);
    }
}