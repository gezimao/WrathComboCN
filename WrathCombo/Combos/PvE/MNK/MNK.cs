using WrathCombo.CustomComboNS;
using static WrathCombo.Combos.PvE.MNK.Config;
using static WrathCombo.Data.ActionWatching;
namespace WrathCombo.Combos.PvE;

internal partial class MNK : Melee
{
    internal class MNK_ST_SimpleMode : CustomCombo
    {
        protected internal override CustomComboPreset Preset => CustomComboPreset.MNK_ST_SimpleMode;

        protected override uint Invoke(uint actionID)
        {
            if (actionID is not (Bootshine or LeapingOpo))
                return actionID;

            //Variant Cure
            if (Variant.CanCure(CustomComboPreset.MNK_Variant_Cure, MNK_VariantCure))
                return Variant.Cure;

            //Variant Rampart
            if (Variant.CanRampart(CustomComboPreset.MNK_Variant_Rampart))
                return Variant.Rampart;

            if (OccultCrescent.ShouldUsePhantomActions())
                return OccultCrescent.BestPhantomAction();

            if (LevelChecked(SteeledMeditation) &&
                (!InCombat() || !InMeleeRange()) &&
                Chakra < 5 &&
                IsOriginal(MasterfulBlitz) &&
                !HasStatusEffect(Buffs.RiddleOfFire) &&
                !HasStatusEffect(Buffs.WindsRumination) &&
                !HasStatusEffect(Buffs.FiresRumination))
                return OriginalHook(SteeledMeditation);

            if (LevelChecked(FormShift) && !InCombat() &&
                !HasStatusEffect(Buffs.FormlessFist) && !HasStatusEffect(Buffs.PerfectBalance) &&
                !HasStatusEffect(Buffs.OpoOpoForm) && !HasStatusEffect(Buffs.RaptorForm) && !HasStatusEffect(Buffs.CoeurlForm))
                return FormShift;

            // OGCDs
            if (CanWeave() && !HasDoubleWeaved())
            {
                if (UseBrotherhood())
                    return Brotherhood;

                if (UseRoF())
                    return RiddleOfFire;

                if (UseRoW())
                    return RiddleOfWind;

                if (UsePerfectBalanceST())
                    return PerfectBalance;

                if (Role.CanSecondWind(25))
                    return Role.SecondWind;

                if (Role.CanBloodBath(40))
                    return Role.Bloodbath;

                if (Chakra >= 5 && InCombat() && LevelChecked(SteeledMeditation) &&
                    !JustUsed(Brotherhood) && !JustUsed(RiddleOfFire))
                    return OriginalHook(SteeledMeditation);
            }

            // GCDs
            if (HasStatusEffect(Buffs.FormlessFist))
                return OpoOpo is 0
                    ? DragonKick
                    : OriginalHook(Bootshine);

            // Masterful Blitz
            if (LevelChecked(MasterfulBlitz) &&
                !HasStatusEffect(Buffs.PerfectBalance) &&
                InMasterfulRange() && !IsOriginal(MasterfulBlitz))
                return OriginalHook(MasterfulBlitz);

            if (HasStatusEffect(Buffs.FiresRumination) &&
                !HasStatusEffect(Buffs.FormlessFist) &&
                !HasStatusEffect(Buffs.PerfectBalance) &&
                !JustUsed(RiddleOfFire, 4) &&
                (JustUsed(OriginalHook(Bootshine)) ||
                 JustUsed(DragonKick) ||
                 GetStatusEffectRemainingTime(Buffs.FiresRumination) < 4 ||
                 !InMeleeRange()))
                return FiresReply;

            if (HasStatusEffect(Buffs.WindsRumination) &&
                !HasStatusEffect(Buffs.PerfectBalance) &&
                ((GetCooldownRemainingTime(RiddleOfFire) > 10) ||
                 HasStatusEffect(Buffs.RiddleOfFire) ||
                 (GetStatusEffectRemainingTime(Buffs.WindsRumination) < GCD * 2) ||
                 !InMeleeRange()))
                return WindsReply;

            // Perfect Balance
            if (DoPerfectBalanceComboST(ref actionID))
                return actionID;

            // Standard Beast Chakras
            return DetermineCoreAbility(actionID, true);
        }
    }

    internal class MNK_ST_AdvancedMode : CustomCombo
    {
        protected internal override CustomComboPreset Preset => CustomComboPreset.MNK_ST_AdvancedMode;

        protected override uint Invoke(uint actionID)
        {
            if (actionID is not (Bootshine or LeapingOpo))
                return actionID;

            //Variant Cure
            if (Variant.CanCure(CustomComboPreset.MNK_Variant_Cure, MNK_VariantCure))
                return Variant.Cure;

            //Variant Rampart
            if (Variant.CanRampart(CustomComboPreset.MNK_Variant_Rampart))
                return Variant.Rampart;

            if (OccultCrescent.ShouldUsePhantomActions())
                return OccultCrescent.BestPhantomAction();

            if (IsEnabled(CustomComboPreset.MNK_STUseMeditation) &&
                LevelChecked(SteeledMeditation) &&
                (!InCombat() || !InMeleeRange()) &&
                Chakra < 5 &&
                IsOriginal(MasterfulBlitz) &&
                !HasStatusEffect(Buffs.RiddleOfFire) &&
                !HasStatusEffect(Buffs.WindsRumination) &&
                !HasStatusEffect(Buffs.FiresRumination))
                return OriginalHook(SteeledMeditation);

            if (IsEnabled(CustomComboPreset.MNK_STUseFormShift) &&
                LevelChecked(FormShift) && !InCombat() &&
                !HasStatusEffect(Buffs.FormlessFist) && !HasStatusEffect(Buffs.PerfectBalance) &&
                !HasStatusEffect(Buffs.OpoOpoForm) && !HasStatusEffect(Buffs.RaptorForm) && !HasStatusEffect(Buffs.CoeurlForm))
                return FormShift;

            if (IsEnabled(CustomComboPreset.MNK_STUseOpener) &&
                Opener().FullOpener(ref actionID))
                return Opener().OpenerStep >= 9 &&
                       CanWeave() && !HasDoubleWeaved() &&
                       Chakra >= 5
                    ? TheForbiddenChakra
                    : actionID;

            // OGCDs
            if (CanWeave() && !HasDoubleWeaved())
            {
                if (IsEnabled(CustomComboPreset.MNK_STUseBuffs))
                {
                    if (IsEnabled(CustomComboPreset.MNK_STUseBrotherhood) &&
                        UseBrotherhood() &&
                        (MNK_ST_Brotherhood_SubOption == 0 ||
                         MNK_ST_Brotherhood_SubOption == 1 && InBossEncounter()))
                        return Brotherhood;

                    if (IsEnabled(CustomComboPreset.MNK_STUseROF) &&
                        UseRoF() &&
                        (MNK_ST_RiddleOfFire_SubOption == 0 ||
                         MNK_ST_RiddleOfFire_SubOption == 1 && InBossEncounter()))
                        return RiddleOfFire;

                    if (IsEnabled(CustomComboPreset.MNK_STUseROW) &&
                        UseRoW() &&
                        (MNK_ST_RiddleOfWind_SubOption == 0 ||
                         MNK_ST_RiddleOfWind_SubOption == 1 && InBossEncounter()))
                        return RiddleOfWind;
                }
                if (IsEnabled(CustomComboPreset.MNK_STUsePerfectBalance) &&
                    UsePerfectBalanceST())
                    return PerfectBalance;

                if (IsEnabled(CustomComboPreset.MNK_ST_ComboHeals))
                {
                    if (Role.CanSecondWind(MNK_ST_SecondWind_Threshold))
                        return Role.SecondWind;

                    if (Role.CanBloodBath(MNK_ST_Bloodbath_Threshold))
                        return Role.Bloodbath;
                }

                if (IsEnabled(CustomComboPreset.MNK_STUseTheForbiddenChakra) &&
                    Chakra >= 5 && InCombat() &&
                    LevelChecked(SteeledMeditation) &&
                    !JustUsed(Brotherhood) && !JustUsed(RiddleOfFire))
                    return OriginalHook(SteeledMeditation);
            }

            // GCDs
            if (HasStatusEffect(Buffs.FormlessFist))
                return OpoOpo is 0
                    ? DragonKick
                    : OriginalHook(Bootshine);

            // Masterful Blitz
            if (IsEnabled(CustomComboPreset.MNK_STUseMasterfulBlitz) &&
                LevelChecked(MasterfulBlitz) &&
                !HasStatusEffect(Buffs.PerfectBalance) && InMasterfulRange() &&
                !IsOriginal(MasterfulBlitz))
                return OriginalHook(MasterfulBlitz);

            if (IsEnabled(CustomComboPreset.MNK_STUseBuffs))
            {
                if (IsEnabled(CustomComboPreset.MNK_STUseFiresReply) &&
                    HasStatusEffect(Buffs.FiresRumination) &&
                    !HasStatusEffect(Buffs.FormlessFist) &&
                    !HasStatusEffect(Buffs.PerfectBalance) &&
                    !JustUsed(RiddleOfFire, 4) &&
                    (JustUsed(OriginalHook(Bootshine)) ||
                     JustUsed(DragonKick) ||
                     (GetStatusEffectRemainingTime(Buffs.FiresRumination) < GCD * 2) ||
                     !InMeleeRange()))
                    return FiresReply;

                if (IsEnabled(CustomComboPreset.MNK_STUseWindsReply) &&
                    HasStatusEffect(Buffs.WindsRumination) &&
                    !HasStatusEffect(Buffs.PerfectBalance) &&
                    ((GetCooldownRemainingTime(RiddleOfFire) > 10) ||
                     HasStatusEffect(Buffs.RiddleOfFire) ||
                     (GetStatusEffectRemainingTime(Buffs.WindsRumination) < GCD * 2) ||
                     !InMeleeRange()))
                    return WindsReply;
            }

            // Perfect Balance
            if (DoPerfectBalanceComboST(ref actionID))
                return actionID;

            // Standard Beast Chakras
            return DetermineCoreAbility(actionID, IsEnabled(CustomComboPreset.MNK_STUseTrueNorth));
        }
    }

    internal class MNK_AOE_SimpleMode : CustomCombo
    {
        protected internal override CustomComboPreset Preset => CustomComboPreset.MNK_AOE_SimpleMode;

        protected override uint Invoke(uint actionID)
        {
            if (actionID is not (ArmOfTheDestroyer or ShadowOfTheDestroyer))
                return actionID;

            //Variant Cure
            if (Variant.CanCure(CustomComboPreset.MNK_Variant_Cure, MNK_VariantCure))
                return Variant.Cure;

            //Variant Rampart
            if (Variant.CanRampart(CustomComboPreset.MNK_Variant_Rampart))
                return Variant.Rampart;

            if (OccultCrescent.ShouldUsePhantomActions())
                return OccultCrescent.BestPhantomAction();

            if (LevelChecked(InspiritedMeditation) &&
                (!InCombat() || !InMeleeRange()) &&
                Chakra < 5 &&
                IsOriginal(MasterfulBlitz) &&
                !HasStatusEffect(Buffs.RiddleOfFire) &&
                !HasStatusEffect(Buffs.WindsRumination) &&
                !HasStatusEffect(Buffs.FiresRumination))
                return OriginalHook(InspiritedMeditation);

            if (LevelChecked(FormShift) && !InCombat() &&
                !HasStatusEffect(Buffs.FormlessFist) && !HasStatusEffect(Buffs.PerfectBalance) &&
                !HasStatusEffect(Buffs.OpoOpoForm) && !HasStatusEffect(Buffs.RaptorForm) && !HasStatusEffect(Buffs.CoeurlForm))
                return FormShift;

            // OGCD's
            if (CanWeave() && !HasDoubleWeaved())
            {
                if (UseBrotherhood())
                    return Brotherhood;

                if (UseRoF())
                    return RiddleOfFire;

                if (UseRoW())
                    return RiddleOfWind;

                if (UsePerfectBalanceAoE())
                    return PerfectBalance;

                if (Role.CanSecondWind(25))
                    return Role.SecondWind;

                if (Role.CanBloodBath(40))
                    return Role.Bloodbath;

                if (Chakra >= 5 &&
                    LevelChecked(InspiritedMeditation) &&
                    HasBattleTarget() && InCombat() &&
                    !JustUsed(Brotherhood) && !JustUsed(RiddleOfFire))
                    return OriginalHook(InspiritedMeditation);
            }

            // Masterful Blitz
            if (LevelChecked(MasterfulBlitz) &&
                !HasStatusEffect(Buffs.PerfectBalance) &&
                InMasterfulRange() &&
                !IsOriginal(MasterfulBlitz))
                return OriginalHook(MasterfulBlitz);

            if (HasStatusEffect(Buffs.FiresRumination) &&
                !HasStatusEffect(Buffs.PerfectBalance) &&
                !HasStatusEffect(Buffs.FormlessFist) &&
                !JustUsed(RiddleOfFire, 4))
                return FiresReply;

            if (HasStatusEffect(Buffs.WindsRumination) &&
                !HasStatusEffect(Buffs.PerfectBalance) &&
                ((GetCooldownRemainingTime(RiddleOfFire) > 10) ||
                 HasStatusEffect(Buffs.RiddleOfFire)))
                return WindsReply;

            // Perfect Balance
            if (DoPerfectBalanceComboAoE(ref actionID))
                return actionID;

            // Monk Rotation
            if (HasStatusEffect(Buffs.OpoOpoForm))
                return OriginalHook(ArmOfTheDestroyer);

            if (HasStatusEffect(Buffs.RaptorForm))
            {
                if (LevelChecked(FourPointFury))
                    return FourPointFury;

                if (LevelChecked(TwinSnakes))
                    return TwinSnakes;
            }

            if (HasStatusEffect(Buffs.CoeurlForm) && LevelChecked(Rockbreaker))
                return Rockbreaker;

            return actionID;
        }
    }

    internal class MNK_AOE_AdvancedMode : CustomCombo
    {
        protected internal override CustomComboPreset Preset => CustomComboPreset.MNK_AOE_AdvancedMode;

        protected override uint Invoke(uint actionID)
        {
            if (actionID is not (ArmOfTheDestroyer or ShadowOfTheDestroyer))
                return actionID;

            //Variant Cure
            if (Variant.CanCure(CustomComboPreset.MNK_Variant_Cure, MNK_VariantCure))
                return Variant.Cure;

            //Variant Rampart
            if (Variant.CanRampart(CustomComboPreset.MNK_Variant_Rampart))
                return Variant.Rampart;

            if (OccultCrescent.ShouldUsePhantomActions())
                return OccultCrescent.BestPhantomAction();

            if (IsEnabled(CustomComboPreset.MNK_AoEUseMeditation) &&
                LevelChecked(InspiritedMeditation) &&
                (!InCombat() || !InMeleeRange()) &&
                Chakra < 5 &&
                IsOriginal(MasterfulBlitz) &&
                !HasStatusEffect(Buffs.RiddleOfFire) &&
                !HasStatusEffect(Buffs.WindsRumination) &&
                !HasStatusEffect(Buffs.FiresRumination))
                return OriginalHook(InspiritedMeditation);

            if (IsEnabled(CustomComboPreset.MNK_AoEUseFormShift) &&
                LevelChecked(FormShift) && !InCombat() &&
                !HasStatusEffect(Buffs.FormlessFist) && !HasStatusEffect(Buffs.PerfectBalance) &&
                !HasStatusEffect(Buffs.OpoOpoForm) && !HasStatusEffect(Buffs.RaptorForm) && !HasStatusEffect(Buffs.CoeurlForm))
                return FormShift;

            // OGCD's 
            if (CanWeave() && !HasDoubleWeaved())
            {
                if (IsEnabled(CustomComboPreset.MNK_AoEUseBuffs))
                {
                    if (IsEnabled(CustomComboPreset.MNK_AoEUseBrotherhood) &&
                        UseBrotherhood() &&
                        GetTargetHPPercent() >= MNK_AoE_Brotherhood_HP)
                        return Brotherhood;

                    if (IsEnabled(CustomComboPreset.MNK_AoEUseROF) &&
                        UseRoF() &&
                        GetTargetHPPercent() >= MNK_AoE_RiddleOfFire_HP)
                        return RiddleOfFire;

                    if (IsEnabled(CustomComboPreset.MNK_AoEUseROW) &&
                        UseRoW() &&
                        GetTargetHPPercent() >= MNK_AoE_RiddleOfWind_HP)
                        return RiddleOfWind;
                }
                if (IsEnabled(CustomComboPreset.MNK_AoEUsePerfectBalance) &&
                    UsePerfectBalanceAoE())
                    return PerfectBalance;

                if (IsEnabled(CustomComboPreset.MNK_AoE_ComboHeals))
                {
                    if (Role.CanSecondWind(MNK_AoE_SecondWind_Threshold))
                        return Role.SecondWind;

                    if (Role.CanBloodBath(MNK_AoE_Bloodbath_Threshold))
                        return Role.Bloodbath;
                }

                if (IsEnabled(CustomComboPreset.MNK_AoEUseHowlingFist) &&
                    Chakra >= 5 && HasBattleTarget() && InCombat() &&
                    LevelChecked(InspiritedMeditation) &&
                    !JustUsed(Brotherhood) && !JustUsed(RiddleOfFire))
                    return OriginalHook(InspiritedMeditation);
            }

            // Masterful Blitz
            if (IsEnabled(CustomComboPreset.MNK_AoEUseMasterfulBlitz) &&
                LevelChecked(MasterfulBlitz) &&
                !HasStatusEffect(Buffs.PerfectBalance) &&
                InMasterfulRange() &&
                !IsOriginal(MasterfulBlitz))
                return OriginalHook(MasterfulBlitz);

            if (IsEnabled(CustomComboPreset.MNK_AoEUseBuffs))
            {
                if (IsEnabled(CustomComboPreset.MNK_AoEUseFiresReply) &&
                    HasStatusEffect(Buffs.FiresRumination) &&
                    !HasStatusEffect(Buffs.FormlessFist) &&
                    !HasStatusEffect(Buffs.PerfectBalance) &&
                    !JustUsed(RiddleOfFire, 4))
                    return FiresReply;

                if (IsEnabled(CustomComboPreset.MNK_AoEUseWindsReply) &&
                    HasStatusEffect(Buffs.WindsRumination) &&
                    !HasStatusEffect(Buffs.PerfectBalance) &&
                    ((GetCooldownRemainingTime(RiddleOfFire) > 10) ||
                     HasStatusEffect(Buffs.RiddleOfFire)))
                    return WindsReply;
            }

            // Perfect Balance
            if (DoPerfectBalanceComboAoE(ref actionID))
                return actionID;

            // Monk Rotation
            if (HasStatusEffect(Buffs.OpoOpoForm))
                return OriginalHook(ArmOfTheDestroyer);

            if (HasStatusEffect(Buffs.RaptorForm))
            {
                if (LevelChecked(FourPointFury))
                    return FourPointFury;

                if (LevelChecked(TwinSnakes))
                    return TwinSnakes;
            }

            if (HasStatusEffect(Buffs.CoeurlForm) && LevelChecked(Rockbreaker))
                return Rockbreaker;

            return actionID;
        }
    }

    internal class MNK_PerfectBalance : CustomCombo
    {
        protected internal override CustomComboPreset Preset => CustomComboPreset.MNK_PerfectBalance;

        protected override uint Invoke(uint actionID) =>
            actionID is PerfectBalance &&
            OriginalHook(MasterfulBlitz) != MasterfulBlitz &&
            LevelChecked(MasterfulBlitz)
                ? OriginalHook(MasterfulBlitz)
                : actionID;
    }

    internal class MNK_Riddle_Brotherhood : CustomCombo
    {
        protected internal override CustomComboPreset Preset => CustomComboPreset.MNK_Riddle_Brotherhood;

        protected override uint Invoke(uint actionID) =>
            actionID is RiddleOfFire &&
            ActionReady(Brotherhood) && IsOnCooldown(RiddleOfFire)
                ? Brotherhood
                : actionID;
    }

    internal class MNK_Brotherhood_Riddle : CustomCombo
    {
        protected internal override CustomComboPreset Preset => CustomComboPreset.MNK_Brotherhood_Riddle;

        protected override uint Invoke(uint actionID) =>
            actionID is Brotherhood &&
            ActionReady(RiddleOfFire) && IsOnCooldown(Brotherhood)
                ? RiddleOfFire
                : actionID;
    }

    internal class MNK_BeastChakras : CustomCombo
    {
        protected internal override CustomComboPreset Preset => CustomComboPreset.MNK_ST_BeastChakras;

        protected override uint Invoke(uint actionID)
        {
            if (IsEnabled(CustomComboPreset.MNK_BC_OPOOPO) &&
                actionID is Bootshine or LeapingOpo)
                return OpoOpo is 0 && LevelChecked(DragonKick)
                    ? DragonKick
                    : OriginalHook(Bootshine);

            if (IsEnabled(CustomComboPreset.MNK_BC_RAPTOR) &&
                actionID is TrueStrike or RisingRaptor)
                return Raptor is 0 && LevelChecked(TwinSnakes)
                    ? TwinSnakes
                    : OriginalHook(TrueStrike);

            if (IsEnabled(CustomComboPreset.MNK_BC_COEURL) &&
                actionID is SnapPunch or PouncingCoeurl)
                return Coeurl is 0 && LevelChecked(Demolish)
                    ? Demolish
                    : OriginalHook(SnapPunch);

            return actionID;
        }
    }

    internal class MNK_PerfectBalanceProtection : CustomCombo
    {
        protected internal override CustomComboPreset Preset => CustomComboPreset.MNK_PerfectBalanceProtection;

        protected override uint Invoke(uint actionID) =>
            actionID is PerfectBalance && HasStatusEffect(Buffs.PerfectBalance) && LevelChecked(PerfectBalance)
                ? All.SavageBlade
                : actionID;
    }
}
