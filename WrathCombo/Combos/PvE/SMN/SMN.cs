using Dalamud.Game.ClientState.JobGauge.Types;
using WrathCombo.CustomComboNS;
using System.Linq;

namespace WrathCombo.Combos.PvE;

internal partial class SMN : Caster
{
    #region Small Features

    internal class SMN_Raise : CustomCombo
    {
        protected internal override CustomComboPreset Preset { get; } = CustomComboPreset.SMN_Raise;

        protected override uint Invoke(uint actionID)
        {
            if (actionID != Role.Swiftcast)
                return actionID;

            if (Variant.CanRaise(CustomComboPreset.SMN_Variant_Raise))
                return Variant.Raise;

            if (IsOnCooldown(Role.Swiftcast))
                return Resurrection;
            return actionID;
        }
    }
    internal class SMN_Searing : CustomCombo
    {
        protected internal override CustomComboPreset Preset { get; } = CustomComboPreset.SMN_Searing;

        protected override uint Invoke(uint actionID)
        {
            if (actionID != SearingLight)
                return actionID;

            if (HasStatusEffect(Buffs.RubysGlimmer))
                return SearingFlash;

            if (HasStatusEffect(Buffs.SearingLight, anyOwner: true))
                return 11;

            return actionID;
        }
    }
    internal class SMN_RuinMobility : CustomCombo
    {
        protected internal override CustomComboPreset Preset { get; } = CustomComboPreset.SMN_RuinMobility;

        protected override uint Invoke(uint actionID)
        {
            if (actionID != Ruin4)
                return actionID;
            bool furtherRuin = HasStatusEffect(Buffs.FurtherRuin);

            if (!furtherRuin)
                return Ruin3;
            return actionID;
        }
    }

    internal class SMN_EDFester : CustomCombo
    {
        protected internal override CustomComboPreset Preset { get; } = CustomComboPreset.SMN_EDFester;

        protected override uint Invoke(uint actionID)
        {
            if (actionID is not (Fester or Necrotize))
                return actionID;

            SMNGauge gauge = GetJobGauge<SMNGauge>();
            if (HasStatusEffect(Buffs.FurtherRuin) && IsOnCooldown(EnergyDrain) && !gauge.HasAetherflowStacks && IsEnabled(CustomComboPreset.SMN_EDFester_Ruin4))
                return Ruin4;

            if (LevelChecked(EnergyDrain) && !gauge.HasAetherflowStacks)
                return EnergyDrain;

            return actionID;
        }
    }

    internal class SMN_ESPainflare : CustomCombo
    {
        protected internal override CustomComboPreset Preset { get; } = CustomComboPreset.SMN_ESPainflare;

        protected override uint Invoke(uint actionID)
        {
            if (actionID is not Painflare)
                return actionID;

            SMNGauge gauge = GetJobGauge<SMNGauge>();

            if (!LevelChecked(Painflare) || gauge.HasAetherflowStacks)
                return actionID;

            if (HasStatusEffect(Buffs.FurtherRuin) && IsOnCooldown(EnergySiphon) && IsEnabled(CustomComboPreset.SMN_ESPainflare_Ruin4))
                return Ruin4;

            if (LevelChecked(EnergySiphon))
                return EnergySiphon;

            if (LevelChecked(EnergyDrain))
                return EnergyDrain;

            return actionID;
        }
    }

    internal class SMN_CarbuncleReminder : CustomCombo
    {
        protected internal override CustomComboPreset Preset => CustomComboPreset.SMN_CarbuncleReminder;

        protected override uint Invoke(uint actionID)
        {
            if (actionID is not (Ruin or Ruin2 or Ruin3 or DreadwyrmTrance or
                 AstralFlow or EnkindleBahamut or SearingLight or
                 RadiantAegis or Outburst or Tridisaster or
                 PreciousBrilliance or Gemshine))
                return actionID;

            if (NeedToSummon && ActionReady(SummonCarbuncle))
                return SummonCarbuncle;

            return actionID;
        }
    }

    internal class SMN_Egi_AstralFlow : CustomCombo
    {
        protected internal override CustomComboPreset Preset => CustomComboPreset.SMN_Egi_AstralFlow;

        protected override uint Invoke(uint actionID)
        {
            if ((actionID is SummonTopaz or SummonTitan or SummonTitan2 or SummonEmerald or SummonGaruda or SummonGaruda2 or SummonRuby or SummonIfrit or SummonIfrit2 && HasStatusEffect(Buffs.TitansFavor)) ||
                (actionID is SummonTopaz or SummonTitan or SummonTitan2 or SummonEmerald or SummonGaruda or SummonGaruda2 && HasStatusEffect(Buffs.GarudasFavor)) ||
                (actionID is SummonTopaz or SummonTitan or SummonTitan2 or SummonRuby or SummonIfrit or SummonIfrit2 && (HasStatusEffect(Buffs.IfritsFavor) || (ComboAction == CrimsonCyclone && InMeleeRange()))))
                return OriginalHook(AstralFlow);

            return actionID;
        }
    }
    internal class SMN_DemiAbilities : CustomCombo
    {
        protected internal override CustomComboPreset Preset => CustomComboPreset.SMN_DemiAbilities;

        protected override uint Invoke(uint actionID)
        {
            if (actionID is not (Aethercharge or DreadwyrmTrance or SummonBahamut) &&
                actionID is not (SummonPhoenix or SummonSolarBahamut))
                return actionID;

            if (IsOffCooldown(EnkindleBahamut) && OriginalHook(Ruin) is AstralImpulse)
                return OriginalHook(EnkindleBahamut);

            if (IsOffCooldown(EnkindlePhoenix) && OriginalHook(Ruin) is FountainOfFire)
                return OriginalHook(EnkindlePhoenix);

            if (IsOffCooldown(EnkindleSolarBahamut) && OriginalHook(Ruin) is UmbralImpulse)
                return OriginalHook(EnkindleBahamut);

            if ((OriginalHook(AstralFlow) is Deathflare && IsOffCooldown(Deathflare)) || (OriginalHook(AstralFlow) is Rekindle && IsOffCooldown(Rekindle)))
                return OriginalHook(AstralFlow);

            if (OriginalHook(AstralFlow) is Sunflare && IsOffCooldown(Sunflare))
                return OriginalHook(Sunflare);

            return actionID;
        }
    }

    #endregion

    #region Simple
    internal class SMN_Simple_Combo : CustomCombo
    {
        protected internal override CustomComboPreset Preset { get; } = CustomComboPreset.SMN_ST_Simple_Combo;

        protected override uint Invoke(uint actionID)
        {
            if (actionID is not (Ruin or Ruin2))
                return actionID;
                        
            #region Variants

            if (Variant.CanCure(CustomComboPreset.SMN_Variant_Cure, Config.SMN_VariantCure))
                return Variant.Cure;

            if (Variant.CanRampart(CustomComboPreset.SMN_Variant_Rampart, WeaveTypes.SpellWeave))
                return Variant.Rampart;

            if (NeedToSummon && ActionReady(SummonCarbuncle))
                return SummonCarbuncle;

            #endregion

            #region OGCD

            if (CanSpellWeave())
            {
                // Searing Light
                if (IsOffCooldown(SearingLight) && LevelChecked(SearingLight) && !HasStatusEffect(Buffs.SearingLight, anyOwner: true) && CurrentDemiSummon is not DemiSummon.None)
                    return SearingLight;
                   
                // Energy Drain
                if (!Gauge.HasAetherflowStacks && ActionReady(EnergyDrain))
                    return EnergyDrain;

                //Searing Flash
                if (HasStatusEffect(Buffs.RubysGlimmer) && LevelChecked(SearingFlash))
                    return SearingFlash;

                // Demi Nuke
                if (CurrentDemiSummon is not DemiSummon.None)
                {
                    if (ActionReady(OriginalHook(EnkindleBahamut)))
                        return OriginalHook(EnkindleBahamut);

                    if (ActionReady(AstralFlow))
                        return OriginalHook(AstralFlow);
                }

                // Lux Solaris
                if (ActionReady(LuxSolaris) &&
                    (PlayerHealthPercentageHp() < 100 || (GetStatusEffectRemainingTime(Buffs.RefulgentLux) is < 3 and > 0)))
                    return OriginalHook(LuxSolaris);

                // Fester
                
                if (ActionReady(Fester))
                    return OriginalHook(Fester);

                //Self Shield Overcap
                if (!HasStatusEffect(Buffs.SearingLight) && GetRemainingCharges(RadiantAegis) == 2 && ActionReady(RadiantAegis))
                    return RadiantAegis;

                // Lucid Dreaming
                if (Role.CanLucidDream(4000))
                    return Role.LucidDreaming;
            }

            #endregion

            // Demi
            if (PartyInCombat() && ActionReady(OriginalHook(Aethercharge)))
                return OriginalHook(Aethercharge);

            #region Titan Phase
            if (IsTitanAttuned || OriginalHook(AstralFlow) is MountainBuster) //Titan attunement ends before last mountian buster
            {
                if (ActionReady(AstralFlow) && CanSpellWeave())
                    return OriginalHook(AstralFlow);

                if (GemshineReady)
                    return OriginalHook(Gemshine);
            }
            #endregion

            #region Garuda Phase
            if (IsGarudaAttuned || OriginalHook(AstralFlow) is Slipstream)
            {
                if (HasStatusEffect(Buffs.GarudasFavor) && (!IsMoving() || HasStatusEffect(Role.Buffs.Swiftcast)))
                    return OriginalHook(AstralFlow);

                if (GemshineReady)
                    return OriginalHook(Gemshine);

                if (ActionReady(Ruin4) && IsMoving())
                    return Ruin4;
            }
            #endregion

            #region Ifrit Phase

            if (IsIfritAttuned || OriginalHook(AstralFlow) is CrimsonCyclone or CrimsonStrike)
            {                
                if (GemshineReady && (!IsMoving() || HasStatusEffect(Role.Buffs.Swiftcast)))
                    return OriginalHook(Gemshine);

                if (HasStatusEffect(Buffs.IfritsFavor) || HasStatusEffect(Buffs.CrimsonStrike) && InMeleeRange())
                    return OriginalHook(AstralFlow);

                if (ActionReady(Ruin4) && !HasStatusEffect(Role.Buffs.Swiftcast) && GemshineReady)
                    return Ruin4;
            }
            #endregion

            // Egi Order 
            if (!ActionReady(OriginalHook(Aethercharge)) && Gauge.SummonTimerRemaining == 0 && Gauge.AttunementTimerRemaining == 0)
            {               
                if (Gauge.IsTitanReady)
                    return OriginalHook(SummonTopaz);

                if (Gauge.IsGarudaReady)
                    return OriginalHook(SummonEmerald);

                if (Gauge.IsIfritReady)
                    return OriginalHook(SummonRuby);
            }

            // Ruin 4 Dump
            if (LevelChecked(Ruin4) && Gauge.SummonTimerRemaining == 0 && Gauge.AttunementTimerRemaining == 0 && HasStatusEffect(Buffs.FurtherRuin))
                return Ruin4;

            return actionID;
        }
    }

    internal class SMN_Simple_Combo_AoE : CustomCombo
    {
        protected internal override CustomComboPreset Preset { get; } = CustomComboPreset.SMN_AoE_Simple_Combo;

        protected override uint Invoke(uint actionID)
        {
            if (actionID is not (Outburst or Tridisaster))
                return actionID;

            #region Variants

            if (Variant.CanCure(CustomComboPreset.SMN_Variant_Cure, Config.SMN_VariantCure))
                return Variant.Cure;

            if (Variant.CanRampart(CustomComboPreset.SMN_Variant_Rampart, WeaveTypes.SpellWeave))
                return Variant.Rampart;

            if (NeedToSummon && ActionReady(SummonCarbuncle))
                return SummonCarbuncle;

            #endregion

            #region OGCD

            if (CanSpellWeave())
            {
                // Searing Light
                if (IsOffCooldown(SearingLight) && LevelChecked(SearingLight) && !HasStatusEffect(Buffs.SearingLight, anyOwner: true) && CurrentDemiSummon is not DemiSummon.None)
                    return SearingLight;

                // Energy Drain
                if (!Gauge.HasAetherflowStacks && ActionReady(EnergyDrain))
                {
                    if (!LevelChecked(EnergySiphon))
                        return EnergyDrain;
                    else
                        return EnergySiphon;
                }

                //Searing Flash
                if (HasStatusEffect(Buffs.RubysGlimmer) && LevelChecked(SearingFlash))
                    return SearingFlash;

                // Demi Nuke
                if (CurrentDemiSummon is not DemiSummon.None)
                {
                    if (ActionReady(OriginalHook(EnkindleBahamut)))
                        return OriginalHook(EnkindleBahamut);

                    if (ActionReady(AstralFlow))
                        return OriginalHook(AstralFlow);
                }

                // Lux Solaris
                if (ActionReady(LuxSolaris) &&
                    (PlayerHealthPercentageHp() < 100 || (GetStatusEffectRemainingTime(Buffs.RefulgentLux) is < 3 and > 0)))
                    return OriginalHook(LuxSolaris);

                // Fester

                if (!LevelChecked(Painflare) && ActionReady(Fester))
                    return OriginalHook(Fester);

                if (ActionReady(Painflare))
                    return OriginalHook(Painflare);

                //Self Shield Overcap
                if (!HasStatusEffect(Buffs.SearingLight) && GetRemainingCharges(RadiantAegis) == 2 && ActionReady(RadiantAegis))
                    return RadiantAegis;

                // Lucid Dreaming
                if (Role.CanLucidDream(4000))
                    return Role.LucidDreaming;
            }

            #endregion

            // Demi
            if (PartyInCombat() && ActionReady(OriginalHook(Aethercharge)))
                return OriginalHook(Aethercharge);

            #region Titan Phase
            if (IsTitanAttuned || OriginalHook(AstralFlow) is MountainBuster)
            {
                if (ActionReady(AstralFlow) && CanSpellWeave())
                    return OriginalHook(AstralFlow);

                if (GemshineReady)
                    return OriginalHook(PreciousBrilliance);

            }    
            #endregion

            #region Garuda Phase
            if (IsGarudaAttuned || OriginalHook(AstralFlow) is Slipstream)
            {
                if (HasStatusEffect(Buffs.GarudasFavor) && (!IsMoving() || HasStatusEffect(Role.Buffs.Swiftcast)))
                    return OriginalHook(AstralFlow);

                if (GemshineReady)
                    return OriginalHook(PreciousBrilliance);

                if (ActionReady(Ruin4) && IsMoving())
                    return Ruin4;
            }
            #endregion

            #region Ifrit Phase

            if (IsIfritAttuned || OriginalHook(AstralFlow) is CrimsonCyclone or CrimsonStrike)
            {
                if (GemshineReady && (!IsMoving() || HasStatusEffect(Role.Buffs.Swiftcast)))
                    return OriginalHook(PreciousBrilliance);

                if (HasStatusEffect(Buffs.IfritsFavor) || HasStatusEffect(Buffs.CrimsonStrike) && InMeleeRange())
                    return OriginalHook(AstralFlow);

                if (ActionReady(Ruin4) && !HasStatusEffect(Role.Buffs.Swiftcast) && GemshineReady)
                    return Ruin4;
            }
            #endregion

            // Egi Order 
            if (!ActionReady(OriginalHook(Aethercharge)) && Gauge.SummonTimerRemaining == 0 && Gauge.AttunementTimerRemaining == 0)
            {
                if (Gauge.IsTitanReady)
                    return OriginalHook(SummonTopaz);

                if (Gauge.IsGarudaReady)
                    return OriginalHook(SummonEmerald);

                if (Gauge.IsIfritReady)
                    return OriginalHook(SummonRuby);
            }

            // Ruin 4 Dump
            if (LevelChecked(Ruin4) && Gauge.SummonTimerRemaining == 0 && Gauge.AttunementTimerRemaining == 0 && HasStatusEffect(Buffs.FurtherRuin))
                return Ruin4;

            return actionID;
        }
    }
    #endregion

    #region Advanced
    internal class SMN_ST_Advanced_Combo : CustomCombo
    {
        internal static int DemiAttackCount = 0;
        protected internal override CustomComboPreset Preset { get; } = CustomComboPreset.SMN_ST_Advanced_Combo;

        protected override uint Invoke(uint actionID)
        {
            if (actionID is not (Ruin or Ruin2))
                return actionID;

            #region Variables
            int SummonerBurstPhase = Config.SMN_ST_BurstPhase;
            int lucidThreshold = Config.SMN_ST_Lucid;
            int swiftcastPhase = Config.SMN_ST_SwiftcastPhase;
            int burstDelay = IsEnabled(CustomComboPreset.SMN_ST_Advanced_Combo_DemiEgiMenu_oGCDPooling) ? Config.SMN_ST_Burst_Delay : 0;

            bool TitanAstralFlow = IsEnabled(CustomComboPreset.SMN_ST_Advanced_Combo_Egi_AstralFlow) && Config.SMN_ST_Egi_AstralFlow[0];
            bool IfritAstralFlowCyclone = IsEnabled(CustomComboPreset.SMN_ST_Advanced_Combo_Egi_AstralFlow) && Config.SMN_ST_Egi_AstralFlow[1];
            bool IfritAstralFlowStrike = IsEnabled(CustomComboPreset.SMN_ST_Advanced_Combo_Egi_AstralFlow) && Config.SMN_ST_Egi_AstralFlow[3];
            bool GarudaAstralFlow = IsEnabled(CustomComboPreset.SMN_ST_Advanced_Combo_Egi_AstralFlow) && Config.SMN_ST_Egi_AstralFlow[2];

            var searingInSummon = GetCooldownRemainingTime(SearingLight) > (Gauge.SummonTimerRemaining / 1000f) + GCDTotal;

            DemiAttackCount = CurrentDemiSummon is not DemiSummon.None ? TimesUsedSinceOtherAction(OriginalHook(Aethercharge), [AstralImpulse, UmbralImpulse, FountainOfFire, AstralFlare, UmbralFlare, BrandOfPurgatory]) : 0;
            #endregion

            //Opener
            if (IsEnabled(CustomComboPreset.SMN_ST_Advanced_Combo_Balance_Opener) &&
                Opener().FullOpener(ref actionID))
                return actionID;

            #region Variants

            if (Variant.CanCure(CustomComboPreset.SMN_Variant_Cure, Config.SMN_VariantCure))
                return Variant.Cure;

            if (Variant.CanRampart(CustomComboPreset.SMN_Variant_Rampart, WeaveTypes.SpellWeave))
                return Variant.Rampart;

            if (NeedToSummon && ActionReady(SummonCarbuncle))
                return SummonCarbuncle;

            #endregion

            #region OGCD

            // Emergency priority Demi Nuke to prevent waste if you can't get demi attacks out to satisfy the slider check.
            if (CurrentDemiSummon is not DemiSummon.None && IsEnabled(CustomComboPreset.SMN_ST_Advanced_Combo_DemiSummons_Attacks) && Gauge.SummonTimerRemaining <= 2500)
            {
                if (IsEnabled(CustomComboPreset.SMN_ST_Advanced_Combo_DemiSummons_Attacks))
                {
                    if (ActionReady(OriginalHook(EnkindleBahamut)))
                        return OriginalHook(EnkindleBahamut);
            
                   if (ActionReady(AstralFlow) && ((IsEnabled(CustomComboPreset.SMN_ST_Advanced_Combo_DemiSummons_Rekindle) && CurrentDemiSummon is DemiSummon.Phoenix) || CurrentDemiSummon is not DemiSummon.Phoenix))
                        return OriginalHook(AstralFlow);
                }
            }

            if (CanSpellWeave())
            {
                // Searing Light
                if (IsEnabled(CustomComboPreset.SMN_ST_Advanced_Combo_SearingLight) && IsOffCooldown(SearingLight) && LevelChecked(SearingLight) && ((!HasStatusEffect(Buffs.SearingLight, anyOwner: true) && Config.SMN_ST_Searing_Any) || !Config.SMN_ST_Searing_Any))
                {
                    if (IsEnabled(CustomComboPreset.SMN_ST_Advanced_Combo_SearingLight_Burst) && TraitLevelChecked(Traits.EnhancedDreadwyrmTrance))
                    {
                        if (SummonerBurstPhase is 0 or 1 && TraitLevelChecked(Traits.EnhancedBahamut) && CurrentDemiSummon is DemiSummon.SolarBahamut ||
                            SummonerBurstPhase is 0 or 1 && !TraitLevelChecked(Traits.EnhancedBahamut) && CurrentDemiSummon is DemiSummon.Bahamut ||
                            SummonerBurstPhase == 2 && TraitLevelChecked(Traits.EnhancedBahamut) && CurrentDemiSummon is DemiSummon.Bahamut ||
                            SummonerBurstPhase == 2 && CurrentDemiSummon is DemiSummon.Phoenix ||
                            SummonerBurstPhase == 3 && CurrentDemiSummon is not DemiSummon.None ||
                            SummonerBurstPhase == 4)
                            return SearingLight;
                    }
                    else
                        return SearingLight;
                }

                // Energy Drain
                if (IsEnabled(CustomComboPreset.SMN_ST_Advanced_Combo_EDFester) && !Gauge.HasAetherflowStacks && ActionReady(EnergyDrain) &&
                    (!LevelChecked(DreadwyrmTrance) || DemiAttackCount >= burstDelay))
                    return EnergyDrain;

                // First set of Festers if Energy Drain is close to being off CD, or off CD while you have aetherflow stacks.
                if (IsEnabled(CustomComboPreset.SMN_ST_Advanced_Combo_EDFester) && IsEnabled(CustomComboPreset.SMN_ST_Advanced_Combo_DemiEgiMenu_oGCDPooling) && ActionReady(Fester) && GetCooldown(EnergyDrain).CooldownRemaining <= 3.2 &&
                    ((HasStatusEffect(Buffs.SearingLight) && IsNotEnabled(CustomComboPreset.SMN_ST_Advanced_Combo_Burst_Any_Option)) || HasStatusEffect(Buffs.SearingLight, anyOwner: true)) &&
                         (SummonerBurstPhase is not 4) ||
                        (SummonerBurstPhase == 4 && !HasStatusEffect(Buffs.TitansFavor)))
                    return OriginalHook(Fester);

                if (IsEnabled(CustomComboPreset.SMN_ST_Advanced_Combo_SearingFlash) && HasStatusEffect(Buffs.RubysGlimmer) && LevelChecked(SearingFlash))
                    return SearingFlash;

                // Demi Nuke
                if (CurrentDemiSummon is not DemiSummon.None && IsEnabled(CustomComboPreset.SMN_ST_Advanced_Combo_DemiSummons_Attacks) && DemiAttackCount >= burstDelay && (IsNotEnabled(CustomComboPreset.SMN_ST_Advanced_Combo_SearingLight_Burst) || HasStatusEffect(Buffs.SearingLight) || searingInSummon))
                {
                    if (ActionReady(OriginalHook(EnkindleBahamut)))
                        return OriginalHook(EnkindleBahamut);

                    if (ActionReady(AstralFlow) && ((IsEnabled(CustomComboPreset.SMN_ST_Advanced_Combo_DemiSummons_Rekindle) && CurrentDemiSummon is DemiSummon.Phoenix) || CurrentDemiSummon is not DemiSummon.Phoenix))
                        return OriginalHook(AstralFlow);
                }

                // Lux Solaris
                if (ActionReady(LuxSolaris) && IsEnabled(CustomComboPreset.SMN_ST_Advanced_Combo_DemiSummons_LuxSolaris) &&
                    (PlayerHealthPercentageHp() < 100 || (GetStatusEffectRemainingTime(Buffs.RefulgentLux) is < 3 and > 0)))
                    return OriginalHook(LuxSolaris);

                // Fester
                if (IsEnabled(CustomComboPreset.SMN_ST_Advanced_Combo_EDFester))
                {
                    if (ActionReady(Fester))
                    {
                        if (IsNotEnabled(CustomComboPreset.SMN_ST_Advanced_Combo_DemiEgiMenu_oGCDPooling))
                            return OriginalHook(Fester);

                        if (!LevelChecked(SearingLight))
                            return OriginalHook(Fester);

                        if ((((HasStatusEffect(Buffs.SearingLight) && IsNotEnabled(CustomComboPreset.SMN_ST_Advanced_Combo_Burst_Any_Option)) || HasStatusEffect(Buffs.SearingLight, anyOwner: true)) &&
                             SummonerBurstPhase is 0 or 1 or 2 or 3 && DemiAttackCount >= burstDelay) ||
                            (SummonerBurstPhase == 4 && !HasStatusEffect(Buffs.TitansFavor)))
                            return OriginalHook(Fester);

                    }
                }
                // Self Shield Overcap
                if (IsEnabled(CustomComboPreset.SMN_ST_Advanced_Combo_Radiant) && !HasStatusEffect(Buffs.SearingLight) && GetRemainingCharges(RadiantAegis) == 2 && ActionReady(RadiantAegis))
                    return RadiantAegis;

                // Lucid Dreaming
                if (IsEnabled(CustomComboPreset.SMN_ST_Advanced_Combo_Lucid) && Role.CanLucidDream(lucidThreshold))
                    return Role.LucidDreaming;
            }

            #endregion

            // Demi
            if (IsEnabled(CustomComboPreset.SMN_ST_Advanced_Combo_DemiSummons) && PartyInCombat() && ActionReady(OriginalHook(Aethercharge)))
                return OriginalHook(Aethercharge);

            #region Titan Phase
            if (IsTitanAttuned || OriginalHook(AstralFlow) is MountainBuster) //Titan attunement ends before last mountian buster
            {
                if (TitanAstralFlow && ActionReady(AstralFlow) && CanSpellWeave())
                    return OriginalHook(AstralFlow);

                if (IsEnabled(CustomComboPreset.SMN_ST_Advanced_Combo_EgiSummons_Attacks) && GemshineReady)
                    return OriginalHook(Gemshine);
            }
            #endregion

            #region Garuda Phase
            if (IsGarudaAttuned || OriginalHook(AstralFlow) is Slipstream)
            {               
                if (GarudaAstralFlow && HasStatusEffect(Buffs.GarudasFavor))
                {
                    if (IsEnabled(CustomComboPreset.SMN_ST_Advanced_Combo_DemiEgiMenu_SwiftcastEgi) && swiftcastPhase is 1 or 3 && Role.CanSwiftcast()) // Forced Swiftcast option
                        return Role.Swiftcast;
                    
                    if (!IsMoving() || HasStatusEffect(Role.Buffs.Swiftcast))
                        return OriginalHook(AstralFlow);                    
                }

                #region Special Ruin 3 rule lvl 54 - 72
                // Use Ruin III instead of Emerald Ruin III if enabled and Ruin Mastery III is not active
                if (IsEnabled(CustomComboPreset.SMN_ST_Ruin3_Emerald_Ruin3) && !TraitLevelChecked(Traits.RuinMastery3) && LevelChecked(Ruin3) && !IsMoving())                
                    return Ruin3;
               
                #endregion

                if (IsEnabled(CustomComboPreset.SMN_ST_Advanced_Combo_EgiSummons_Attacks) && GemshineReady)
                    return OriginalHook(Gemshine);

                if (IsEnabled(CustomComboPreset.SMN_ST_Advanced_Combo_Ruin4) && ActionReady(Ruin4) && IsMoving())
                    return Ruin4;
            }
            #endregion

            #region Ifrit Phase

            if (IsIfritAttuned || OriginalHook(AstralFlow) is CrimsonCyclone or CrimsonStrike)
            {
                if (IsEnabled(CustomComboPreset.SMN_ST_Advanced_Combo_DemiEgiMenu_SwiftcastEgi) && swiftcastPhase is 2 or 3 && Role.CanSwiftcast())
                    return Role.Swiftcast;

                if (IsEnabled(CustomComboPreset.SMN_ST_Advanced_Combo_EgiSummons_Attacks) && GemshineReady && 
                    (!IsMoving() || HasStatusEffect(Role.Buffs.Swiftcast)))
                    return OriginalHook(Gemshine);

                if (IfritAstralFlowCyclone && HasStatusEffect(Buffs.IfritsFavor) &&
                   ((!Config.SMN_ST_CrimsonCycloneMelee) || (Config.SMN_ST_CrimsonCycloneMelee && InMeleeRange()))  //Melee Check
                   || (IfritAstralFlowStrike && HasStatusEffect(Buffs.CrimsonStrike) && InMeleeRange())) //After Strike
                    return OriginalHook(AstralFlow);

                if (IsEnabled(CustomComboPreset.SMN_ST_Advanced_Combo_Ruin4) && ActionReady(Ruin4) && !HasStatusEffect(Role.Buffs.Swiftcast))
                    return Ruin4;
            }
            #endregion

            #region Egi Priority

            foreach (var prio in Config.SMN_ST_Egi_Priority.Items.OrderBy(x => x))
            {
                var index = Config.SMN_ST_Egi_Priority.IndexOf(prio);
                var config = GetMatchingConfigST(index, OptionalTarget,
                    out var spell, out var enabled);

                if (!enabled) continue;

                if (!ActionReady(OriginalHook(Aethercharge)) && Gauge.SummonTimerRemaining == 0 && Gauge.AttunementTimerRemaining == 0)
                    return spell;
            }

            #endregion

            // Ruin 4 Dump

            if (IsEnabled(CustomComboPreset.SMN_ST_Advanced_Combo_Ruin4) && LevelChecked(Ruin4) && !IsAttunedAny  && CurrentDemiSummon is DemiSummon.None && HasStatusEffect(Buffs.FurtherRuin))
                return Ruin4;

            return actionID;
        }
    }

    internal class SMN_Advanced_Combo_AoE : CustomCombo
    {
        internal static int DemiAttackCount = 0;
        protected internal override CustomComboPreset Preset { get; } = CustomComboPreset.SMN_AoE_Advanced_Combo;

        protected override uint Invoke(uint actionID)
        {
            if (actionID is not (Outburst or Tridisaster))
                return actionID;

            #region Variables

            int SummonerBurstPhase = Config.SMN_AoE_BurstPhase;
            int lucidThreshold = Config.SMN_AoE_Lucid;
            int swiftcastPhase = Config.SMN_AoE_SwiftcastPhase;
            int burstDelay = IsEnabled(CustomComboPreset.SMN_AoE_Advanced_Combo_DemiEgiMenu_oGCDPooling) ? Config.SMN_AoE_Burst_Delay : 0;

            bool TitanAstralFlow = IsEnabled(CustomComboPreset.SMN_AoE_Advanced_Combo_Egi_AstralFlow) && Config.SMN_AoE_Egi_AstralFlow[0];
            bool IfritAstralFlowCyclone = IsEnabled(CustomComboPreset.SMN_AoE_Advanced_Combo_Egi_AstralFlow) && Config.SMN_AoE_Egi_AstralFlow[1];
            bool IfritAstralFlowStrike = IsEnabled(CustomComboPreset.SMN_AoE_Advanced_Combo_Egi_AstralFlow) && Config.SMN_AoE_Egi_AstralFlow[3];
            bool GarudaAstralFlow = IsEnabled(CustomComboPreset.SMN_AoE_Advanced_Combo_Egi_AstralFlow) && Config.SMN_AoE_Egi_AstralFlow[2];

            var searingInSummon = GetCooldownRemainingTime(SearingLight) > (Gauge.SummonTimerRemaining / 1000f) + GCDTotal;

            DemiAttackCount = CurrentDemiSummon is not DemiSummon.None ? TimesUsedSinceOtherAction(OriginalHook(Aethercharge), [AstralImpulse, UmbralImpulse, FountainOfFire, AstralFlare, UmbralFlare, BrandOfPurgatory]) : 0;
            #endregion

            #region Variant

            if (Variant.CanCure(CustomComboPreset.SMN_Variant_Cure, Config.SMN_VariantCure))
                return Variant.Cure;

            if (Variant.CanRampart(CustomComboPreset.SMN_Variant_Rampart, WeaveTypes.SpellWeave))
                return Variant.Rampart;

            if (NeedToSummon && ActionReady(SummonCarbuncle))
                return SummonCarbuncle;

            #endregion

            #region OGCDS

            // Emergency priority Demi Nuke to prevent waste if you can't get demi attacks out to satisfy the slider check.
            if (CurrentDemiSummon is not DemiSummon.None && IsEnabled(CustomComboPreset.SMN_AoE_Advanced_Combo_DemiSummons_Attacks) && Gauge.SummonTimerRemaining <= 2500)
            {
                if (IsEnabled(CustomComboPreset.SMN_AoE_Advanced_Combo_DemiSummons_Attacks))
                {
                    if (ActionReady(OriginalHook(EnkindleBahamut)))
                        return OriginalHook(EnkindleBahamut);

                    if (ActionReady(AstralFlow) && ((IsEnabled(CustomComboPreset.SMN_AoE_Advanced_Combo_DemiSummons_Rekindle) && CurrentDemiSummon is DemiSummon.Phoenix) || CurrentDemiSummon is not DemiSummon.Phoenix))
                        return OriginalHook(AstralFlow);
                }
            }

            if (CanSpellWeave())
            {
                // Searing Light
                if (IsEnabled(CustomComboPreset.SMN_AoE_Advanced_Combo_SearingLight) && IsOffCooldown(SearingLight) && LevelChecked(SearingLight) && ((!HasStatusEffect(Buffs.SearingLight, anyOwner: true) && Config.SMN_AoE_Searing_Any) || !Config.SMN_AoE_Searing_Any))
                {
                    if (IsEnabled(CustomComboPreset.SMN_AoE_Advanced_Combo_SearingLight_Burst) && TraitLevelChecked(Traits.EnhancedDreadwyrmTrance))
                    {
                        if (SummonerBurstPhase is 0 or 1 && TraitLevelChecked(Traits.EnhancedBahamut) && CurrentDemiSummon is DemiSummon.SolarBahamut ||
                            SummonerBurstPhase is 0 or 1 && !TraitLevelChecked(Traits.EnhancedBahamut) && CurrentDemiSummon is DemiSummon.Bahamut ||
                            SummonerBurstPhase == 2 && TraitLevelChecked(Traits.EnhancedBahamut) && CurrentDemiSummon is DemiSummon.Bahamut ||
                            SummonerBurstPhase == 2 && CurrentDemiSummon is DemiSummon.Phoenix ||
                            SummonerBurstPhase == 3 && CurrentDemiSummon is not DemiSummon.None ||
                            SummonerBurstPhase == 4)
                            return SearingLight;
                    }
                    else
                        return SearingLight;
                }

                // Energy Drain
                if (IsEnabled(CustomComboPreset.SMN_AoE_Advanced_Combo_ESPainflare) && !Gauge.HasAetherflowStacks && ActionReady(EnergyDrain) &&
                    (!LevelChecked(DreadwyrmTrance) || DemiAttackCount >= burstDelay))
                {
                    if (!LevelChecked(EnergySiphon))
                        return EnergyDrain;
                    else
                        return EnergySiphon;
                }
                    

                // First set of Painflares if Energy Drain is close to being off CD, or off CD while you have aetherflow stacks.
                if (IsEnabled(CustomComboPreset.SMN_AoE_Advanced_Combo_ESPainflare) && IsEnabled(CustomComboPreset.SMN_AoE_Advanced_Combo_DemiEgiMenu_oGCDPooling) && ActionReady(Painflare) && GetCooldown(EnergyDrain).CooldownRemaining <= 3.2 &&
                    ((HasStatusEffect(Buffs.SearingLight) && IsNotEnabled(CustomComboPreset.SMN_AoE_Advanced_Combo_Burst_Any_Option)) || HasStatusEffect(Buffs.SearingLight, anyOwner: true)) &&
                         (SummonerBurstPhase is not 4) ||
                        (SummonerBurstPhase == 4 && !HasStatusEffect(Buffs.TitansFavor)))
                    return OriginalHook(Painflare);

                if (IsEnabled(CustomComboPreset.SMN_AoE_Advanced_Combo_SearingFlash) && HasStatusEffect(Buffs.RubysGlimmer) && LevelChecked(SearingFlash))
                    return SearingFlash;

                // Demi Nuke
                if (CurrentDemiSummon is not DemiSummon.None && IsEnabled(CustomComboPreset.SMN_AoE_Advanced_Combo_DemiSummons_Attacks) && DemiAttackCount >= burstDelay && (IsNotEnabled(CustomComboPreset.SMN_AoE_Advanced_Combo_SearingLight_Burst) || HasStatusEffect(Buffs.SearingLight) || searingInSummon))
                {
                    if (ActionReady(OriginalHook(EnkindleBahamut)))
                        return OriginalHook(EnkindleBahamut);

                    if (ActionReady(AstralFlow) && ((IsEnabled(CustomComboPreset.SMN_AoE_Advanced_Combo_DemiSummons_Rekindle) && CurrentDemiSummon is DemiSummon.Phoenix) || CurrentDemiSummon is not DemiSummon.Phoenix))
                        return OriginalHook(AstralFlow);
                }

                // Lux Solaris
                if (ActionReady(LuxSolaris) && IsEnabled(CustomComboPreset.SMN_ST_Advanced_Combo_DemiSummons_LuxSolaris) &&
                    (PlayerHealthPercentageHp() < 100 || (GetStatusEffectRemainingTime(Buffs.RefulgentLux) is < 3 and > 0)))
                    return OriginalHook(LuxSolaris);

                // Painflare
                if (IsEnabled(CustomComboPreset.SMN_AoE_Advanced_Combo_ESPainflare))
                {
                    if (!LevelChecked(Painflare) && ActionReady(Fester))
                         return OriginalHook(Fester); 

                    if (ActionReady(Painflare))
                    {
                        if (IsNotEnabled(CustomComboPreset.SMN_AoE_Advanced_Combo_DemiEgiMenu_oGCDPooling) || !LevelChecked(SearingLight))
                            return OriginalHook(Painflare);

                        if ((((HasStatusEffect(Buffs.SearingLight) && IsNotEnabled(CustomComboPreset.SMN_AoE_Advanced_Combo_Burst_Any_Option)) || HasStatusEffect(Buffs.SearingLight, anyOwner: true)) &&
                             SummonerBurstPhase is 0 or 1 or 2 or 3 && DemiAttackCount >= burstDelay) ||
                            (SummonerBurstPhase == 4 && !HasStatusEffect(Buffs.TitansFavor)))
                            return OriginalHook(Painflare);
                    }
                    
                }

                // Self Shield Overcap
                if (IsEnabled(CustomComboPreset.SMN_AoE_Advanced_Combo_Radiant) && !HasStatusEffect(Buffs.SearingLight) && GetRemainingCharges(RadiantAegis) == 2 && ActionReady(RadiantAegis))
                    return RadiantAegis;

                // Lucid Dreaming
                if (IsEnabled(CustomComboPreset.SMN_AoE_Advanced_Combo_Lucid) && Role.CanLucidDream(lucidThreshold))
                    return Role.LucidDreaming;
            }

            #endregion

            // Demi
            if (IsEnabled(CustomComboPreset.SMN_AoE_Advanced_Combo_DemiSummons) && PartyInCombat() && ActionReady(OriginalHook(Aethercharge)))
                return OriginalHook(Aethercharge);

            #region Titan Phase

            if (IsTitanAttuned || OriginalHook(AstralFlow) is MountainBuster) //Titan attunement ends before last mountian buster
            {
                if (TitanAstralFlow && ActionReady(AstralFlow) && CanSpellWeave())
                    return OriginalHook(AstralFlow);

                if (IsEnabled(CustomComboPreset.SMN_AoE_Advanced_Combo_EgiSummons_Attacks) && GemshineReady)
                    return OriginalHook(PreciousBrilliance);

            }
            #endregion

            #region Garuda Phase
            if (IsGarudaAttuned || OriginalHook(AstralFlow) is Slipstream)
            {
                if (GarudaAstralFlow && HasStatusEffect(Buffs.GarudasFavor))
                {
                    if (IsEnabled(CustomComboPreset.SMN_AoE_Advanced_Combo_DemiEgiMenu_SwiftcastEgi) && swiftcastPhase is 1 or 3 && Role.CanSwiftcast()) // Forced Swiftcast option
                        return Role.Swiftcast;
                   
                    if (!IsMoving() || HasStatusEffect(Role.Buffs.Swiftcast))
                        return OriginalHook(AstralFlow);
                }                

                if (IsEnabled(CustomComboPreset.SMN_AoE_Advanced_Combo_EgiSummons_Attacks) && GemshineReady)
                    return OriginalHook(PreciousBrilliance);

                if (IsEnabled(CustomComboPreset.SMN_AoE_Advanced_Combo_Ruin4) && ActionReady(Ruin4) && IsMoving())
                    return Ruin4;
            }

            #endregion

            #region Ifrit Phase
            if (IsIfritAttuned || OriginalHook(AstralFlow) is CrimsonCyclone or CrimsonStrike)
            {
                if (IsEnabled(CustomComboPreset.SMN_AoE_Advanced_Combo_DemiEgiMenu_SwiftcastEgi) && swiftcastPhase is 2 or 3 && (Role.CanSwiftcast()))
                    return Role.Swiftcast;

                if (IsEnabled(CustomComboPreset.SMN_AoE_Advanced_Combo_EgiSummons_Attacks) && GemshineReady && 
                    (!IsMoving() || HasStatusEffect(Role.Buffs.Swiftcast)))
                    return OriginalHook(PreciousBrilliance);

                if (IfritAstralFlowCyclone && HasStatusEffect(Buffs.IfritsFavor) &&
                   ((!Config.SMN_AoE_CrimsonCycloneMelee) || (Config.SMN_AoE_CrimsonCycloneMelee && InMeleeRange()))  //Melee Check
                   || (IfritAstralFlowStrike && HasStatusEffect(Buffs.CrimsonStrike) && InMeleeRange())) //After Strike
                    return OriginalHook(AstralFlow);

                if (IsEnabled(CustomComboPreset.SMN_AoE_Advanced_Combo_Ruin4) && ActionReady(Ruin4) && !HasStatusEffect(Role.Buffs.Swiftcast))
                    return Ruin4;
            }
            #endregion

            #region Egi Priority

            foreach (var prio in Config.SMN_AoE_Egi_Priority.Items.OrderBy(x => x))
            {
                var index = Config.SMN_AoE_Egi_Priority.IndexOf(prio);
                var config = GetMatchingConfigAoE(index, OptionalTarget,
                    out var spell, out var enabled);

                if (!enabled) continue;

                if (!ActionReady(OriginalHook(Aethercharge)) && Gauge.SummonTimerRemaining == 0 && Gauge.AttunementTimerRemaining == 0)
                    return spell;
            }

            #endregion

            // Ruin 4 Dump
            if (IsEnabled(CustomComboPreset.SMN_AoE_Advanced_Combo_Ruin4) && LevelChecked(Ruin4) && !IsAttunedAny && CurrentDemiSummon is DemiSummon.None && HasStatusEffect(Buffs.FurtherRuin))
                return Ruin4;

            return actionID;
        }
    }
    #endregion
}
