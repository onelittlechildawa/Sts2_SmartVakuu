using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Nodes.Screens.CardSelection;
using SmartVakuu.Cards;

namespace SmartVakuu;

[HarmonyPatch(typeof(WhisperingEarring))]
public static class SmartVakuuPatch
{
    private const int MaxCardsToPlay = 13;

    [HarmonyPrefix]
    [HarmonyPatch(nameof(WhisperingEarring.BeforePlayPhaseStart))]
    public static bool BeforePlayPhaseStart_Prefix(
        WhisperingEarring __instance,
        PlayerChoiceContext choiceContext,
        Player player,
        ref Task __result)
    {
        if (player != __instance.Owner)
            return true;

        CombatState? combatState = player.Creature.CombatState;
        if (combatState == null || combatState.RoundNumber > 1)
            return true;

        __result = SmartBeforePlayPhaseStart(__instance, choiceContext, player);
        return false;
    }

    private static async Task SmartBeforePlayPhaseStart(
        WhisperingEarring relic,
        PlayerChoiceContext choiceContext,
        Player player)
    {
        CombatState? combatState = player.Creature.CombatState;
        if (combatState == null)
            return;

        // 1. Show strategy selection
        Strategy strategy = await ShowStrategyChoice(player);

        // 2. Flash relic
        relic.Flash();

        // 3. Play cards by strategy
        using (CardSelectCmd.PushSelector(new VakuuCardSelector()))
        {
            int cardsPlayed = 0;
            for (; cardsPlayed < MaxCardsToPlay; cardsPlayed++)
            {
                if (CombatManager.Instance.IsOverOrEnding)
                    break;

                CardPile hand = PileType.Hand.GetPile(player);
                CardModel? bestCard = hand.Cards
                    .Where(c => c.CanPlay())
                    .OrderByDescending(c => SmartCardScorer.Score(c, strategy))
                    .FirstOrDefault();

                if (bestCard == null)
                    break;

                Creature? target = SmartCardScorer.GetTarget(bestCard, strategy, combatState);

                await bestCard.SpendResources();
                await CardCmd.AutoPlay(choiceContext, bestCard, target,
                    AutoPlayType.Default, skipXCapture: true);
            }

            if (cardsPlayed == 0)
                return;
        }
    }

    /// <summary>
    /// Provide our custom strategy cards for UI selection.
    /// </summary>
    private static async Task<Strategy> ShowStrategyChoice(Player player)
    {
        // Use our custom cards, which are now registered to card pools
        var attackCard  = ModelDb.Card<StrategyAttack>().ToMutable();
        var defenseCard = ModelDb.Card<StrategyDefense>().ToMutable();
        var powerCard   = ModelDb.Card<StrategyPower>().ToMutable();
        var randomCard  = ModelDb.Card<StrategyRandom>().ToMutable();

        // Assign owner for proper rendering
        attackCard.Owner = player;
        defenseCard.Owner = player;
        powerCard.Owner = player;
        randomCard.Owner = player;

        var cards = new List<CardModel> { attackCard, defenseCard, powerCard, randomCard };

        var screen = NChooseACardSelectionScreen.ShowScreen(cards, canSkip: false);

        if (screen == null)
            return Strategy.Attack;

        var selected = (await screen.CardsSelected()).FirstOrDefault();

        // Map by card type to determine strategy
        if (selected == attackCard) return Strategy.Attack;
        if (selected == defenseCard) return Strategy.Defense;
        if (selected == powerCard) return Strategy.PowerUp;
        if (selected == randomCard) return Strategy.Random;

        return Strategy.Attack;
    }
}
