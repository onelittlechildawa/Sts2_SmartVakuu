using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace SmartVakuu.Cards;

/// <summary>
/// 全力攻击策略卡 - 仅用于策略选择界面展示
/// Attack-type card frame so it looks like an attack card
/// </summary>
public sealed class StrategyAttack : CardModel
{
    public StrategyAttack()
        : base(0, CardType.Attack, CardRarity.Token, TargetType.Self, shouldShowInCardLibrary: false)
    {
    }

    public override bool CanBeGeneratedInCombat => false;
    public override bool CanBeGeneratedByModifiers => false;

    protected override Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // Never actually played — display-only for strategy selection screen
        return Task.CompletedTask;
    }
}
