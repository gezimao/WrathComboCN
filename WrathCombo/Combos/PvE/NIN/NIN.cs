using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.ClientState.JobGauge.Types;
using Dalamud.Game.ClientState.Statuses;
using WrathCombo.Combos.PvE.Content;
using WrathCombo.CustomComboNS;
using WrathCombo.Data;
using WrathCombo.Extensions;
namespace WrathCombo.Combos.PvE;

internal partial class NIN : Melee
{
    internal class NIN_ST_AeolianEdgeCombo : CustomCombo
    {
        protected internal override CustomComboPreset Preset { get; } = CustomComboPreset.NIN_ST_AeolianEdgeCombo;

        protected override uint Invoke(uint actionID)
        {
            if (actionID is not AeolianEdge)
                return actionID;

            if (ComboTimer > 0)
            {
                if (ComboAction is SpinningEdge && LevelChecked(GustSlash))
                    return GustSlash;

                if (ComboAction is GustSlash && LevelChecked(AeolianEdge))
                    return AeolianEdge;
            }

            return SpinningEdge;
        }
    }

    internal class NIN_ST_AdvancedMode : CustomCombo
    {
        protected internal static NINOpenerMaxLevel4thGCDKunai NINOpener = new();

        protected internal MudraCasting MudraState = new();
        protected internal override CustomComboPreset Preset { get; } = CustomComboPreset.NIN_ST_AdvancedMode;

        protected override uint Invoke(uint actionID)
        {
            if (actionID is not SpinningEdge)
                return actionID;

            NINGauge gauge = GetJobGauge<NINGauge>();
            bool canWeave = CanWeave();
            bool canDelayedWeave = CanDelayedWeave();
            bool inTrickBurstSaveWindow = IsEnabled(CustomComboPreset.NIN_ST_AdvancedMode_TrickAttack_Cooldowns) && IsEnabled(CustomComboPreset.NIN_ST_AdvancedMode_TrickAttack) && GetCooldownRemainingTime(TrickAttack) <= Config.Advanced_Trick_Cooldown;
            bool useBhakaBeforeTrickWindow = GetCooldownRemainingTime(TrickAttack) >= 3;
            bool setupSuitonWindow = GetCooldownRemainingTime(OriginalHook(TrickAttack)) <= Config.Trick_CooldownRemaining && !HasStatusEffect(Buffs.ShadowWalker);
            bool setupKassatsuWindow = GetCooldownRemainingTime(TrickAttack) <= 10 && HasStatusEffect(Buffs.ShadowWalker);
            bool chargeCheck = IsNotEnabled(CustomComboPreset.NIN_ST_AdvancedMode_Ninjitsus_ChargeHold) || IsEnabled(CustomComboPreset.NIN_ST_AdvancedMode_Ninjitsus_ChargeHold) && (InMudra || GetRemainingCharges(Ten) == 2 || GetRemainingCharges(Ten) == 1 && GetCooldownChargeRemainingTime(Ten) < 3);
            bool poolCharges = !(bool)Config.Advanced_ChargePool || GetRemainingCharges(Ten) == 1 && GetCooldownChargeRemainingTime(Ten) < 2 || TrickDebuff || InMudra;
            bool raitonUptime = IsEnabled(CustomComboPreset.NIN_ST_AdvancedMode_Raiton_Uptime);
            bool suitonUptime = IsEnabled(CustomComboPreset.NIN_ST_AdvancedMode_Suiton_Uptime);
            int bhavaPool = Config.Ninki_BhavaPooling;
            int bunshinPool = Config.Ninki_BunshinPoolingST;
            int burnKazematoi = Config.BurnKazematoi;
            int secondWindThreshold = Config.SecondWindThresholdST;
            int shadeShiftThreshold = Config.ShadeShiftThresholdST;
            int bloodbathThreshold = Config.BloodbathThresholdST;
            double playerHP = PlayerHealthPercentageHp();
            bool phantomUptime = IsEnabled(CustomComboPreset.NIN_ST_AdvancedMode_Phantom_Uptime);
            bool trueNorthArmor = IsEnabled(CustomComboPreset.NIN_ST_AdvancedMode_TrueNorth) && Role.CanTrueNorth() && !OnTargetsFlank();
            bool trueNorthEdge = IsEnabled(CustomComboPreset.NIN_ST_AdvancedMode_TrueNorth) && Role.CanTrueNorth() && !OnTargetsRear();
            bool dynamic = Config.Advanced_TrueNorth == 0;

            if (IsEnabled(CustomComboPreset.NIN_ST_AdvancedMode_BalanceOpener) && 
                Opener().FullOpener(ref actionID))
                return actionID;

            if (IsNotEnabled(CustomComboPreset.NIN_ST_AdvancedMode_Ninjitsus) || ActionWatching.TimeSinceLastAction.TotalSeconds >= 5 && !InCombat())
                MudraState.CurrentMudra = MudraCasting.MudraState.None;

            if (IsEnabled(CustomComboPreset.NIN_ST_AdvancedMode_Ninjitsus_Suiton) && IsOnCooldown(TrickAttack) && MudraState.CurrentMudra == MudraCasting.MudraState.CastingSuiton && !setupSuitonWindow)
                MudraState.CurrentMudra = MudraCasting.MudraState.None;

            if (IsEnabled(CustomComboPreset.NIN_ST_AdvancedMode_Ninjitsus_Suiton) && IsOnCooldown(TrickAttack) && MudraState.CurrentMudra != MudraCasting.MudraState.CastingSuiton && setupSuitonWindow)
                MudraState.CurrentMudra = MudraCasting.MudraState.CastingSuiton;

            if (OriginalHook(Ninjutsu) is Rabbit)
                return OriginalHook(Ninjutsu);

            if (InMudra)
            {
                if (MudraState.ContinueCurrentMudra(ref actionID))
                    return actionID;
            }

            if (!Suiton.LevelChecked()) //For low level
            {
                if (Raiton.LevelChecked() && IsEnabled(CustomComboPreset.NIN_ST_AdvancedMode_Ninjitsus_Raiton)) //under 45 will only use Raiton
                {
                    if (MudraState.CastRaiton(ref actionID))
                        return actionID;
                }
                else if (!Raiton.LevelChecked() && MudraState.CastFumaShuriken(ref actionID) && IsEnabled(CustomComboPreset.NIN_ST_AdvancedMode_Ninjitsus_FumaShuriken)) // 30-35 will use only fuma
                    return actionID;
            }

            if (HasStatusEffect(Buffs.TenChiJin))
            {
                if (OriginalHook(Ten) == TCJFumaShurikenTen)
                    return OriginalHook(Ten);
                if (OriginalHook(Chi) == TCJRaiton)
                    return OriginalHook(Chi);
                if (OriginalHook(Jin) == TCJSuiton)
                    return OriginalHook(Jin);
            }

            if (IsEnabled(CustomComboPreset.NIN_ST_AdvancedMode_Kassatsu_HyoshoRaynryu) &&
                HasStatusEffect(Buffs.Kassatsu) &&
                TrickDebuff &&
                MudraState.CastHyoshoRanryu(ref actionID))
                return actionID;

            if (Variant.CanCure(CustomComboPreset.NIN_Variant_Cure, Config.NIN_VariantCure))
                return Variant.Cure;

            if (OccultCrescent.ShouldUsePhantomActions())
                return OccultCrescent.BestPhantomAction();

            if (InCombat() && !InMeleeRange())
            {
                if (IsEnabled(CustomComboPreset.NIN_ST_AdvancedMode_Bunshin_Phantom) &&
                    HasStatusEffect(Buffs.PhantomReady) &&
                    (GetCooldownRemainingTime(TrickAttack) > GetStatusEffectRemainingTime(Buffs.PhantomReady) || TrickDebuff || HasStatusEffect(Buffs.Bunshin) && MugDebuff) &&
                    PhantomKamaitachi.LevelChecked()
                    && phantomUptime)
                    return OriginalHook(PhantomKamaitachi);

                if (IsEnabled(CustomComboPreset.NIN_ST_AdvancedMode_Ninjitsus_Suiton) &&
                    setupSuitonWindow &&
                    TrickAttack.LevelChecked() &&
                    !HasStatusEffect(Buffs.ShadowWalker) &&
                    chargeCheck &&
                    suitonUptime &&
                    MudraState.CastSuiton(ref actionID))
                    return actionID;

                if (IsEnabled(CustomComboPreset.NIN_ST_AdvancedMode_Ninjitsus_Raiton) &&
                    !inTrickBurstSaveWindow &&
                    chargeCheck &&
                    poolCharges &&
                    raitonUptime &&
                    MudraState.CastRaiton(ref actionID))
                    return actionID;

                if (IsEnabled(CustomComboPreset.NIN_ST_AdvancedMode_RangedUptime) && ThrowingDaggers.LevelChecked() && HasTarget() && !HasStatusEffect(Buffs.RaijuReady))
                    return OriginalHook(ThrowingDaggers);
            }

            if (canWeave && !InMudra)
            {
                if (Variant.CanRampart(CustomComboPreset.NIN_Variant_Rampart))
                    return Variant.Rampart;

                if (IsEnabled(CustomComboPreset.NIN_ST_AdvancedMode_Mug) &&
                    IsEnabled(CustomComboPreset.NIN_ST_AdvancedMode_Mug_AlignBefore) &&
                    HasStatusEffect(Buffs.ShadowWalker) &&
                    GetCooldownRemainingTime(TrickAttack) <= 3 &&
                    (IsEnabled(CustomComboPreset.NIN_ST_AdvancedMode_TrickAttack_Delayed) && InCombat() &&
                     CombatEngageDuration().TotalSeconds > 6 ||
                     IsNotEnabled(CustomComboPreset.NIN_ST_AdvancedMode_TrickAttack_Delayed)) &&
                    IsOffCooldown(Mug) &&
                    canDelayedWeave &&
                    Mug.LevelChecked())
                {
                    if (Dokumori.LevelChecked() && gauge.Ninki >= 60)
                        return OriginalHook(Bhavacakra);
                    return OriginalHook(Mug);
                }

                if (IsEnabled(CustomComboPreset.NIN_ST_AdvancedMode_TrickAttack) &&
                    HasStatusEffect(Buffs.ShadowWalker) &&
                    IsOffCooldown(TrickAttack) &&
                    canDelayedWeave &&
                    (IsEnabled(CustomComboPreset.NIN_ST_AdvancedMode_TrickAttack_Delayed) && InCombat() && CombatEngageDuration().TotalSeconds > 8 ||
                     IsNotEnabled(CustomComboPreset.NIN_ST_AdvancedMode_TrickAttack_Delayed)))
                    return OriginalHook(TrickAttack);

                if (IsEnabled(CustomComboPreset.NIN_ST_AdvancedMode_TenriJindo) && HasStatusEffect(Buffs.TenriJendo) && (TrickDebuff && MugDebuff || GetStatusEffectRemainingTime(Buffs.TenriJendo) <= 3))
                    return OriginalHook(TenriJendo);

                if (IsEnabled(CustomComboPreset.NIN_ST_AdvancedMode_Bunshin) && Bunshin.LevelChecked() && IsOffCooldown(Bunshin) && gauge.Ninki >= bunshinPool)
                    return OriginalHook(Bunshin);

                if (IsEnabled(CustomComboPreset.NIN_ST_AdvancedMode_Kassatsu) && (TrickDebuff || setupKassatsuWindow) && IsOffCooldown(Kassatsu) && Kassatsu.LevelChecked())
                    return OriginalHook(Kassatsu);

                //healing - please move if not appropriate priority
                if (IsEnabled(CustomComboPreset.NIN_ST_AdvancedMode_SecondWind) && Role.CanSecondWind(secondWindThreshold))
                    return Role.SecondWind;

                if (IsEnabled(CustomComboPreset.NIN_ST_AdvancedMode_ShadeShift) && ShadeShift.LevelChecked() && playerHP <= shadeShiftThreshold && IsOffCooldown(ShadeShift))
                    return ShadeShift;

                if (IsEnabled(CustomComboPreset.NIN_ST_AdvancedMode_Bloodbath) && Role.CanBloodBath(bloodbathThreshold))
                    return Role.Bloodbath;

                if (IsEnabled(CustomComboPreset.NIN_ST_AdvancedMode_Bhavacakra) &&
                    (TrickDebuff && gauge.Ninki >= 50 || useBhakaBeforeTrickWindow && gauge.Ninki >= 85) &&
                    (IsNotEnabled(CustomComboPreset.NIN_ST_AdvancedMode_Mug) || IsEnabled(CustomComboPreset.NIN_ST_AdvancedMode_Mug) && IsOnCooldown(Mug)) &&
                    Bhavacakra.LevelChecked())
                    return OriginalHook(Bhavacakra);

                if (IsEnabled(CustomComboPreset.NIN_ST_AdvancedMode_Bhavacakra) &&
                    (TrickDebuff && gauge.Ninki >= 50 || useBhakaBeforeTrickWindow && gauge.Ninki >= 60) &&
                    (IsNotEnabled(CustomComboPreset.NIN_ST_AdvancedMode_Mug) || IsEnabled(CustomComboPreset.NIN_ST_AdvancedMode_Mug) && IsOnCooldown(Mug)) &&
                    !Bhavacakra.LevelChecked() && Hellfrog.LevelChecked())
                    return OriginalHook(Hellfrog);

                if (!inTrickBurstSaveWindow)
                {
                    if (IsEnabled(CustomComboPreset.NIN_ST_AdvancedMode_Mug) && IsOffCooldown(Mug) && Mug.LevelChecked())
                    {
                        if (IsNotEnabled(CustomComboPreset.NIN_ST_AdvancedMode_Mug_AlignAfter) || IsEnabled(CustomComboPreset.NIN_ST_AdvancedMode_Mug_AlignAfter) && TrickDebuff)
                            return OriginalHook(Mug);
                    }

                    if (IsEnabled(CustomComboPreset.NIN_ST_AdvancedMode_Meisui) && HasStatusEffect(Buffs.ShadowWalker) && gauge.Ninki <= 50 && IsOffCooldown(Meisui) && Meisui.LevelChecked())
                        return OriginalHook(Meisui);

                    if (IsEnabled(CustomComboPreset.NIN_ST_AdvancedMode_Bhavacakra) && gauge.Ninki >= bhavaPool && Bhavacakra.LevelChecked())
                        return OriginalHook(Bhavacakra);

                    if (IsEnabled(CustomComboPreset.NIN_ST_AdvancedMode_Bhavacakra) && gauge.Ninki >= bhavaPool && !Bhavacakra.LevelChecked() && Hellfrog.LevelChecked())
                        return OriginalHook(Hellfrog);

                    if (IsEnabled(CustomComboPreset.NIN_ST_AdvancedMode_AssassinateDWAD) && IsOffCooldown(OriginalHook(Assassinate)) && Assassinate.LevelChecked())
                        return OriginalHook(Assassinate);

                    if (IsEnabled(CustomComboPreset.NIN_ST_AdvancedMode_TCJ) && IsOffCooldown(TenChiJin) && TenChiJin.LevelChecked())
                        return OriginalHook(TenChiJin);
                }

                if (IsEnabled(CustomComboPreset.NIN_ST_AdvancedMode_SecondWind) && Role.CanSecondWind(secondWindThreshold))
                    return Role.SecondWind;

                if (IsEnabled(CustomComboPreset.NIN_ST_AdvancedMode_ShadeShift) && ShadeShift.LevelChecked() && playerHP <= shadeShiftThreshold && IsOffCooldown(ShadeShift))
                    return ShadeShift;

                if (IsEnabled(CustomComboPreset.NIN_ST_AdvancedMode_Bloodbath) && Role.CanBloodBath(bloodbathThreshold))
                    return Role.Bloodbath;
            }

            if (IsEnabled(CustomComboPreset.NIN_ST_AdvancedMode_Raiju) && HasStatusEffect(Buffs.RaijuReady))
            {
                if (IsEnabled(CustomComboPreset.NIN_ST_AdvancedMode_Raiju_Forked) && !InMeleeRange())
                    return OriginalHook(ForkedRaiju);
                return OriginalHook(FleetingRaiju);
            }

            if (IsEnabled(CustomComboPreset.NIN_ST_AdvancedMode_Kassatsu_HyoshoRaynryu) &&
                !inTrickBurstSaveWindow &&
                (IsNotEnabled(CustomComboPreset.NIN_ST_AdvancedMode_Mug) || IsEnabled(CustomComboPreset.NIN_ST_AdvancedMode_Mug) && IsOnCooldown(Mug)) &&
                MudraState.CastHyoshoRanryu(ref actionID))
                return actionID;

            if (IsEnabled(CustomComboPreset.NIN_ST_AdvancedMode_Ninjitsus))
            {
                if (IsEnabled(CustomComboPreset.NIN_ST_AdvancedMode_Ninjitsus_Suiton) &&
                    setupSuitonWindow &&
                    TrickAttack.LevelChecked() &&
                    !HasStatusEffect(Buffs.ShadowWalker) &&
                    chargeCheck &&
                    MudraState.CastSuiton(ref actionID))
                    return actionID;

                if (IsEnabled(CustomComboPreset.NIN_ST_AdvancedMode_Ninjitsus_Raiton) &&
                    !inTrickBurstSaveWindow &&
                    chargeCheck &&
                    poolCharges &&
                    MudraState.CastRaiton(ref actionID))
                    return actionID;

                if (IsEnabled(CustomComboPreset.NIN_ST_AdvancedMode_Ninjitsus_FumaShuriken) &&
                    !Raiton.LevelChecked() &&
                    chargeCheck &&
                    MudraState.CastFumaShuriken(ref actionID))
                    return actionID;
            }

            if (IsEnabled(CustomComboPreset.NIN_ST_AdvancedMode_Bunshin_Phantom) &&
                HasStatusEffect(Buffs.PhantomReady) &&
                (GetCooldownRemainingTime(TrickAttack) > GetStatusEffectRemainingTime(Buffs.PhantomReady) || TrickDebuff || HasStatusEffect(Buffs.Bunshin) && MugDebuff || GetStatusEffectRemainingTime(Buffs.PhantomReady) < 6) &&
                PhantomKamaitachi.LevelChecked())
                return OriginalHook(PhantomKamaitachi);

            if (ComboTimer > 1f)
            {
                if (ComboAction == SpinningEdge && GustSlash.LevelChecked())
                    return OriginalHook(GustSlash);

                if (ComboAction == GustSlash && ArmorCrush.LevelChecked())
                {
                    if (gauge.Kazematoi == 0)
                    {
                        if (trueNorthArmor)
                            return Role.TrueNorth;

                        return ArmorCrush;
                    }

                    if (GetTargetHPPercent() <= burnKazematoi && gauge.Kazematoi > 0)
                    {
                        if (trueNorthEdge)
                            return Role.TrueNorth;

                        return AeolianEdge;
                    }

                    if (dynamic)
                    {
                        if (gauge.Kazematoi >= 4)
                        {
                            if (trueNorthEdge)
                                return Role.TrueNorth;

                            return AeolianEdge;
                        }

                        if (OnTargetsFlank())
                            return ArmorCrush;
                        return AeolianEdge;
                    }
                    if (gauge.Kazematoi < 3)
                    {
                        if (trueNorthArmor)
                            return Role.TrueNorth;

                        return ArmorCrush;
                    }

                    return AeolianEdge;
                }
                if (ComboAction == GustSlash && !ArmorCrush.LevelChecked() && AeolianEdge.LevelChecked())
                {
                    if (trueNorthEdge)
                        return OriginalHook(Role.TrueNorth);
                    return OriginalHook(AeolianEdge);
                }
            }
            return OriginalHook(SpinningEdge);
        }
    }

    internal class NIN_AoE_AdvancedMode : CustomCombo
    {
        protected internal MudraCasting MudraState = new();
        protected internal override CustomComboPreset Preset { get; } = CustomComboPreset.NIN_AoE_AdvancedMode;

        protected override uint Invoke(uint actionID)
        {
            if (actionID is not DeathBlossom)
                return actionID;

            Status? dotonBuff = GetStatusEffect(Buffs.Doton);
            NINGauge gauge = GetJobGauge<NINGauge>();
            bool canWeave = CanWeave();
            bool chargeCheck = IsNotEnabled(CustomComboPreset.NIN_AoE_AdvancedMode_Ninjitsus_ChargeHold) || IsEnabled(CustomComboPreset.NIN_AoE_AdvancedMode_Ninjitsus_ChargeHold) && GetRemainingCharges(Ten) == 2;
            bool inMudraState = InMudra;
            int hellfrogPool = Config.Ninki_HellfrogPooling;
            int dotonTimer = Config.Advanced_DotonTimer;
            int dotonThreshold = Config.Advanced_DotonHP;
            int tcjPath = Config.Advanced_TCJEnderAoE;
            int bunshingPool = Config.Ninki_BunshinPoolingAoE;
            int secondWindThreshold = Config.SecondWindThresholdAoE;
            int shadeShiftThreshold = Config.ShadeShiftThresholdAoE;
            int bloodbathThreshold = Config.BloodbathThresholdAoE;
            double playerHP = PlayerHealthPercentageHp();

            if (IsNotEnabled(CustomComboPreset.NIN_AoE_AdvancedMode_Ninjitsus) || ActionWatching.TimeSinceLastAction.TotalSeconds >= 5 && !InCombat())
                MudraState.CurrentMudra = MudraCasting.MudraState.None;

            if (OriginalHook(Ninjutsu) is Rabbit)
                return OriginalHook(Ninjutsu);

            if (InMudra)
            {
                if (MudraState.ContinueCurrentMudra(ref actionID))
                    return actionID;
            }

            if (HasStatusEffect(Buffs.TenChiJin))
            {
                if (tcjPath == 0)
                {
                    if (OriginalHook(Chi) == TCJFumaShurikenChi)
                        return OriginalHook(Chi);
                    if (OriginalHook(Ten) == TCJKaton)
                        return OriginalHook(Ten);
                    if (OriginalHook(Jin) == TCJSuiton)
                        return OriginalHook(Jin);
                }
                else
                {
                    if (OriginalHook(Jin) == TCJFumaShurikenJin)
                        return OriginalHook(Jin);
                    if (OriginalHook(Ten) == TCJKaton)
                        return OriginalHook(Ten);
                    if (OriginalHook(Chi) == TCJDoton)
                        return OriginalHook(Chi);
                }
            }

            if (IsEnabled(CustomComboPreset.NIN_AoE_AdvancedMode_GokaMekkyaku) && HasStatusEffect(Buffs.Kassatsu))
                MudraState.CurrentMudra = MudraCasting.MudraState.CastingGokaMekkyaku;

            if (Variant.CanCure(CustomComboPreset.NIN_Variant_Cure, Config.NIN_VariantCure))
                return Variant.Cure;

            if (OccultCrescent.ShouldUsePhantomActions())
                return OccultCrescent.BestPhantomAction();

            if (IsEnabled(CustomComboPreset.NIN_AoE_AdvancedMode_KunaisBane))
            {
                if (!HasStatusEffect(Buffs.ShadowWalker) && KunaisBane.LevelChecked() && GetCooldownRemainingTime(KunaisBane) < 5 && MudraState.CastHuton(ref actionID))
                    return actionID;

                if (HasStatusEffect(Buffs.ShadowWalker) && KunaisBane.LevelChecked() && IsOffCooldown(KunaisBane) && canWeave)
                    return KunaisBane;
            }

            if (canWeave && !inMudraState)
            {
                if (Variant.CanRampart(CustomComboPreset.NIN_Variant_Rampart))
                    return Variant.Rampart;

                if (IsEnabled(CustomComboPreset.NIN_AoE_AdvancedMode_TenriJindo) && HasStatusEffect(Buffs.TenriJendo))
                    return OriginalHook(TenriJendo);

                if (IsEnabled(CustomComboPreset.NIN_AoE_AdvancedMode_Bunshin) && Bunshin.LevelChecked() && IsOffCooldown(Bunshin) && gauge.Ninki >= bunshingPool)
                    return OriginalHook(Bunshin);

                if (IsEnabled(CustomComboPreset.NIN_AoE_AdvancedMode_HellfrogMedium) && gauge.Ninki >= hellfrogPool && Hellfrog.LevelChecked())
                {
                    if (HasStatusEffect(Buffs.Meisui) && TraitLevelChecked(440))
                        return OriginalHook(Bhavacakra);

                    return OriginalHook(Hellfrog);
                }

                if (IsEnabled(CustomComboPreset.NIN_AoE_AdvancedMode_HellfrogMedium) && gauge.Ninki >= hellfrogPool && !Hellfrog.LevelChecked() && Bhavacakra.LevelChecked())
                {
                    return OriginalHook(Bhavacakra);
                }

                if (IsEnabled(CustomComboPreset.NIN_AoE_AdvancedMode_Kassatsu) &&
                    IsOffCooldown(Kassatsu) &&
                    Kassatsu.LevelChecked() &&
                    (IsEnabled(CustomComboPreset.NIN_AoE_AdvancedMode_Ninjitsus_Doton) && (dotonBuff != null || GetTargetHPPercent() < dotonThreshold) ||
                     IsNotEnabled(CustomComboPreset.NIN_AoE_AdvancedMode_Ninjitsus_Doton)))
                    return OriginalHook(Kassatsu);

                if (IsEnabled(CustomComboPreset.NIN_AoE_AdvancedMode_Meisui) && HasStatusEffect(Buffs.ShadowWalker) && gauge.Ninki <= 50 && IsOffCooldown(Meisui) && Meisui.LevelChecked())
                    return OriginalHook(Meisui);

                if (IsEnabled(CustomComboPreset.NIN_AoE_AdvancedMode_AssassinateDWAD) && IsOffCooldown(OriginalHook(Assassinate)) && Assassinate.LevelChecked())
                    return OriginalHook(Assassinate);

                // healing - please move if not appropriate priority
                if (IsEnabled(CustomComboPreset.NIN_AoE_AdvancedMode_SecondWind) && Role.CanSecondWind(secondWindThreshold))
                    return Role.SecondWind;

                if (IsEnabled(CustomComboPreset.NIN_AoE_AdvancedMode_ShadeShift) && ShadeShift.LevelChecked() && playerHP <= shadeShiftThreshold && IsOffCooldown(ShadeShift))
                    return ShadeShift;

                if (IsEnabled(CustomComboPreset.NIN_AoE_AdvancedMode_Bloodbath) && Role.CanBloodBath(bloodbathThreshold))
                    return Role.Bloodbath;

                if (IsEnabled(CustomComboPreset.NIN_AoE_AdvancedMode_TCJ) &&
                    IsOffCooldown(TenChiJin) &&
                    TenChiJin.LevelChecked())
                {
                    if (IsEnabled(CustomComboPreset.NIN_AoE_AdvancedMode_Ninjitsus_Doton) && tcjPath == 1 &&
                        (dotonBuff?.RemainingTime <= dotonTimer || dotonBuff is null) &&
                        GetTargetHPPercent() >= dotonThreshold &&
                        !WasLastAction(Doton) ||
                        tcjPath == 0)
                        return OriginalHook(TenChiJin);
                }
            }

            if (IsEnabled(CustomComboPreset.NIN_AoE_AdvancedMode_GokaMekkyaku) &&
                MudraState.CastGokaMekkyaku(ref actionID))
                return actionID;

            if (IsEnabled(CustomComboPreset.NIN_AoE_AdvancedMode_Ninjitsus))
            {
                if (IsEnabled(CustomComboPreset.NIN_AoE_AdvancedMode_Ninjitsus_Doton) &&
                    (dotonBuff?.RemainingTime <= dotonTimer || dotonBuff is null) &&
                    GetTargetHPPercent() >= dotonThreshold &&
                    chargeCheck &&
                    !(WasLastAction(Doton) || WasLastAction(TCJDoton) || dotonBuff is not null) &&
                    MudraState.CastDoton(ref actionID))
                    return actionID;

                if (IsEnabled(CustomComboPreset.NIN_AoE_AdvancedMode_Ninjitsus_Katon) &&
                    chargeCheck &&
                    (IsEnabled(CustomComboPreset.NIN_AoE_AdvancedMode_Ninjitsus_Doton) && (dotonBuff != null || GetTargetHPPercent() < dotonThreshold) ||
                     IsNotEnabled(CustomComboPreset.NIN_AoE_AdvancedMode_Ninjitsus_Doton)) &&
                    MudraState.CastKaton(ref actionID))
                    return actionID;
            }

            if (IsEnabled(CustomComboPreset.NIN_AoE_AdvancedMode_Bunshin_Phantom) && HasStatusEffect(Buffs.PhantomReady) && PhantomKamaitachi.LevelChecked())
                return OriginalHook(PhantomKamaitachi);

            if (ComboTimer > 1f)
            {
                if (ComboAction is DeathBlossom && HakkeMujinsatsu.LevelChecked())
                    return OriginalHook(HakkeMujinsatsu);
            }

            return OriginalHook(DeathBlossom);
        }
    }

    internal class NIN_ST_SimpleMode : CustomCombo
    {
        protected internal static NINOpenerMaxLevel4thGCDKunai NINOpener = new();

        protected internal MudraCasting MudraState = new();
        protected internal override CustomComboPreset Preset { get; } = CustomComboPreset.NIN_ST_SimpleMode;

        protected override uint Invoke(uint actionID)
        {
            if (actionID is not SpinningEdge)
                return actionID;

            NINGauge gauge = GetJobGauge<NINGauge>();
            bool canWeave = CanWeave();
            bool canDelayedWeave = CanDelayedWeave();
            bool inTrickBurstSaveWindow = GetCooldownRemainingTime(TrickAttack) <= 20;
            bool useBhakaBeforeTrickWindow = GetCooldownRemainingTime(TrickAttack) >= 3;
            bool setupSuitonWindow = GetCooldownRemainingTime(OriginalHook(TrickAttack)) <= 18 && !HasStatusEffect(Buffs.ShadowWalker);
            bool setupKassatsuWindow = GetCooldownRemainingTime(TrickAttack) <= 10 && HasStatusEffect(Buffs.ShadowWalker);
            bool poolCharges = GetRemainingCharges(Ten) == 1 && GetCooldownChargeRemainingTime(Ten) < 2 || TrickDebuff || InMudra;
            bool raitonUptime = true;
            int bhavaPool = 50;
            int bunshinPool = 50;
            int secondWindThreshold = 50;
            int shadeShiftThreshold = 50;
            int bloodbathThreshold = 50;
            double playerHP = PlayerHealthPercentageHp();
            bool phantomUptime = true;
            bool trueNorthArmor = !OnTargetsFlank() && Role.CanTrueNorth();
            bool trueNorthEdge = !OnTargetsRear() && Role.CanTrueNorth();
            bool dynamic = true;

            if (ActionWatching.TimeSinceLastAction.TotalSeconds >= 5 && !InCombat())
                MudraState.CurrentMudra = MudraCasting.MudraState.None;

            if (IsOnCooldown(TrickAttack) && MudraState.CurrentMudra == MudraCasting.MudraState.CastingSuiton && !setupSuitonWindow)
                MudraState.CurrentMudra = MudraCasting.MudraState.None;

            if (IsOnCooldown(TrickAttack) && MudraState.CurrentMudra != MudraCasting.MudraState.CastingSuiton && setupSuitonWindow)
                MudraState.CurrentMudra = MudraCasting.MudraState.CastingSuiton;

            if (OriginalHook(Ninjutsu) is Rabbit)
                return OriginalHook(Ninjutsu);

            if (InMudra)
            {
                if (MudraState.ContinueCurrentMudra(ref actionID))
                    return actionID;
            }

            if (IsOffCooldown(Mug) && Mug.LevelChecked())
            {
                if ((GetCooldown(TrickAttack).CooldownRemaining < 3 || TrickDebuff) &&
                    CombatEngageDuration().TotalSeconds > 5 &&
                    canDelayedWeave)
                    return OriginalHook(Mug);
            }

            if (HasStatusEffect(Buffs.Kassatsu) &&
                TrickDebuff &&
                MudraState.CastHyoshoRanryu(ref actionID))
                return actionID;

            if (!Suiton.LevelChecked()) //For low level
            {
                if (Raiton.LevelChecked()) //under 45 will only use Raiton
                {
                    if (MudraState.CastRaiton(ref actionID))
                        return actionID;
                }
                else if (!Raiton.LevelChecked() && MudraState.CastFumaShuriken(ref actionID)) // 30-35 will use only fuma
                    return actionID;
            }

            if (HasStatusEffect(Buffs.TenChiJin))
            {
                if (OriginalHook(Ten) == TCJFumaShurikenTen)
                    return OriginalHook(Ten);
                if (OriginalHook(Chi) == TCJRaiton)
                    return OriginalHook(Chi);
                if (OriginalHook(Jin) == TCJSuiton)
                    return OriginalHook(Jin);
            }

            if (Variant.CanCure(CustomComboPreset.NIN_Variant_Cure, Config.NIN_VariantCure))
                return Variant.Cure;

            if (OccultCrescent.ShouldUsePhantomActions())
                return OccultCrescent.BestPhantomAction();

            if (InCombat() && !InMeleeRange())
            {
                if (HasStatusEffect(Buffs.PhantomReady) &&
                    (GetCooldownRemainingTime(TrickAttack) > GetStatusEffectRemainingTime(Buffs.PhantomReady) && GetStatusEffectRemainingTime(Buffs.PhantomReady) < 5 || TrickDebuff || HasStatusEffect(Buffs.Bunshin) && MugDebuff) &&
                    PhantomKamaitachi.LevelChecked()
                    && phantomUptime)
                    return OriginalHook(PhantomKamaitachi);

                if (setupSuitonWindow &&
                    TrickAttack.LevelChecked() &&
                    !HasStatusEffect(Buffs.ShadowWalker) &&
                    MudraState.CastSuiton(ref actionID))
                    return actionID;

                if (!inTrickBurstSaveWindow &&
                    poolCharges &&
                    raitonUptime &&
                    MudraState.CastRaiton(ref actionID))
                    return actionID;

                if (ThrowingDaggers.LevelChecked() && HasTarget() && !HasStatusEffect(Buffs.RaijuReady))
                    return OriginalHook(ThrowingDaggers);
            }

            if (canWeave && !InMudra)
            {
                if (Variant.CanRampart(CustomComboPreset.NIN_Variant_Rampart))
                    return Variant.Rampart;

                if (HasStatusEffect(Buffs.ShadowWalker) &&
                    IsOffCooldown(TrickAttack) &&
                    InCombat() && CombatEngageDuration().TotalSeconds > 8 &&
                    canDelayedWeave)
                    return OriginalHook(TrickAttack);

                if (HasStatusEffect(Buffs.TenriJendo) && (TrickDebuff || GetStatusEffectRemainingTime(Buffs.TenriJendo) <= 3))
                    return OriginalHook(TenriJendo);

                if (Bunshin.LevelChecked() && IsOffCooldown(Bunshin) && gauge.Ninki >= bunshinPool)
                    return OriginalHook(Bunshin);

                if ((TrickDebuff || setupKassatsuWindow) && IsOffCooldown(Kassatsu) && Kassatsu.LevelChecked())
                    return OriginalHook(Kassatsu);

                //healing - please move if not appropriate priority
                if (Role.CanSecondWind(secondWindThreshold))
                    return Role.SecondWind;

                if (ShadeShift.LevelChecked() && playerHP <= shadeShiftThreshold && IsOffCooldown(ShadeShift))
                    return ShadeShift;

                if (Role.CanSecondWind(bloodbathThreshold))
                    return Role.Bloodbath;

                if ((TrickDebuff && gauge.Ninki >= 50 || useBhakaBeforeTrickWindow && gauge.Ninki >= 85) &&
                    Bhavacakra.LevelChecked())
                    return OriginalHook(Bhavacakra);

                if ((TrickDebuff && gauge.Ninki >= 50 || useBhakaBeforeTrickWindow && gauge.Ninki >= 60) &&
                    !Bhavacakra.LevelChecked() && Hellfrog.LevelChecked())
                    return OriginalHook(Hellfrog);

                if (!inTrickBurstSaveWindow)
                {
                    if (HasStatusEffect(Buffs.ShadowWalker) && gauge.Ninki <= 50 && IsOffCooldown(Meisui) && Meisui.LevelChecked())
                        return OriginalHook(Meisui);

                    if (gauge.Ninki >= bhavaPool && Bhavacakra.LevelChecked())
                        return OriginalHook(Bhavacakra);

                    if (gauge.Ninki >= bhavaPool && !Bhavacakra.LevelChecked() && Hellfrog.LevelChecked())
                        return OriginalHook(Hellfrog);

                    if (IsOffCooldown(OriginalHook(Assassinate)) && Assassinate.LevelChecked())
                        return OriginalHook(Assassinate);

                    if (IsOffCooldown(TenChiJin) && TenChiJin.LevelChecked())
                        return OriginalHook(TenChiJin);
                }

                if (Role.CanSecondWind(secondWindThreshold))
                    return Role.SecondWind;

                if (ShadeShift.LevelChecked() && playerHP <= shadeShiftThreshold && IsOffCooldown(ShadeShift))
                    return ShadeShift;

                if (Role.CanBloodBath(bloodbathThreshold))
                    return Role.Bloodbath;
            }

            if (HasStatusEffect(Buffs.RaijuReady) && InMeleeRange())
            {
                return OriginalHook(FleetingRaiju);
            }

            if (!inTrickBurstSaveWindow &&
                IsOnCooldown(Mug) &&
                MudraState.CastHyoshoRanryu(ref actionID))
                return actionID;

            if (setupSuitonWindow &&
                TrickAttack.LevelChecked() &&
                !HasStatusEffect(Buffs.ShadowWalker) &&
                MudraState.CastSuiton(ref actionID))
                return actionID;

            if (
                !inTrickBurstSaveWindow &&
                poolCharges &&
                MudraState.CastRaiton(ref actionID))
                return actionID;

            if (HasStatusEffect(Buffs.PhantomReady) &&
                (GetCooldownRemainingTime(TrickAttack) > GetStatusEffectRemainingTime(Buffs.PhantomReady) && GetStatusEffectRemainingTime(Buffs.PhantomReady) < 5 || TrickDebuff || HasStatusEffect(Buffs.Bunshin) && HasStatusEffect(Debuffs.Mug, CurrentTarget)) &&
                PhantomKamaitachi.LevelChecked())
                return OriginalHook(PhantomKamaitachi);

            if (!Raiton.LevelChecked() &&
                MudraState.CastFumaShuriken(ref actionID))
                return actionID;

            if (ComboTimer > 1f)
            {
                if (ComboAction == SpinningEdge && GustSlash.LevelChecked())
                    return OriginalHook(GustSlash);

                if (ComboAction == GustSlash && ArmorCrush.LevelChecked())
                {
                    if (gauge.Kazematoi == 0)
                    {
                        if (trueNorthArmor)
                            return Role.TrueNorth;

                        return ArmorCrush;
                    }

                    if (dynamic)
                    {
                        if (gauge.Kazematoi >= 4)
                        {
                            if (trueNorthEdge)
                                return Role.TrueNorth;

                            return AeolianEdge;
                        }

                        if (OnTargetsFlank())
                            return ArmorCrush;
                        return AeolianEdge;
                    }
                    if (gauge.Kazematoi < 3)
                    {
                        if (trueNorthArmor)
                            return Role.TrueNorth;

                        return ArmorCrush;
                    }

                    return AeolianEdge;
                }
                if (ComboAction == GustSlash && !ArmorCrush.LevelChecked() && AeolianEdge.LevelChecked())
                {
                    if (trueNorthEdge)
                        return OriginalHook(Role.TrueNorth);
                    return OriginalHook(AeolianEdge);
                }
            }
            return OriginalHook(SpinningEdge);
        }
    }

    internal class NIN_AoE_SimpleMode : CustomCombo
    {
        private readonly MudraCasting _mudraState = new();
        protected internal override CustomComboPreset Preset { get; } = CustomComboPreset.NIN_AoE_SimpleMode;

        protected override uint Invoke(uint actionID)
        {
            if (actionID is not DeathBlossom)
                return actionID;

            Status? dotonBuff = GetStatusEffect(Buffs.Doton);
            NINGauge gauge = GetJobGauge<NINGauge>();
            bool canWeave = CanWeave();

            if (ActionWatching.TimeSinceLastAction.TotalSeconds >= 5 && !InCombat())
                _mudraState.CurrentMudra = MudraCasting.MudraState.None;

            if (OriginalHook(Ninjutsu) is Rabbit)
                return OriginalHook(Ninjutsu);

            if (InMudra)
            {
                if (_mudraState.ContinueCurrentMudra(ref actionID))
                    return actionID;
            }

            if (HasStatusEffect(Buffs.TenChiJin))
            {
                if (WasLastAction(TCJFumaShurikenJin))
                    return OriginalHook(Ten);
                if (WasLastAction(TCJKaton) || WasLastAction(HollowNozuchi))
                    return OriginalHook(Chi);
                return OriginalHook(Jin);
            }

            if (HasStatusEffect(Buffs.Kassatsu))
            {
                if (GokaMekkyaku.LevelChecked())
                {
                    _mudraState.CurrentMudra = MudraCasting.MudraState.CastingGokaMekkyaku;
                    if (_mudraState.CastGokaMekkyaku(ref actionID))
                        return actionID;
                }
                else
                {
                    _mudraState.CurrentMudra = MudraCasting.MudraState.CastingKaton;
                    if (_mudraState.CastKaton(ref actionID))
                        return actionID;
                }
            }

            if (Variant.CanCure(CustomComboPreset.NIN_Variant_Cure, Config.NIN_VariantCure))
                return Variant.Cure;

            if (OccultCrescent.ShouldUsePhantomActions())
                return OccultCrescent.BestPhantomAction();

            if (!HasStatusEffect(Buffs.ShadowWalker) && KunaisBane.LevelChecked() && GetCooldownRemainingTime(KunaisBane) < 5 && _mudraState.CastHuton(ref actionID))
                return actionID;

            if (HasStatusEffect(Buffs.ShadowWalker) && KunaisBane.LevelChecked() && IsOffCooldown(KunaisBane) && canWeave)
                return KunaisBane;

            if (GetTargetHPPercent() > 20 && (dotonBuff is null || dotonBuff.RemainingTime <= GetCooldownChargeRemainingTime(Ten)) && !JustUsed(Doton) && IsOnCooldown(TenChiJin))
            {
                if (_mudraState.CastDoton(ref actionID))
                    return actionID;
            }
            else if (_mudraState.CurrentMudra == MudraCasting.MudraState.CastingDoton)
                _mudraState.CurrentMudra = MudraCasting.MudraState.None;

            if (_mudraState.CastKaton(ref actionID))
                return actionID;

            if (canWeave && !InMudra)
            {
                if (Variant.CanRampart(CustomComboPreset.NIN_Variant_Rampart))
                    return Variant.Rampart;

                if (IsOffCooldown(TenChiJin) && TenChiJin.LevelChecked())
                    return OriginalHook(TenChiJin);

                if (HasStatusEffect(Buffs.TenriJendo))
                    return TenriJendo;

                if (IsOffCooldown(Bunshin) && gauge.Ninki >= 50 && Bunshin.LevelChecked())
                    return OriginalHook(Bunshin);

                if (HasStatusEffect(Buffs.ShadowWalker) && gauge.Ninki < 50 && IsOffCooldown(Meisui) && Meisui.LevelChecked())
                    return OriginalHook(Meisui);

                if (HasStatusEffect(Buffs.Meisui) && gauge.Ninki >= 50)
                    return OriginalHook(Bhavacakra);

                if (gauge.Ninki >= 50 && Hellfrog.LevelChecked())
                    return OriginalHook(Hellfrog);

                if (gauge.Ninki >= 50 && !Hellfrog.LevelChecked() && Bhavacakra.LevelChecked())
                    return OriginalHook(Bhavacakra);

                if (IsOffCooldown(Kassatsu) && Kassatsu.LevelChecked())
                    return OriginalHook(Kassatsu);
            }
            else
            {
                if (HasStatusEffect(Buffs.PhantomReady))
                    return OriginalHook(PhantomKamaitachi);
            }

            if (ComboTimer > 1f)
            {
                if (ComboAction is DeathBlossom && HakkeMujinsatsu.LevelChecked())
                    return OriginalHook(HakkeMujinsatsu);
            }

            return OriginalHook(DeathBlossom);
        }
    }

    internal class NIN_ArmorCrushCombo : CustomCombo
    {
        protected internal override CustomComboPreset Preset { get; } = CustomComboPreset.NIN_ArmorCrushCombo;

        protected override uint Invoke(uint actionID)
        {
            if (actionID is not ArmorCrush)
                return actionID;
            if (ComboTimer > 0f)
            {
                if (ComboAction == SpinningEdge && GustSlash.LevelChecked())
                {
                    return GustSlash;
                }

                if (ComboAction == GustSlash && ArmorCrush.LevelChecked())
                {
                    return ArmorCrush;
                }
            }
            return SpinningEdge;
        }
    }

    internal class NIN_HideMug : CustomCombo
    {
        protected internal override CustomComboPreset Preset { get; } = CustomComboPreset.NIN_HideMug;

        protected override uint Invoke(uint actionID)
        {
            if (actionID is not Hide)
                return actionID;

            if (HasCondition(ConditionFlag.InCombat))
            {
                return OriginalHook(Mug);
            }

            if (HasStatusEffect(Buffs.Hidden))
            {
                return OriginalHook(TrickAttack);
            }

            return actionID;
        }
    }

    internal class NIN_KassatsuChiJin : CustomCombo
    {
        protected internal override CustomComboPreset Preset { get; } = CustomComboPreset.NIN_KassatsuChiJin;

        protected override uint Invoke(uint actionID)
        {
            if (actionID == Chi && TraitLevelChecked(250) && HasStatusEffect(Buffs.Kassatsu))
            {
                return Jin;
            }
            return actionID;
        }
    }

    internal class NIN_KassatsuTrick : CustomCombo
    {
        protected internal override CustomComboPreset Preset { get; } = CustomComboPreset.NIN_KassatsuTrick;

        protected override uint Invoke(uint actionID)
        {
            if (actionID is not Kassatsu)
                return actionID;
            if (HasStatusEffect(Buffs.ShadowWalker) || HasStatusEffect(Buffs.Hidden))
            {
                return OriginalHook(TrickAttack);
            }
            return OriginalHook(Kassatsu);
        }
    }

    internal class NIN_TCJMeisui : CustomCombo
    {
        protected internal override CustomComboPreset Preset => CustomComboPreset.NIN_TCJMeisui;

        protected override uint Invoke(uint actionID)
        {
            if (actionID is not TenChiJin)
                return actionID;

            if (HasStatusEffect(Buffs.ShadowWalker))
                return Meisui;

            if (HasStatusEffect(Buffs.TenChiJin) && IsEnabled(CustomComboPreset.NIN_TCJ))
            {
                float tcjTimer = GetStatusEffectRemainingTime(Buffs.TenChiJin, anyOwner: true);

                if (tcjTimer > 5)
                    return OriginalHook(Ten);

                if (tcjTimer > 4)
                    return OriginalHook(Chi);

                if (tcjTimer > 3)
                    return OriginalHook(Jin);
            }
            return actionID;
        }
    }

    internal class NIN_Simple_Mudras : CustomCombo
    {
        protected internal override CustomComboPreset Preset { get; } = CustomComboPreset.NIN_Simple_Mudras;

        protected override uint Invoke(uint actionID)
        {
            if (actionID is not (Ten or Chi or Jin) || !HasStatusEffect(Buffs.Mudra))
                return actionID;

            int mudrapath = Config.NIN_SimpleMudra_Choice;

            if (mudrapath == 1)
            {
                if (Ten.LevelChecked() && actionID == Ten)
                {
                    if (Jin.LevelChecked() && OriginalHook(Ninjutsu) is Raiton)
                    {
                        return OriginalHook(JinCombo);
                    }

                    if (Chi.LevelChecked() && OriginalHook(Ninjutsu) is HyoshoRanryu)
                    {
                        return OriginalHook(ChiCombo);
                    }

                    if (OriginalHook(Ninjutsu) == FumaShuriken)
                    {
                        if (HasStatusEffect(Buffs.Kassatsu) && Traits.EnhancedKasatsu.TraitLevelChecked())
                            return JinCombo;

                        if (Chi.LevelChecked())
                            return OriginalHook(ChiCombo);

                        if (Jin.LevelChecked())
                            return OriginalHook(JinCombo);
                    }
                }

                if (Chi.LevelChecked() && actionID == Chi)
                {
                    if (OriginalHook(Ninjutsu) is Hyoton)
                    {
                        return OriginalHook(TenCombo);
                    }

                    if (Jin.LevelChecked() && OriginalHook(Ninjutsu) == FumaShuriken)
                    {
                        return OriginalHook(JinCombo);
                    }
                }

                if (Jin.LevelChecked() && actionID == Jin)
                {
                    if (OriginalHook(Ninjutsu) is GokaMekkyaku or Katon)
                    {
                        return OriginalHook(ChiCombo);
                    }

                    if (OriginalHook(Ninjutsu) == FumaShuriken)
                    {
                        return OriginalHook(TenCombo);
                    }
                }

                return OriginalHook(Ninjutsu);
            }

            if (mudrapath == 2)
            {
                if (Ten.LevelChecked() && actionID == Ten)
                {
                    if (Chi.LevelChecked() && OriginalHook(Ninjutsu) is Hyoton or HyoshoRanryu)
                    {
                        return OriginalHook(Chi);
                    }

                    if (OriginalHook(Ninjutsu) == FumaShuriken)
                    {
                        if (Jin.LevelChecked())
                            return OriginalHook(JinCombo);

                        if (Chi.LevelChecked())
                            return OriginalHook(ChiCombo);
                    }
                }

                if (Chi.LevelChecked() && actionID == Chi)
                {
                    if (Jin.LevelChecked() && OriginalHook(Ninjutsu) is Katon or GokaMekkyaku)
                    {
                        return OriginalHook(Jin);
                    }

                    if (OriginalHook(Ninjutsu) == FumaShuriken)
                    {
                        return OriginalHook(Ten);
                    }
                }

                if (Jin.LevelChecked() && actionID == Jin)
                {
                    if (OriginalHook(Ninjutsu) is Raiton)
                    {
                        return OriginalHook(Ten);
                    }

                    if (OriginalHook(Ninjutsu) == GokaMekkyaku)
                    {
                        return OriginalHook(Chi);
                    }

                    if (OriginalHook(Ninjutsu) == FumaShuriken)
                    {
                        if (HasStatusEffect(Buffs.Kassatsu) && Traits.EnhancedKasatsu.TraitLevelChecked())
                            return OriginalHook(Ten);
                        return OriginalHook(Chi);
                    }
                }

                return OriginalHook(Ninjutsu);
            }

            return actionID;
        }
    }
}
