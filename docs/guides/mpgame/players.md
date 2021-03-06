﻿---
uid: Addons.MpGame.Players
title: Players
---
## Players

Though optional, it helps to start thinking about the player type first.

The base `Player` class looks like this:
```cs
public class Player
{
    public Player(IUser user, IMessageChannel channel);

    public IUser User { get; }

    public async Task<IUserMessage> SendMessageAsync(string text, Embed embed = null);

    protected virtual bool ShouldKick(int backstuffedDms);
}
```

There's very little state held in the base class. So if you want to implement, for example a card game,
it would make sense that a player would need a property to contain the cards he or she has in their hand.
In order to do this, create a class that derives from `Player` and add such properties/methods.
```cs
public class CardPlayer : Player
{
    // It would make a lot of sense to keep a property
    // like this private.
    // You'll also have to provide your own 'Card' type for this.
    private Hand<Card> Hand { get; } = new Hand<Card>();

    // You need a constructor to call the base constructor.
    public CardPlayer(IUser user, IMessageChannel channel)
        : base(user, channel)
    {
    }

    // And you'll want a method that adds a card to the player's hand.
    public void AddCard(Card card) => Hand.Add(card);

    // You can specify to kick automatically kick a player
    // if the user has their DMs disabled too many times
    // by overriding 'ShouldKick'. By default, a player
    // will never be auto-kicked.
    protected override bool ShouldKick(int backstuffedDms) => backstuffedDms > 5;
}
```
