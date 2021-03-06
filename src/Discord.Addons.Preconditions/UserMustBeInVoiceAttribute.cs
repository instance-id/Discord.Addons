﻿using System;
using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;

namespace Discord.Addons.Preconditions
{
    /// <summary>
    ///     Indicates that this command can only be used while
    ///     the user is in a voice channel in the same Guild.
    ///     This precondition automatically applies <see cref="RequireContextAttribute"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public sealed class UserMustBeInVoiceAttribute : RequireContextAttribute
    {
        /// <summary>
        /// </summary>
        public UserMustBeInVoiceAttribute()
            : base(ContextType.Guild)
        {
        }

        /// <inheritdoc />
        public override async Task<PreconditionResult> CheckPermissionsAsync(
            ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            var baseResult = await base.CheckPermissionsAsync(context, command, services);
            if (!baseResult.IsSuccess)
                return baseResult;

            var current = (context.User as IVoiceState)?.VoiceChannel?.Id;
            return (await context.Guild.GetVoiceChannelsAsync()).Any(v => v.Id == current)
                ? PreconditionResult.FromSuccess()
                : PreconditionResult.FromError("Command must be invoked while in a voice channel in this guild.");
        }
    }
}
