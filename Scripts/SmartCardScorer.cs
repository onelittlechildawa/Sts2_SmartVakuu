using System;
using System.Collections.Generic;
using System.Linq;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;

namespace SmartVakuu;

public enum Strategy
{
    Attack,
    Defense,
    PowerUp,
    Random
}

public static class SmartCardScorer
{
    private static readonly Random unseededRandom = new Random();

    /// <summary>
    /// Score a card based on the chosen strategy.
    /// Higher score = play first.
    /// </summary>
    public static float Score(CardModel card, Strategy strategy)
    {
        if (strategy == Strategy.Random)
            return (float)unseededRandom.NextDouble();

        float damage = GetDynamicVarSum<DamageVar>(card);
        float block = GetDynamicVarSum<BlockVar>(card);
        float draw = GetDynamicVarSum<CardsVar>(card);
        float energy = GetDynamicVarSum<EnergyVar>(card);
        bool isPower = card.Type == CardType.Power;

        // Factor in energy cost. 0-cost cards are treated as 0.5 cost to avoid division by zero 
        // and severely over-value them relative to 1-cost cards.
        float cost = Math.Max(0.5f, card.EnergyCost.GetAmountToSpend());

        // Energy-gain and draw cards are always very valuable
        // because they enable more plays this turn
        float universalBonus = energy * 8f + draw * 4f;

        float score = strategy switch
        {
            Strategy.Attack => (damage * 3f + block * 0.5f) / cost + (isPower ? 5f / cost : 0f) + universalBonus,
            Strategy.Defense => (block * 3f + damage * 0.5f) / cost + (isPower ? 3f / cost : 0f) + universalBonus,
            Strategy.PowerUp => (isPower ? 20f / cost : 0f) + (block * 1f + damage * 0.5f) / cost + universalBonus,
            _ => 0f
        };

        // Small penalty for Unplayable/Status/Curse cards
        if (card.Type == CardType.Status || card.Type == CardType.Curse)
        {
            score -= 100f;
        }

        return score;
    }

    /// <summary>
    /// Choose the best target for a card based on strategy.
    /// </summary>
    public static Creature? GetTarget(CardModel card, Strategy strategy, CombatState combatState)
    {
        var enemies = combatState.HittableEnemies;

        if (!enemies.Any())
            return null;

        if (card.TargetType == TargetType.AnyEnemy)
        {
            return strategy switch
            {
                Strategy.Random => enemies.OrderBy(e => unseededRandom.Next()).FirstOrDefault(),
                Strategy.Attack => enemies.OrderBy(e => e.CurrentHp).FirstOrDefault(),
                Strategy.Defense => GetMostDangerousEnemy(enemies) ?? enemies.FirstOrDefault(),
                Strategy.PowerUp => enemies.FirstOrDefault(),
                _ => enemies.FirstOrDefault()
            };
        }

        return card.TargetType switch
        {
            TargetType.AnyAlly => card.Owner?.Creature,
            TargetType.AnyPlayer => card.Owner?.Creature,
            _ => null
        };
    }

    /// <summary>
    /// Find the enemy with the highest incoming attack damage.
    /// </summary>
    private static Creature? GetMostDangerousEnemy(IEnumerable<Creature> enemies)
    {
        Creature? most = null;
        int maxDamage = 0;

        foreach (var enemy in enemies)
        {
            var move = enemy.Monster?.NextMove;
            if (move == null) continue;

            int totalDamage = 0;
            foreach (var intent in move.Intents)
            {
                if (intent is AttackIntent atk)
                {
                    try
                    {
                        totalDamage += (int)atk.GetTotalDamage(Enumerable.Empty<Creature>(), enemy);
                    }
                    catch
                    {
                        // If damage calculation fails, use a default
                        totalDamage += 10;
                    }
                }
            }

            if (totalDamage > maxDamage)
            {
                maxDamage = totalDamage;
                most = enemy;
            }
        }

        return most;
    }

    /// <summary>
    /// Sum up all DynamicVars of a given type on a card.
    /// </summary>
    private static float GetDynamicVarSum<T>(CardModel card) where T : DynamicVar
    {
        float sum = 0;
        foreach (var kvp in card.DynamicVars)
        {
            if (kvp.Value is T dv)
            {
                try
                {
                    sum += (float)dv.BaseValue;
                }
                catch
                {
                    // Some vars may not have a numeric base value
                }
            }
        }
        return sum;
    }
}
