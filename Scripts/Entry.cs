using HarmonyLib;
using MegaCrit.Sts2.Core.Modding;
using MegaCrit.Sts2.Core.Models.CardPools;
using SmartVakuu.Cards;

namespace SmartVakuu;

[ModInitializer("Init")]
public class Entry
{
    public static void Init()
    {
        // Add custom cards to specific card pools to give them card frames and energy icons.
        // Ironclad = Red frame, Silent = Green frame, Defect = Blue frame.
        ModHelper.AddModelToPool<IroncladCardPool, StrategyAttack>();
        ModHelper.AddModelToPool<SilentCardPool, StrategyDefense>();
        ModHelper.AddModelToPool<DefectCardPool, StrategyPower>();
        ModHelper.AddModelToPool<ColorlessCardPool, StrategyRandom>();

        var harmony = new Harmony("SmartVakuu");
        harmony.PatchAll();
    }
}
