﻿#region

using Dalamud.Game.ClientState.Objects.SubKinds;
using ECommons.Logging;
using Functions = XIVSlothCombo.CustomComboNS.Functions.CustomComboFunctions;
using Options = XIVSlothCombo.Combos.CustomComboPreset;

// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable CheckNamespace

#endregion

namespace XIVSlothCombo.Combos.PvE;

internal partial class DRK
{
    /// <summary>
    ///     Shorter reference to the local player.
    /// </summary>
    /// <seealso cref="Functions.LocalPlayer"/>
    private static readonly IPlayerCharacter? LocalPlayer = Functions.LocalPlayer;

    /// <summary>
    ///     Whether the player has a shield from TBN from themselves.
    /// </summary>
    /// <seealso cref="Buffs.BlackestNightShield"/>
    private static bool HasOwnTBN
    {
        get
        {
            var has = false;
            if (LocalPlayer is not null)
                has = Functions.FindEffect(
                    Buffs.BlackestNightShield,
                    LocalPlayer,
                    LocalPlayer.GameObjectId
                ) is not null;

            return has;
        }
    }

    /// <summary>
    ///     Whether the player has a shield from TBN from anyone.
    /// </summary>
    /// <seealso cref="Buffs.BlackestNightShield"/>
    private static bool HasAnyTBN
    {
        get
        {
            var has = false;
            if (LocalPlayer is not null)
                has = Functions.FindEffect(Buffs.BlackestNightShield) is not null;

            return has;
        }
    }

    /// <summary>
    ///     Decides if the player should use TBN on themselves,
    ///     based on general rules and the player's configuration.
    /// </summary>
    /// <param name="aoe">Whether AoE or ST options should be checked.</param>
    /// <returns>Whether TBN should be used on self.</returns>
    /// <seealso cref="BlackestNight"/>
    /// <seealso cref="Buffs.BlackestNightShield"/>
    /// <seealso cref="CustomComboPreset.DRK_ST_TBN" />
    /// <seealso cref="Config.DRK_ST_TBNThreshold" />
    /// <seealso cref="Config.DRK_ST_TBNBossRestriction" />
    /// <seealso cref="CustomComboPreset.DRK_AoE_TBN" />
    private static bool ShouldTBNSelf(bool aoe = false)
    {
        // Bail if TBN is disabled
        if ((!aoe
             && (!Functions.IsEnabled(Options.DRK_ST_Mitigation)
                 || !Functions.IsEnabled(Options.DRK_ST_TBN)))
            || (aoe
                && (!Functions.IsEnabled(Options.DRK_AoE_Mitigation)
                    || !Functions.IsEnabled(Options.DRK_AoE_TBN))))
            return false;

        // Bail if we already have TBN
        if (HasOwnTBN)
            return false;

        // Bail if we're dead or unloaded
        if (LocalPlayer is null)
            return false;

        // Bail if we have no target
        if (LocalPlayer.TargetObject is null)
            return false;

        var hpRemaining = Functions.PlayerHealthPercentageHp();
        var hpThreshold = !aoe ? (float)Config.DRK_ST_TBNThreshold : 0f;

        // Bail if we're above the threshold
        if (hpRemaining > hpThreshold)
            return false;

        var targetIsBoss = Functions.IsBoss(LocalPlayer.TargetObject);
        var bossRestriction =
            !aoe
                ? (int)Config.DRK_ST_TBNBossRestriction
                : (int)Config.BossAvoidance.Off; // Don't avoid bosses in AoE

        // Bail if we're trying to avoid bosses and the target is one
        if (bossRestriction is (int)Config.BossAvoidance.On
            && targetIsBoss)
            return false;

        // Bail if we already have a TBN and burst is >30s away ()
        if (Functions.GetCooldownRemainingTime(LivingShadow) > 30
            && HasAnyTBN)
            return false;

        return true;
    }
}
