﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.Rest;

namespace Discord.Addons.SimplePermissions
{
    internal sealed class FancyHelpMessage
    {
        internal const string SFirst  = "\u23EE";
        internal const string SBack   = "\u25C0";
        internal const string SNext   = "\u25B6";
        internal const string SLast   = "\u23ED";
        internal const string SDelete = "\u274C";

        private static IEmote EFirst  { get; } = new Emoji(SFirst);
        private static IEmote EBack   { get; } = new Emoji(SBack);
        private static IEmote ENext   { get; } = new Emoji(SNext);
        private static IEmote ELast   { get; } = new Emoji(SLast);
        private static IEmote EDelete { get; } = new Emoji(SDelete);
        private static readonly IEmote[] _emotes = new[] { EFirst, EBack, ENext, ELast, EDelete };

        private readonly IUser _user;
        private readonly IMessageChannel _channel;
        private readonly IEnumerable<CommandInfo> _commands;
        private readonly int _cmdsPerPage = 5;
        private readonly uint _lastPage;
        private readonly IApplication _app;

        internal ulong UserId => _user.Id;
        internal ulong MsgId => _msg?.Id ?? 0UL;
        private IUserMessage? _msg;
        private uint _currentPage;

        public FancyHelpMessage(IMessageChannel channel, IUser user, IEnumerable<CommandInfo> commands, IApplication app)
        {
            _user = user;
            _channel = channel;
            _commands = commands;
            _currentPage = 0;
            _lastPage = (uint)Math.Ceiling((commands.Count() / (double)_cmdsPerPage)) - 1;
            _app = app;
        }

        public async Task<FancyHelpMessage> SendMessage()
        {
            _msg = await _channel.SendMessageAsync(String.Empty, embed: GetPage(0)).ConfigureAwait(false);
            await _msg.AddReactionsAsync(_emotes).ConfigureAwait(false);
            //await _msg.AddReactionAsync(EFirst).ConfigureAwait(false);
            //await _msg.AddReactionAsync(EBack).ConfigureAwait(false);
            //await _msg.AddReactionAsync(ENext).ConfigureAwait(false);
            //await _msg.AddReactionAsync(ELast).ConfigureAwait(false);
            //await _msg.AddReactionAsync(EDelete).ConfigureAwait(false);

            return this;
        }

        private Embed GetPage(int page)
        {
            var c = _commands.Skip(page * _cmdsPerPage).Take(_cmdsPerPage);
            //var m = c.First().Module.Name;
            return new EmbedBuilder()
            {
                Author = new EmbedAuthorBuilder
                {
                    Name = _app.Name,
                    IconUrl = _app.IconUrl
                },
                Title = "Available commands.",
                Description = $"Page {page + 1} of {_lastPage + 1}",
                Footer = new EmbedFooterBuilder
                {
                    Text = "Powered by SimplePermissions"
                }
            }.AddFieldSequence(c, (fb, cmd) => fb.WithIsInline(false)
                .WithName($"{cmd.Module.Name}: {cmd.Aliases.FirstOrDefault()}")
                .WithValue($"{cmd.Summary}\n\t{String.Join(", ", cmd.Parameters.Select(p => p.FormatParam()))}"))
            .Build();
        }

        public async Task First()
        {
            await _msg!.RemoveReactionAsync(new Emoji(SFirst), _user);
            if (_currentPage == 0) return;

            await _msg.ModifyAsync(m => m.Embed = GetPage((int)(_currentPage = 0)));

        }

        public async Task Next()
        {
            await _msg!.RemoveReactionAsync(new Emoji(SNext), _user);
            if (_currentPage == _lastPage) return;

            await _msg.ModifyAsync(m => m.Embed = GetPage((int)++_currentPage));
        }

        public async Task Back()
        {
            await _msg!.RemoveReactionAsync(new Emoji(SBack), _user);
            if (_currentPage == 0) return;

            await _msg.ModifyAsync(m => m.Embed = GetPage((int)--_currentPage));
        }

        public async Task Last()
        {
            await _msg!.RemoveReactionAsync(new Emoji(SLast), _user);
            if (_currentPage == _lastPage) return;

            await _msg.ModifyAsync(m => m.Embed = GetPage((int)(_currentPage = _lastPage - 1)));
        }

        public Task Delete()
        {
            return _msg!.DeleteAsync();
        }
    }
}
