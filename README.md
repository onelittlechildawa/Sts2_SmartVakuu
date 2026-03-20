# SmartVakuu (瓦库的低语 - 智能优化 Mod)

For *Slay the Spire 2* (杀戮尖塔2).

This mod completely overhauls the **Whispering Earring** (瓦库的低语) boss relic. Instead of playing your cards completely randomly and ruining your run, the Earring now allows you to choose an overarching strategy at the start of combat, and employs a smart scoring AI to play cards optimally based on your choice!

## Features
At the start of turn 1, you will be prompted to choose one of four distinct strategies:

- 🗡️ **All-out Attack (全力攻击)**: The AI fiercely prioritizes Attack cards and damage. It will automatically target the enemy with the lowest Health to try and score a kill.
- 🛡️ **Steady Defense (稳健防御)**: The AI prioritizes Block cards and self-preservation. It is smart enough to target the enemy currently declaring the highest incoming damage against you.
- ✨ **Power Build (蓄力能力)**: The AI focuses on accelerating your scaling by heavily prioritizing Power cards, followed by energy generation and card draw.
- 🎲 **Pure Chaos (纯粹混沌)**: Miss the old random Earring? Choose this to play cards completely randomly. **Note:** Unlike the vanilla game's seeded RNG, this strategy uses an unseeded RNG engine. Every time you Save & Quit and reload, the cards will be played in a brand new, wildly different organic order!

*Energy and draw cards are always evaluated highly across all strategies.* The AI mathematically factors in the true `EnergyCost` of each card (including properly assessing `X`-cost cards) to ensure it plays the highest *value-per-energy* cards first.

## Installation
1. Go to the [Releases](../../releases) tab and download the latest `SmartVakuu_Release.zip`.
2. Extract the contents into your Slay the Spire 2 `mods/` directory. The path should look like: `.../Slay the Spire 2/mods/SmartVakuu/`
3. Ensure the folder contains `SmartVakuu.json`, `SmartVakuu.dll`, and `SmartVakuu.pck`.
4. Launch the game and ensure Mods are enabled.

## Development & Usage
This mod adds 4 custom UI proxy cards into the standard character CardPools to allow for strategy selection without polluting your deck. It overrides their `CanBeGeneratedInCombat` properties to guarantee they will never drop or be generated during normal gameplay.
