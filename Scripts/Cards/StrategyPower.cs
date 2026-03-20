using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace SmartVakuu.Cards;

/// <summary>
/// 蓄力能力策略卡 - 仅用于策略选择界面展示
/// Power-type card frame so it looks like a power card
/// </summary>
public sealed class StrategyPower : CardModel
{
    public StrategyPower()
        : base(0, CardType.Power, CardRarity.Token, TargetType.Self, shouldShowInCardLibrary: false)
    {
    }

    public override bool CanBeGeneratedInCombat => false;
    public override bool CanBeGeneratedByModifiers => false;

    protected override Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        return Task.CompletedTask;
    }
}
