using Shared.GameActions;
using Shared.PossibleCards;

namespace GameKernel.GameStates;

public class UserChooseDeck : IGameState
{
    public UserChooseDeck(Game game) => CurrentGame = game;
    
    public Game CurrentGame { get; }
    public bool IsValidAction(GameAction action)
    {
        if (action is not UserChoseDeck choose) return false;
        var allowed = CurrentGame.AllowedDecks;

        return CurrentGame.Players.ContainsKey(choose.UserId)
               && (allowed.Item1 == choose.DeckType || allowed.Item2 == choose.DeckType)
               && CurrentGame._landsAndDecksHolder.ContainsDeck(choose.DeckType);
    }

    
    public void Execute(GameAction action)
    {
        var choose = (UserChoseDeck)action;
        var dnl = CurrentGame._landsAndDecksHolder.GrabDeck(choose.DeckType);
        var player = CurrentGame.Players[choose.UserId];
        player.Deck = dnl.Item1;
        player.Lands = dnl.Item2;
        CurrentGame.PlayersDeck[choose.UserId] = player.Deck;
        
        CurrentGame.RegisterAction(new UserChoseDeck(choose.UserId, player.Deck.DeckType));
        TakeCards(choose);

        if (CurrentGame.Players.Values.All(x => x.Deck != null))
            ChangeState();
    }

    private AllCards[] TakeFiveCardsFromDeck(int userId)
    {
        var deck = CurrentGame.PlayersDeck[userId];
        return Enumerable.Range(0, 5).Select(x => deck.GetCard()).ToArray();
    }

    private void TakeCards(UserChoseDeck choose)
    {
        var player = CurrentGame.Players[choose.UserId];
        player.Deck.Shuffle();
        var cards = TakeFiveCardsFromDeck(choose.UserId);
        foreach (var card in cards)
            player.TakeCard(card);
        var lands = CurrentGame.Players[choose.UserId].Lands.Select(x => x.LandType).ToArray();
        CurrentGame.RegisterAction(new UserTakeLands(choose.UserId, lands));
        CurrentGame.RegisterAction(new UserTakeCards(choose.UserId, cards, CurrentGame.PlayersDeck[choose.UserId].CardsLeft));
    }
    
    public void ChangeState()
    {
        CurrentGame.RegisterAction(new UserDecisionStart {UserId = 1});
        CurrentGame._landsAndDecksHolder = null!;
        CurrentGame.GameState = new TakeCardsState(1, CurrentGame, true);
        CurrentGame.GameState.Execute(new GameAction() {UserId = 1});
        CurrentGame.GameState = new AfterGameStartPlayerDecision(CurrentGame);
    }
}