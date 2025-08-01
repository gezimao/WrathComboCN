﻿using System.Linq;
using WrathCombo.Core;
using WrathCombo.CustomComboNS;
using WrathCombo.Data;
using WrathCombo.Extensions;

namespace WrathCombo.Combos.PvE;

internal partial class PLD : Tank
{
    internal class PLD_ST_BasicCombo : CustomCombo
    {
        protected internal override CustomComboPreset Preset { get; } = CustomComboPreset.PLD_ST_BasicCombo;

        protected override uint Invoke(uint actionID)
        {
            if (actionID is not RageOfHalone)
                return actionID;

            if (ComboTimer > 0)
            {
                if (ComboAction is FastBlade && LevelChecked(RiotBlade))
                    return RiotBlade;

                if (ComboAction is RiotBlade && LevelChecked(RageOfHalone))
                    return OriginalHook(RageOfHalone);
            }

            return FastBlade;
        }
    }

    internal class PLD_ST_SimpleMode : CustomCombo
    {
        protected internal override CustomComboPreset Preset { get; } = CustomComboPreset.PLD_ST_SimpleMode;
        internal static int RoyalAuthorityCount => ActionWatching.CombatActions.Count(x => x == OriginalHook(RageOfHalone));

        protected override uint Invoke(uint actionID)
        {
            if (actionID is not FastBlade)
                return actionID;

            #region Variables

            var canFightOrFlight = OriginalHook(FightOrFlight) is FightOrFlight && ActionReady(FightOrFlight);
            float durationFightOrFlight = GetStatusEffectRemainingTime(Buffs.FightOrFlight);
            float cooldownFightOrFlight = GetCooldownRemainingTime(FightOrFlight);
            float cooldownRequiescat = GetCooldownRemainingTime(Requiescat);
            uint playerMP = LocalPlayer.CurrentMp;
            bool canWeave = CanWeave();
            bool canEarlyWeave = CanWeave(1.5f);
            bool hasRequiescat = HasStatusEffect(Buffs.Requiescat);
            bool hasDivineMight = HasStatusEffect(Buffs.DivineMight);
            bool hasFightOrFlight = HasStatusEffect(Buffs.FightOrFlight);
            bool hasDivineMagicMP = playerMP >= GetResourceCost(HolySpirit);
            bool hasRequiescatMP = playerMP >= GetResourceCost(HolySpirit) * 3.6;
            bool inBurstWindow = JustUsed(FightOrFlight, 30f);
            bool inAtonementStarter = HasStatusEffect(Buffs.AtonementReady);
            bool inAtonementFinisher = HasStatusEffect(Buffs.SepulchreReady);
            bool afterOpener = LevelChecked(BladeOfFaith) && RoyalAuthorityCount > 0;
            bool inAtonementPhase = HasStatusEffect(Buffs.AtonementReady) || HasStatusEffect(Buffs.SupplicationReady) || HasStatusEffect(Buffs.SepulchreReady);
            bool isDivineMightExpiring = GetStatusEffectRemainingTime(Buffs.DivineMight) < 6;
            bool isAtonementExpiring = HasStatusEffect(Buffs.AtonementReady) && GetStatusEffectRemainingTime(Buffs.AtonementReady) < 6 ||
                                       HasStatusEffect(Buffs.SupplicationReady) && GetStatusEffectRemainingTime(Buffs.SupplicationReady) < 6 ||
                                       HasStatusEffect(Buffs.SepulchreReady) && GetStatusEffectRemainingTime(Buffs.SepulchreReady) < 6;
            bool justMitted = JustUsed(OriginalHook(Sheltron)) ||
                              JustUsed(OriginalHook(Sentinel), 4f) ||
                              JustUsed(DivineVeil, 4f) ||
                              JustUsed(Role.Rampart, 4f) ||
                              JustUsed(HallowedGround, 9f);

            #endregion

            // Interrupt
            if (Role.CanInterject())
                return Role.Interject;

            // Stun
            if (!TargetIsBoss()
                && TargetIsCasting()
                && !JustUsed(Role.Interject)
                && !InBossEncounter())
                if (Role.CanLowBlow())
                    return Role.LowBlow;

            // Variant Cure
            if (Variant.CanCure(CustomComboPreset.PLD_Variant_Cure, Config.PLD_VariantCure))
                return Variant.Cure;

            #region Mitigations

            if (Config.PLD_ST_MitsOptions != 1)
            {
                if (InCombat() && //Player is in combat
                    !justMitted) //Player has not used a mitigation ability in the last 4-9 seconds
                {
                    //HallowedGround
                    if (ActionReady(HallowedGround) && //HallowedGround is ready
                        PlayerHealthPercentageHp() < 30) //Player's health is below 30%
                        return HallowedGround;

                    if (IsPlayerTargeted())
                    {
                        //Sentinel / Damnation
                        if (ActionReady(OriginalHook(Sentinel)) && //Sentinel is ready
                            PlayerHealthPercentageHp() < 60) //Player's health is below 60%
                            return OriginalHook(Sentinel);

                        //Rampart
                        if (Role.CanRampart(80)) //Player's health is below 80%
                            return Role.Rampart;

                        //Reprisal
                        if (Role.CanReprisal(90)) //Player's health is below 80%
                            return Role.Reprisal;
                    }

                    //Bulwark
                    if (ActionReady(Bulwark) && //Bulwark is ready
                        PlayerHealthPercentageHp() < 70) //Player's health is below 80%
                        return Bulwark;

                    //Sheltron
                    if (ActionReady(OriginalHook(Sheltron)) && //Sheltron
                        PlayerHealthPercentageHp() < 90) //Player's health is below 95%
                        return OriginalHook(Sheltron);
                }
            }

            #endregion

            if (HasBattleTarget())
            {
                // Weavables
                if (canWeave)
                {
                    if (InMeleeRange())
                    {
                        // Requiescat
                        if (ActionReady(Requiescat) && cooldownFightOrFlight > 50)
                            return OriginalHook(Requiescat);

                        // Fight or Flight
                        if (canFightOrFlight)
                        {
                            if (!LevelChecked(Requiescat))
                            {
                                if (!LevelChecked(RageOfHalone))
                                {
                                    // Level 2-25
                                    if (ComboAction is FastBlade)
                                        return OriginalHook(FightOrFlight);
                                }

                                // Level 26-67
                                else if (ComboAction is RiotBlade)
                                    return OriginalHook(FightOrFlight);
                            }

                            // Level 68+
                            else if (cooldownRequiescat < 0.5f && hasRequiescatMP && canEarlyWeave && (ComboAction is RoyalAuthority || afterOpener))
                                return OriginalHook(FightOrFlight);
                        }

                        // Variant Ultimatum
                        if (Variant.CanUltimatum(CustomComboPreset.PLD_Variant_Ultimatum))
                            return Variant.Ultimatum;

                        // Circle of Scorn / Spirits Within
                        if (cooldownFightOrFlight > 15)
                        {
                            if (ActionReady(CircleOfScorn))
                                return CircleOfScorn;

                            if (ActionReady(SpiritsWithin))
                                return OriginalHook(SpiritsWithin);
                        }
                    }

                    // Variant Spirit Dart
                    if (Variant.CanSpiritDart(CustomComboPreset.PLD_Variant_SpiritDart))
                        return Variant.SpiritDart;

                    // Blade of Honor
                    if (LevelChecked(BladeOfHonor) && OriginalHook(Requiescat) == BladeOfHonor)
                        return OriginalHook(Requiescat);
                }

                // Requiescat Phase
                if (hasDivineMagicMP)
                {
                    // Confiteor & Blades
                    if (HasStatusEffect(Buffs.ConfiteorReady) || LevelChecked(BladeOfFaith) && OriginalHook(Confiteor) != Confiteor)
                        return OriginalHook(Confiteor);

                    // Pre-Blades
                    if (hasRequiescat)
                        return HolySpirit;
                }

                // Goring Blade
                if (HasStatusEffect(Buffs.GoringBladeReady) && InMeleeRange())
                    return GoringBlade;

                // Holy Spirit Prioritization
                if (hasDivineMight && hasDivineMagicMP)
                {
                    // Delay Sepulchre / Prefer Sepulchre
                    if (inAtonementFinisher && (cooldownFightOrFlight < 3 || durationFightOrFlight > 3))
                        return HolySpirit;

                    // Fit in Burst
                    if (!inAtonementFinisher && hasFightOrFlight && durationFightOrFlight < 3)
                        return HolySpirit;
                }

                // Atonement: During Burst / Before Expiring / Spend Starter / Before Refreshing
                if (inAtonementPhase && InMeleeRange() && (inBurstWindow || isAtonementExpiring || inAtonementStarter || ComboAction is RiotBlade))
                    return OriginalHook(Atonement);

                // Holy Spirit: During Burst / Before Expiring / Outside Melee / Before Refreshing
                if (hasDivineMight && hasDivineMagicMP && (inBurstWindow || isDivineMightExpiring || !InMeleeRange() || ComboAction is RiotBlade))
                    return HolySpirit;

                // Out of Range
                if (LevelChecked(ShieldLob) && !InMeleeRange())
                    return ShieldLob;
            }

            // Basic Combo
            if (ComboTimer > 0)
            {
                if (ComboAction is FastBlade && LevelChecked(RiotBlade))
                    return RiotBlade;

                if (ComboAction is RiotBlade && LevelChecked(RageOfHalone))
                    return OriginalHook(RageOfHalone);
            }

            return actionID;
        }
    }

    internal class PLD_AoE_SimpleMode : CustomCombo
    {
        protected internal override CustomComboPreset Preset { get; } = CustomComboPreset.PLD_AoE_SimpleMode;

        protected override uint Invoke(uint actionID)
        {
            if (actionID is not TotalEclipse)
                return actionID;

            #region Variables

            var canFightOrFlight = OriginalHook(FightOrFlight) is FightOrFlight && ActionReady(FightOrFlight);
            float cooldownFightOrFlight = GetCooldownRemainingTime(FightOrFlight);
            float cooldownRequiescat = GetCooldownRemainingTime(Requiescat);
            uint playerMP = LocalPlayer.CurrentMp;
            bool canWeave = CanWeave();
            bool canEarlyWeave = CanWeave(1.5f);
            bool hasRequiescat = HasStatusEffect(Buffs.Requiescat);
            bool hasDivineMight = HasStatusEffect(Buffs.DivineMight);
            bool hasDivineMagicMP = playerMP >= GetResourceCost(HolySpirit);
            bool hasRequiescatMP = playerMP >= GetResourceCost(HolySpirit) * 3.6;
            bool justMitted = JustUsed(OriginalHook(Sheltron)) ||
                              JustUsed(OriginalHook(Sentinel), 4f) ||
                              JustUsed(DivineVeil, 4f) ||
                              JustUsed(Role.Rampart, 4f) ||
                              JustUsed(HallowedGround, 9f);

            #endregion

            // Interrupt
            if (Role.CanInterject())
                return Role.Interject;

            // Stun
            if (TargetIsCasting() && !JustUsed(Role.Interject))
                if (Role.CanLowBlow())
                    return Role.LowBlow;

            // Variant Cure
            if (Variant.CanCure(CustomComboPreset.PLD_Variant_Cure, Config.PLD_VariantCure))
                return Variant.Cure;

            if (Config.PLD_AoE_MitsOptions != 1)
            {
                if (InCombat() && //Player is in combat
                    !justMitted) //Player has not used a mitigation ability in the last 4-9 seconds
                {
                    //Hallowed Ground
                    if (ActionReady(HallowedGround) && //Hallowed Ground is ready
                        PlayerHealthPercentageHp() < 30) //Player's health is below 30%
                        return HallowedGround;

                    if (IsPlayerTargeted())
                    {
                        //Sentinel / Guardian
                        if (ActionReady(OriginalHook(Sentinel)) && //Sentinel is ready
                            PlayerHealthPercentageHp() < 60) //Player's health is below 60%
                            return OriginalHook(Sentinel);

                        //Rampart
                        if (Role.CanRampart(80))
                            return Role.Rampart;

                        //Reprisal
                        if (Role.CanReprisal(90, checkTargetForDebuff: false))
                            return Role.Reprisal;
                    }

                    //Bulwark
                    if (ActionReady(Bulwark) && //Bulwark is ready
                        PlayerHealthPercentageHp() < 70) //Player's health is below 80%
                        return Bulwark;

                    //Sheltron
                    if (ActionReady(OriginalHook(Sheltron)) && //Sheltron
                        PlayerHealthPercentageHp() < 90) //Player's health is below 95%
                        return OriginalHook(Sheltron);
                }
            }

            if (HasBattleTarget())
            {
                // Weavables
                if (canWeave)
                {
                    if (InMeleeRange())
                    {
                        // Requiescat
                        if (ActionReady(Requiescat) && cooldownFightOrFlight > 50)
                            return OriginalHook(Requiescat);

                        // Fight or Flight
                        if (canFightOrFlight)
                        {
                            if (!LevelChecked(Requiescat))
                                return OriginalHook(FightOrFlight);

                            else if (cooldownRequiescat < 0.5f && hasRequiescatMP && canEarlyWeave)
                                return OriginalHook(FightOrFlight);
                        }

                        // Variant Ultimatum
                        if (Variant.CanUltimatum(CustomComboPreset.PLD_Variant_Ultimatum))
                            return Variant.Ultimatum;

                        // Circle of Scorn / Spirits Within
                        if (cooldownFightOrFlight > 15)
                        {
                            if (ActionReady(CircleOfScorn))
                                return CircleOfScorn;

                            if (ActionReady(SpiritsWithin))
                                return OriginalHook(SpiritsWithin);
                        }
                    }

                    // Variant Spirit Dart
                    if (Variant.CanSpiritDart(CustomComboPreset.PLD_Variant_SpiritDart))
                        return Variant.SpiritDart;

                    // Blade of Honor
                    if (LevelChecked(BladeOfHonor) && OriginalHook(Requiescat) == BladeOfHonor)
                        return OriginalHook(Requiescat);
                }

                // Confiteor & Blades
                if (hasDivineMagicMP && (HasStatusEffect(Buffs.ConfiteorReady) || LevelChecked(BladeOfFaith) && OriginalHook(Confiteor) != Confiteor))
                    return OriginalHook(Confiteor);
            }

            // Holy Circle
            if (LevelChecked(HolyCircle) && hasDivineMagicMP && (hasDivineMight || hasRequiescat))
                return HolyCircle;

            // Basic Combo
            if (ComboTimer > 0 && ComboAction is TotalEclipse && LevelChecked(Prominence))
                return Prominence;

            return actionID;
        }
    }

    internal class PLD_ST_AdvancedMode : CustomCombo
    {
        protected internal override CustomComboPreset Preset { get; } = CustomComboPreset.PLD_ST_AdvancedMode;
        internal static int RoyalAuthorityCount => ActionWatching.CombatActions.Count(x => x == OriginalHook(RageOfHalone));

        protected override uint Invoke(uint actionID)
        {
            if (actionID is not FastBlade)
                return actionID;

            #region Variables

            var canFightOrFlight = OriginalHook(FightOrFlight) is FightOrFlight && ActionReady(FightOrFlight);
            float durationFightOrFlight = GetStatusEffectRemainingTime(Buffs.FightOrFlight);
            float cooldownFightOrFlight = GetCooldownRemainingTime(FightOrFlight);
            float cooldownRequiescat = GetCooldownRemainingTime(Requiescat);
            uint playerMP = LocalPlayer.CurrentMp;
            bool canWeave = CanWeave();
            bool canEarlyWeave = CanWeave(1.5f);
            bool hasRequiescat = HasStatusEffect(Buffs.Requiescat);
            bool hasDivineMight = HasStatusEffect(Buffs.DivineMight);
            bool hasFightOrFlight = HasStatusEffect(Buffs.FightOrFlight);
            bool hasDivineMagicMP = playerMP >= GetResourceCost(HolySpirit);
            bool hasJustUsedMitigation = JustUsed(OriginalHook(Sheltron)) || JustUsed(OriginalHook(Sentinel), 5f) ||
                                         JustUsed(Role.Rampart, 5f) || JustUsed(HallowedGround, 9f);
            bool hasRequiescatMP = IsNotEnabled(CustomComboPreset.PLD_ST_AdvancedMode_MP_Reserve) && playerMP >= GetResourceCost(HolySpirit) * 3.6 ||
                                   IsEnabled(CustomComboPreset.PLD_ST_AdvancedMode_MP_Reserve) && playerMP >= GetResourceCost(HolySpirit) * 3.6 + Config.PLD_ST_MP_Reserve;
            bool inBurstWindow = JustUsed(FightOrFlight, 30f);
            bool inAtonementStarter = HasStatusEffect(Buffs.AtonementReady);
            bool inAtonementFinisher = HasStatusEffect(Buffs.SepulchreReady);
            bool afterOpener = LevelChecked(BladeOfFaith) && RoyalAuthorityCount > 0;
            bool isDivineMightExpiring = GetStatusEffectRemainingTime(Buffs.DivineMight) < 6;
            bool isAboveMPReserve = IsNotEnabled(CustomComboPreset.PLD_ST_AdvancedMode_MP_Reserve) ||
                                    IsEnabled(CustomComboPreset.PLD_ST_AdvancedMode_MP_Reserve) && playerMP >= GetResourceCost(HolySpirit) + Config.PLD_ST_MP_Reserve;
            bool inAtonementPhase = HasStatusEffect(Buffs.AtonementReady) || HasStatusEffect(Buffs.SupplicationReady) || HasStatusEffect(Buffs.SepulchreReady);
            bool isAtonementExpiring = HasStatusEffect(Buffs.AtonementReady) && GetStatusEffectRemainingTime(Buffs.AtonementReady) < 6 ||
                                       HasStatusEffect(Buffs.SupplicationReady) && GetStatusEffectRemainingTime(Buffs.SupplicationReady) < 6 ||
                                       HasStatusEffect(Buffs.SepulchreReady) && GetStatusEffectRemainingTime(Buffs.SepulchreReady) < 6;

            #endregion

            // Interrupt
            if (IsEnabled(CustomComboPreset.PLD_ST_Interrupt)
                && Role.CanInterject())
                return Role.Interject;

            // Stun
            if (!TargetIsBoss()
                && TargetIsCasting()
                && !JustUsed(Role.Interject)
                && !InBossEncounter())
                if (IsEnabled(CustomComboPreset.PLD_ST_ShieldBash) && ActionReady(ShieldBash) && !JustUsed(Role.LowBlow) && !JustUsedOn(ShieldBash, CurrentTarget, 10))
                    return ShieldBash;
                else if (IsEnabled(CustomComboPreset.PLD_ST_LowBlow) && Role.CanLowBlow() && !JustUsed(ShieldBash))
                    return Role.LowBlow;

            // Variant Cure
            if (Variant.CanCure(CustomComboPreset.PLD_Variant_Cure, Config.PLD_VariantCure))
                return Variant.Cure;

            if (IsEnabled(CustomComboPreset.PLD_ST_AdvancedMode_BalanceOpener) &&
                Opener().FullOpener(ref actionID))
                return actionID;

            if (HasBattleTarget())
            {
                // Weavables
                if (canWeave)
                {
                    if (InMeleeRange())
                    {
                        // Requiescat
                        if (IsEnabled(CustomComboPreset.PLD_ST_AdvancedMode_Requiescat) && ActionReady(Requiescat) && cooldownFightOrFlight > 50)
                            return OriginalHook(Requiescat);

                        // Fight or Flight
                        if (IsEnabled(CustomComboPreset.PLD_ST_AdvancedMode_FoF) && canFightOrFlight && GetTargetHPPercent() >= Config.PLD_ST_FoF_Trigger)
                        {
                            if (!LevelChecked(Requiescat))
                            {
                                if (!LevelChecked(RageOfHalone))
                                {
                                    // Level 2-25
                                    if (ComboAction is FastBlade)
                                        return OriginalHook(FightOrFlight);
                                }

                                // Level 26-67
                                else if (ComboAction is RiotBlade)
                                    return OriginalHook(FightOrFlight);
                            }

                            // Level 68+
                            else if (cooldownRequiescat < 0.5f && hasRequiescatMP && canEarlyWeave && (ComboAction is RoyalAuthority || afterOpener))
                                return OriginalHook(FightOrFlight);
                        }

                        // Variant Ultimatum
                        if (Variant.CanUltimatum(CustomComboPreset.PLD_Variant_Ultimatum))
                            return Variant.Ultimatum;

                        // Circle of Scorn / Spirits Within
                        if (cooldownFightOrFlight > 15)
                        {
                            if (IsEnabled(CustomComboPreset.PLD_ST_AdvancedMode_CircleOfScorn) && ActionReady(CircleOfScorn))
                                return CircleOfScorn;

                            if (IsEnabled(CustomComboPreset.PLD_ST_AdvancedMode_SpiritsWithin) && ActionReady(SpiritsWithin))
                                return OriginalHook(SpiritsWithin);
                        }
                    }

                    // Variant Spirit Dart
                    if (Variant.CanSpiritDart(CustomComboPreset.PLD_Variant_SpiritDart))
                        return Variant.SpiritDart;

                    // Intervene
                    if (IsEnabled(CustomComboPreset.PLD_ST_AdvancedMode_Intervene) && LevelChecked(Intervene) && TimeMoving.Ticks == 0 &&
                        cooldownFightOrFlight > 40 && GetRemainingCharges(Intervene) > Config.PLD_Intervene_HoldCharges && !WasLastAction(Intervene) &&
                        (Config.PLD_Intervene_MeleeOnly == 1 && InMeleeRange() || GetTargetDistance() == 0 && Config.PLD_Intervene_MeleeOnly == 2))
                        return Intervene;

                    // Blade of Honor
                    if (IsEnabled(CustomComboPreset.PLD_ST_AdvancedMode_BladeOfHonor) && LevelChecked(BladeOfHonor) && OriginalHook(Requiescat) == BladeOfHonor)
                        return OriginalHook(Requiescat);

                    // Mitigation
                    if (IsEnabled(CustomComboPreset.PLD_ST_AdvancedMode_Mitigation) && IsPlayerTargeted() && !hasJustUsedMitigation && InCombat())
                    {
                        // Hallowed Ground
                        if (IsEnabled(CustomComboPreset.PLD_ST_AdvancedMode_HallowedGround) && ActionReady(HallowedGround) &&
                            PlayerHealthPercentageHp() < Config.PLD_ST_HallowedGround_Health && (Config.PLD_ST_HallowedGround_SubOption == 1 ||
                                                                                                 TargetIsBoss() && Config.PLD_ST_HallowedGround_SubOption == 2))
                            return HallowedGround;

                        // Sentinel / Guardian
                        if (IsEnabled(CustomComboPreset.PLD_ST_AdvancedMode_Sentinel) && ActionReady(OriginalHook(Sentinel)) &&
                            PlayerHealthPercentageHp() < Config.PLD_ST_Sentinel_Health && (Config.PLD_ST_Sentinel_SubOption == 1 ||
                                                                                           TargetIsBoss() && Config.PLD_ST_Sentinel_SubOption == 2))
                            return OriginalHook(Sentinel);

                        // Rampart
                        if (IsEnabled(CustomComboPreset.PLD_ST_AdvancedMode_Rampart) &&
                            Role.CanRampart(Config.PLD_ST_Rampart_Health) && (Config.PLD_ST_Rampart_SubOption == 1 ||
                                                                              TargetIsBoss() && Config.PLD_ST_Rampart_SubOption == 2))
                            return Role.Rampart;

                        // Sheltron
                        if (IsEnabled(CustomComboPreset.PLD_ST_AdvancedMode_Sheltron) && LevelChecked(Sheltron) &&
                            Gauge.OathGauge >= Config.PLD_ST_SheltronOption && PlayerHealthPercentageHp() < 95 &&
                            !HasStatusEffect(Buffs.Sheltron) && !HasStatusEffect(Buffs.HolySheltron))
                            return OriginalHook(Sheltron);
                    }
                }

                // Requiescat Phase
                if (hasDivineMagicMP)
                {
                    // Confiteor & Blades
                    if (IsEnabled(CustomComboPreset.PLD_ST_AdvancedMode_Confiteor) && HasStatusEffect(Buffs.ConfiteorReady) ||
                        IsEnabled(CustomComboPreset.PLD_ST_AdvancedMode_Blades) && LevelChecked(BladeOfFaith) && OriginalHook(Confiteor) != Confiteor)
                        return OriginalHook(Confiteor);

                    // Pre-Blades
                    if ((IsEnabled(CustomComboPreset.PLD_ST_AdvancedMode_Confiteor) || IsEnabled(CustomComboPreset.PLD_ST_AdvancedMode_Blades)) && hasRequiescat)
                        return HolySpirit;
                }

                // Goring Blade
                if (IsEnabled(CustomComboPreset.PLD_ST_AdvancedMode_GoringBlade) && HasStatusEffect(Buffs.GoringBladeReady) && InMeleeRange())
                    return GoringBlade;

                // Holy Spirit Prioritization
                if (IsEnabled(CustomComboPreset.PLD_ST_AdvancedMode_HolySpirit) && hasDivineMight && hasDivineMagicMP && isAboveMPReserve)
                {
                    // Delay Sepulchre / Prefer Sepulchre
                    if (inAtonementFinisher && (cooldownFightOrFlight < 3 || durationFightOrFlight > 3))
                        return HolySpirit;

                    // Fit in Burst
                    if (!inAtonementFinisher && hasFightOrFlight && durationFightOrFlight < 3)
                        return HolySpirit;
                }

                // Atonement: During Burst / Before Expiring / Spend Starter / Before Refreshing
                if (IsEnabled(CustomComboPreset.PLD_ST_AdvancedMode_Atonement) && inAtonementPhase && InMeleeRange() &&
                    (inBurstWindow || isAtonementExpiring || inAtonementStarter || ComboAction is RiotBlade))
                    return OriginalHook(Atonement);

                // Holy Spirit: During Burst / Before Expiring / Outside Melee / Before Refreshing
                if (IsEnabled(CustomComboPreset.PLD_ST_AdvancedMode_HolySpirit) && hasDivineMight && hasDivineMagicMP && isAboveMPReserve &&
                    (inBurstWindow || isDivineMightExpiring || !InMeleeRange() || ComboAction is RiotBlade))
                    return HolySpirit;

                // Out of Range
                if (IsEnabled(CustomComboPreset.PLD_ST_AdvancedMode_ShieldLob) && !InMeleeRange())
                {
                    // Holy Spirit (Not Moving)
                    if (LevelChecked(HolySpirit) && hasDivineMagicMP && isAboveMPReserve && TimeMoving.Ticks == 0 && Config.PLD_ShieldLob_SubOption == 2)
                        return HolySpirit;

                    // Shield Lob
                    if (LevelChecked(ShieldLob))
                        return ShieldLob;
                }
            }

            // Basic Combo
            if (ComboTimer > 0)
            {
                if (ComboAction is FastBlade && LevelChecked(RiotBlade))
                    return RiotBlade;

                if (ComboAction is RiotBlade && LevelChecked(RageOfHalone))
                    return OriginalHook(RageOfHalone);
            }

            return actionID;
        }
    }

    internal class PLD_AoE_AdvancedMode : CustomCombo
    {
        protected internal override CustomComboPreset Preset { get; } = CustomComboPreset.PLD_AoE_AdvancedMode;

        protected override uint Invoke(uint actionID)
        {
            if (actionID is not TotalEclipse)
                return actionID;

            #region Variables

            var canFightOrFlight = OriginalHook(FightOrFlight) is FightOrFlight && ActionReady(FightOrFlight);
            float cooldownFightOrFlight = GetCooldownRemainingTime(FightOrFlight);
            float cooldownRequiescat = GetCooldownRemainingTime(Requiescat);
            uint playerMP = LocalPlayer.CurrentMp;
            bool canWeave = CanWeave();
            bool canEarlyWeave = CanWeave(1.5f);
            bool hasRequiescat = HasStatusEffect(Buffs.Requiescat);
            bool hasDivineMight = HasStatusEffect(Buffs.DivineMight);
            bool hasDivineMagicMP = playerMP >= GetResourceCost(HolySpirit);
            bool hasJustUsedMitigation = JustUsed(OriginalHook(Sheltron)) || JustUsed(OriginalHook(Sentinel), 5f) ||
                                         JustUsed(Role.Rampart, 5f) || JustUsed(HallowedGround, 9f);
            bool hasRequiescatMP = IsNotEnabled(CustomComboPreset.PLD_AoE_AdvancedMode_MP_Reserve) && playerMP >= GetResourceCost(HolySpirit) * 3.6 ||
                                   IsEnabled(CustomComboPreset.PLD_AoE_AdvancedMode_MP_Reserve) && playerMP >= GetResourceCost(HolySpirit) * 3.6 + Config.PLD_AoE_MP_Reserve;
            bool isAboveMPReserve = IsNotEnabled(CustomComboPreset.PLD_AoE_AdvancedMode_MP_Reserve) ||
                                    IsEnabled(CustomComboPreset.PLD_AoE_AdvancedMode_MP_Reserve) && playerMP >= GetResourceCost(HolySpirit) + Config.PLD_AoE_MP_Reserve;

            #endregion

            // Interrupt
            if (IsEnabled(CustomComboPreset.PLD_AoE_Interrupt)
                && Role.CanInterject())
                return Role.Interject;

            // Stun
            if (TargetIsCasting() && !JustUsed(Role.Interject))
                if (IsEnabled(CustomComboPreset.PLD_AoE_ShieldBash) && ActionReady(ShieldBash) && !JustUsed(Role.LowBlow) && !JustUsedOn(ShieldBash, CurrentTarget, 10))
                    return ShieldBash;
                else if (IsEnabled(CustomComboPreset.PLD_AoE_LowBlow) && Role.CanLowBlow() && !JustUsed(ShieldBash))
                    return Role.LowBlow;

            // Variant Cure
            if (Variant.CanCure(CustomComboPreset.PLD_Variant_Cure, Config.PLD_VariantCure))
                return Variant.Cure;

            if (HasBattleTarget())
            {
                // Weavables
                if (canWeave)
                {
                    if (InMeleeRange())
                    {
                        // Requiescat
                        if (IsEnabled(CustomComboPreset.PLD_AoE_AdvancedMode_Requiescat) && ActionReady(Requiescat) && cooldownFightOrFlight > 50)
                            return OriginalHook(Requiescat);

                        // Fight or Flight
                        if (IsEnabled(CustomComboPreset.PLD_AoE_AdvancedMode_FoF) && canFightOrFlight && GetTargetHPPercent() >= Config.PLD_AoE_FoF_Trigger)
                        {
                            if (!LevelChecked(Requiescat))
                                return OriginalHook(FightOrFlight);

                            else if (cooldownRequiescat < 0.5f && hasRequiescatMP && canEarlyWeave)
                                return OriginalHook(FightOrFlight);
                        }

                        // Variant Ultimatum
                        if (Variant.CanUltimatum(CustomComboPreset.PLD_Variant_Ultimatum))
                            return Variant.Ultimatum;

                        // Circle of Scorn / Spirits Within
                        if (cooldownFightOrFlight > 15)
                        {
                            if (IsEnabled(CustomComboPreset.PLD_AoE_AdvancedMode_CircleOfScorn) && ActionReady(CircleOfScorn))
                                return CircleOfScorn;

                            if (IsEnabled(CustomComboPreset.PLD_AoE_AdvancedMode_SpiritsWithin) && ActionReady(SpiritsWithin))
                                return OriginalHook(SpiritsWithin);
                        }
                    }

                    // Variant Spirit Dart
                    if (Variant.CanSpiritDart(CustomComboPreset.PLD_Variant_SpiritDart))
                        return Variant.SpiritDart;

                    // Intervene
                    if (IsEnabled(CustomComboPreset.PLD_AoE_AdvancedMode_Intervene) && LevelChecked(Intervene) && TimeMoving.Ticks == 0 &&
                        cooldownFightOrFlight > 40 && GetRemainingCharges(Intervene) > Config.PLD_AoE_Intervene_HoldCharges && !WasLastAction(Intervene) &&
                        (Config.PLD_AoE_Intervene_MeleeOnly == 1 && InMeleeRange() || GetTargetDistance() == 0 && Config.PLD_AoE_Intervene_MeleeOnly == 2))
                        return Intervene;

                    // Blade of Honor
                    if (IsEnabled(CustomComboPreset.PLD_AoE_AdvancedMode_BladeOfHonor) && LevelChecked(BladeOfHonor) && OriginalHook(Requiescat) == BladeOfHonor)
                        return OriginalHook(Requiescat);

                    // Mitigation
                    if (IsEnabled(CustomComboPreset.PLD_AoE_AdvancedMode_Mitigation) && IsPlayerTargeted() && !hasJustUsedMitigation && InCombat())
                    {
                        // Hallowed Ground
                        if (IsEnabled(CustomComboPreset.PLD_AoE_AdvancedMode_HallowedGround) && ActionReady(HallowedGround) &&
                            PlayerHealthPercentageHp() < Config.PLD_AoE_HallowedGround_Health && (Config.PLD_AoE_HallowedGround_SubOption == 1 ||
                                                                                                  TargetIsBoss() && Config.PLD_AoE_HallowedGround_SubOption == 2))
                            return HallowedGround;

                        // Sentinel / Guardian
                        if (IsEnabled(CustomComboPreset.PLD_AoE_AdvancedMode_Sentinel) && ActionReady(OriginalHook(Sentinel)) &&
                            PlayerHealthPercentageHp() < Config.PLD_AoE_Sentinel_Health && (Config.PLD_AoE_Sentinel_SubOption == 1 ||
                                                                                            TargetIsBoss() && Config.PLD_AoE_Sentinel_SubOption == 2))
                            return OriginalHook(Sentinel);

                        // Rampart
                        if (IsEnabled(CustomComboPreset.PLD_AoE_AdvancedMode_Rampart) &&
                            Role.CanRampart(Config.PLD_AoE_Rampart_Health) && (Config.PLD_AoE_Rampart_SubOption == 1 ||
                                                                               TargetIsBoss() && Config.PLD_AoE_Rampart_SubOption == 2))
                            return Role.Rampart;

                        // Sheltron
                        if (IsEnabled(CustomComboPreset.PLD_AoE_AdvancedMode_Sheltron) && LevelChecked(Sheltron) &&
                            Gauge.OathGauge >= Config.PLD_AoE_SheltronOption && PlayerHealthPercentageHp() < 95 &&
                            !HasStatusEffect(Buffs.Sheltron) && !HasStatusEffect(Buffs.HolySheltron))
                            return OriginalHook(Sheltron);
                    }
                }

                // Confiteor & Blades
                if (hasDivineMagicMP && (IsEnabled(CustomComboPreset.PLD_AoE_AdvancedMode_Confiteor) && HasStatusEffect(Buffs.ConfiteorReady) ||
                                         IsEnabled(CustomComboPreset.PLD_AoE_AdvancedMode_Blades) && LevelChecked(BladeOfFaith) && OriginalHook(Confiteor) != Confiteor))
                    return OriginalHook(Confiteor);
            }

            // Holy Circle
            if (LevelChecked(HolyCircle) && hasDivineMagicMP && (IsEnabled(CustomComboPreset.PLD_AoE_AdvancedMode_HolyCircle) && isAboveMPReserve && hasDivineMight ||
                                                                 (IsEnabled(CustomComboPreset.PLD_AoE_AdvancedMode_Confiteor) || IsEnabled(CustomComboPreset.PLD_AoE_AdvancedMode_Blades)) && hasRequiescat))
                return HolyCircle;

            // Basic Combo
            if (ComboTimer > 0 && ComboAction is TotalEclipse && LevelChecked(Prominence))
                return Prominence;

            return actionID;
        }
    }

    internal class PLD_Requiescat_Confiteor : CustomCombo
    {
        protected internal override CustomComboPreset Preset { get; } = CustomComboPreset.PLD_Requiescat_Options;

        protected override uint Invoke(uint actionID)
        {
            if (actionID is not (Requiescat or Imperator))
                return actionID;

            var canFightOrFlight = OriginalHook(FightOrFlight) is FightOrFlight && ActionReady(FightOrFlight);

            // Fight or Flight
            if (Config.PLD_Requiescat_SubOption == 2 && (!LevelChecked(Requiescat) || (canFightOrFlight && ActionReady(Requiescat))))
                return OriginalHook(FightOrFlight);

            // Confiteor & Blades
            if (HasStatusEffect(Buffs.ConfiteorReady) || LevelChecked(BladeOfFaith) && OriginalHook(Confiteor) != Confiteor)
                return OriginalHook(Confiteor);

            // Pre-Blades
            if (HasStatusEffect(Buffs.Requiescat))
            {
                // AoE
                if (LevelChecked(HolyCircle) && NumberOfEnemiesInRange(HolyCircle, null) > 2)
                    return HolyCircle;

                return HolySpirit;
            }

            return actionID;
        }
    }

    internal class PLD_CircleOfScorn : CustomCombo
    {
        protected internal override CustomComboPreset Preset { get; } = CustomComboPreset.PLD_SpiritsWithin;

        protected override uint Invoke(uint actionID)
        {
            if (actionID is not (SpiritsWithin or Expiacion))
                return actionID;

            if (IsOffCooldown(OriginalHook(SpiritsWithin)))
                return OriginalHook(SpiritsWithin);

            if (ActionReady(CircleOfScorn) && (Config.PLD_SpiritsWithin_SubOption == 1 || Config.PLD_SpiritsWithin_SubOption == 2 && JustUsed(OriginalHook(SpiritsWithin), 5f)))
                return CircleOfScorn;

            return actionID;
        }
    }

    internal class PLD_ShieldLob_HolySpirit : CustomCombo
    {
        protected internal override CustomComboPreset Preset { get; } = CustomComboPreset.PLD_ShieldLob_Feature;

        protected override uint Invoke(uint actionID)
        {
            if (actionID is not ShieldLob)
                return actionID;

            if (LevelChecked(HolySpirit) && GetResourceCost(HolySpirit) <= LocalPlayer.CurrentMp && (TimeMoving.Ticks == 0 || HasStatusEffect(Buffs.DivineMight)))
                return HolySpirit;

            return actionID;
        }
    }

    internal class PLD_RetargetClemency : CustomCombo
    {
        protected internal override CustomComboPreset Preset { get; } = CustomComboPreset.PLD_RetargetClemency;

        protected override uint Invoke(uint actionID)
        {
            if (actionID != Clemency)
                return actionID;

            int healthThreshold = Config.PLD_RetargetClemency_Health;

            var target =
                //Mouseover retarget option
                (IsEnabled(CustomComboPreset.PLD_RetargetClemency_MO)
                    ? SimpleTarget.UIMouseOverTarget.IfNotThePlayer().IfInParty()
                    : null) ??

                //Hard target
                SimpleTarget.HardTarget.IfFriendly() ??

                //Lowest HP option
                (IsEnabled(CustomComboPreset.PLD_RetargetClemency_LowHP)
                 && PlayerHealthPercentageHp() > healthThreshold
                    ? SimpleTarget.LowestHPAlly.IfNotThePlayer().IfAlive()
                    : null);

            return target != null
                ? actionID.Retarget(target)
                : actionID;
        }
    }
    internal class PLD_RetargetSheltron : CustomCombo
    {
        protected internal override CustomComboPreset Preset { get; } = CustomComboPreset.PLD_RetargetSheltron;

        protected override uint Invoke(uint action)
        {
            if (action is not (Sheltron or HolySheltron))
                return action;

            var target =
                //Mouseover retarget option
                (IsEnabled(CustomComboPreset.PLD_RetargetSheltron_MO)
                    ? SimpleTarget.UIMouseOverTarget.IfNotThePlayer().IfInParty()
                    : null) ??

                //Hard target retarget
                SimpleTarget.HardTarget.IfNotThePlayer().IfInParty() ??
                
                //Targets target retarget option
                (IsEnabled(CustomComboPreset.PLD_RetargetSheltron_TT)
                    && !PlayerHasAggro
                    ? SimpleTarget.TargetsTarget.IfNotThePlayer().IfInParty()
                    : null);

            // Intervention if trying to Buff an ally
            if (ActionReady(Intervention) &&
                target != null &&
                CanApplyStatus(target, Buffs.Intervention))
                return Intervention.Retarget([Sheltron, HolySheltron], target);

            return action;
        }
    }
    #region One-Button Mitigation

    internal class PLD_Mit_OneButton : CustomCombo
    {
        protected internal override CustomComboPreset Preset { get; } = CustomComboPreset.PLD_Mit_OneButton;

        protected override uint Invoke(uint actionID)
        {
            if (actionID is not Bulwark)
                return actionID;

            if (IsEnabled(CustomComboPreset.PLD_Mit_HallowedGround_Max) &&
                ActionReady(HallowedGround) &&
                PlayerHealthPercentageHp() <= Config.PLD_Mit_HallowedGround_Max_Health &&
                ContentCheck.IsInConfiguredContent(
                    Config.PLD_Mit_HallowedGround_Max_Difficulty,
                    Config.PLD_Mit_HallowedGround_Max_DifficultyListSet
                ))
                return HallowedGround;

            foreach (int priority in Config.PLD_Mit_Priorities.Items.OrderBy(x => x))
            {
                int index = Config.PLD_Mit_Priorities.IndexOf(priority);
                if (CheckMitigationConfigMeetsRequirements(index, out uint action))
                    return action;
            }

            return actionID;
        }
    }

    internal class PLD_Mit_OneButton_Party : CustomCombo
    {
        protected internal override CustomComboPreset Preset { get; } =
            CustomComboPreset.PLD_Mit_Party;

        protected override uint Invoke(uint action)
        {
            if (action is not DivineVeil)
                return action;

            if (ActionReady(Role.Reprisal))
                return Role.Reprisal;

            if (ActionReady(PassageOfArms) &&
                IsEnabled(CustomComboPreset.PLD_Mit_Party_Wings) &&
                !HasStatusEffect(Buffs.PassageOfArms, anyOwner: true))
                return PassageOfArms;

            return action;
        }
    }

    #endregion

    internal class PLD_RetargetShieldBash : CustomCombo
    {
        protected internal override CustomComboPreset Preset { get; } = CustomComboPreset.PLD_RetargetShieldBash;

        protected override uint Invoke(uint actionID)
        {
            if (actionID is not ShieldBash)
                return actionID;

            var tar = SimpleTarget.StunnableEnemy(Config.PLD_RetargetStunLockout ? Config.PLD_RetargetShieldBash_Strength : 3);

            if (tar is not null)
                return ShieldBash.Retarget(actionID, tar);
            else if (Config.PLD_RetargetStunLockout)
                return All.SavageBlade;

            return actionID;
        }
    }
}
