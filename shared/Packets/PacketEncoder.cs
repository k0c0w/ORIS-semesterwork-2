using Shared.GameActions;
using Shared.Decks;
using Shared.PossibleCards;
using Microsoft.CSharp.RuntimeBinder;

namespace Shared.Packets;

public class PacketEncoder
{
    public static Packet EncodeGameAction(GameAction gameAction)
    {
        try
        {
            var packet = new Packet();
            packet.Write((int)PacketId.GameActionPacket);
            return Encode((dynamic)gameAction, packet);
        }
        catch(RuntimeBinderException)
        {
            Console.WriteLine($"Не удалось закодировать пакет: {gameAction.GetType().Name}");
            throw;
        }
    }

    private static Packet Encode(UserDecisionStart request, Packet packet)
    {
        packet.Write((int)GameActionPacket.UserDecisionStart);
        packet.Write(request.UserId);
        return packet;
    }
    
    private static Packet Encode(BadRequest request, Packet packet)
    {
        packet.Write((int)GameActionPacket.BadRequest);
        return packet;
    }
    
    private static Packet Encode(GameStart request, Packet packet)
    {
        packet.Write((int)GameActionPacket.GameStart);
        var info = request.FirstPlayerInfo;
        packet.Write(info.Item1);
        packet.Write(info.Item2);

        info = request.SecondPlayerInfo;
        packet.Write(info.Item1);
        packet.Write(info.Item2);
        
        return packet;
    }

    private static Packet Encode(PossibleDecks request, Packet packet)
    {
        packet.Write((int)GameActionPacket.PossibleDecks);
        packet.Write((int)request.First);
        packet.Write((int)request.Second);
        return packet;
    }

    private static Packet Encode(UserChoseDeck request, Packet packet)
    {
        packet.Write((int)GameActionPacket.UserChoseDeck);
        packet.Write(request.UserId);
        packet.Write((int)request.DeckType);
        return packet;
    }

    private static Packet Encode(UserPutCard request, Packet packet)
    {
        packet.Write((int)GameActionPacket.UserPutCard);
        packet.Write(request.UserId);
        packet.Write(request.Line);
        packet.Write((int)request.Card);
        packet.Write(request.EnergyLeft);
        return packet;
    }

    private static Packet Encode(UserDecisionEnd request, Packet packet)
    {
        packet.Write((int)GameActionPacket.UserDecisionEnd);
        packet.Write(request.UserId);
        return packet;
    }
    
    private static Packet Encode(UserTakeLands take, Packet packet)
    {
        packet.Write((int)GameActionPacket.UserTakeLands);
        packet.Write(take.UserId);
        var lands = take.Lands;
        for(var i = 0; i < 4; i++)
            packet.Write((int)lands[i]);
        return packet;
    }

    private static Packet Encode(UserTakeDamage damage, Packet packet)
    {
        packet.Write((int)GameActionPacket.UserTakeDamage);
        packet.Write(damage.UserId);
        packet.Write(damage.Damage);
        return packet;
    }

    private static Packet Encode(CreatureState state, Packet packet)
    {
        packet.Write((int)GameActionPacket.CreatureState);
        packet.Write(state.UserId);
        packet.Write(state.Owner);
        packet.Write(state.Line);
        packet.Write(state.HPBefore);
        packet.Write(state.HPAfter);
        packet.Write(state.IsDead);
        packet.Write(state.IsFlooped);
        return packet;
    }

    private static Packet Encode(UserTakeCards request, Packet packet)
    {
        packet.Write((int)GameActionPacket.UserTakeCards);
        packet.Write(request.UserId);
        packet.Write(request.TakenCards);
        foreach (var card in request.Cards)
            packet.Write((int)card);
        packet.Write(request.CardsInDeckLeft);
        return packet;
    }

    private static Packet Encode(Winner winner, Packet packet)
    {
        packet.Write((int)GameActionPacket.Winner);
        packet.Write(winner.UserId);
        return packet;
    }
    

    public static GameAction DecodeGameAction(Packet packet)
    {
        /*var packetType = (PacketId)packet.ReadInt();
        if (packetType != PacketId.GameActionPacket)
            throw new InvalidOperationException($"Incorrect packet type {packetType}.");*/
        var action = (GameActionPacket)packet.ReadInt();
        return action switch
        {
            GameActionPacket.BadRequest => BadRequest,
            GameActionPacket.GameStart => DecodeGameStart(packet),
            GameActionPacket.PossibleDecks => DecodePossibleDecks(packet),
            GameActionPacket.UserChoseDeck => DecodeUserChoseDeck(packet),
            GameActionPacket.UserPutCard => DecodeUserPutCard(packet),
            GameActionPacket.UserDecisionStart => DecodeUserDecisionStart(packet),
            GameActionPacket.UserDecisionEnd => DecodeUserDecisionEnd(packet),
            GameActionPacket.UserTakeLands => DecodeUserTakeLands(packet),
            GameActionPacket.UserTakeCards => DecodeUserTakeCards(packet),
            GameActionPacket.UserTakeDamage => DecodeUserTakeDamage(packet),
            GameActionPacket.CreatureState => DecodeCreatureState(packet),
            GameActionPacket.Winner => DecodeWinner(packet),
            _ => throw new InvalidOperationException()
        };
    }

    private static Winner DecodeWinner(Packet packet)
    {
        var user = packet.ReadInt();
        return new Winner(user);
    }
    
    private static UserTakeCards DecodeUserTakeCards(Packet packet)
    {
        var user = packet.ReadInt();
        var length = packet.ReadInt();
        var cards = new AllCards[length];
        for (var i = 0; i < length; i++)
            cards[i] = (AllCards)packet.ReadInt();
        
        return new UserTakeCards(user, cards, packet.ReadInt());
    }
    
    private static UserTakeDamage DecodeUserTakeDamage(Packet packet)
    {
        var user = packet.ReadInt();
        var damage = packet.ReadInt();
        return new UserTakeDamage(user, damage);
    }

    private static CreatureState DecodeCreatureState(Packet packet)
    {
        //must be -1 only
        var userId = packet.ReadInt();
        return new CreatureState(packet.ReadInt(), packet.ReadInt())
        {
            HPBefore = packet.ReadInt(),
            HPAfter = packet.ReadInt(),
            IsDead = packet.ReadBool(),
            IsFlooped = packet.ReadBool(),
            UserId = -1,
        };
    }
    
    private static PossibleDecks DecodePossibleDecks(Packet packet)
    {
        var first = (DeckTypes)packet.ReadInt();
        var second = (DeckTypes)packet.ReadInt();
        return new PossibleDecks { First = first, Second = second, UserId = -1};
    }

    private static UserChoseDeck DecodeUserChoseDeck(Packet packet)
    {
        var user = packet.ReadInt();
        var deck = (DeckTypes)packet.ReadInt();
        return new UserChoseDeck(user, deck);
    }
    
    private static BadRequest BadRequest { get; } = new BadRequest();

    private static GameStart DecodeGameStart(Packet packet)
    {
        var firstId = packet.ReadInt();
        var username = packet.ReadString();
        var firstInfo = (firstId, username);

        
        var secondId = packet.ReadInt();
        username = packet.ReadString();
        var secondInfo = (secondId, username);

        return new GameStart { FirstPlayerInfo = firstInfo, SecondPlayerInfo = secondInfo };
    }
    
    private static UserPutCard DecodeUserPutCard(Packet packet)
    {
        var client = packet.ReadInt();
        var line = packet.ReadInt();
        var card = (AllCards)packet.ReadInt();
        return new UserPutCard(client, line, card) { EnergyLeft = packet.ReadInt() };
    }

    private static UserDecisionStart DecodeUserDecisionStart(Packet packet) =>
        new UserDecisionStart { UserId = packet.ReadInt() };

    private static UserDecisionEnd DecodeUserDecisionEnd(Packet packet) =>
        new UserDecisionEnd { UserId = packet.ReadInt() };
    

    private static UserTakeLands DecodeUserTakeLands(Packet packet)
    {
        var user = packet.ReadInt();
        var lands = new LandType[4];
        for (var i = 0; i < 4; i++)
            lands[i] = (LandType)packet.ReadInt();

        return new UserTakeLands(user, lands);
    }
}