﻿using Dalamud.Game.ClientState.JobGauge.Types;
using WrathCombo.Combos.PvE.Content;
using WrathCombo.CustomComboNS;
using WrathCombo.Extensions;

namespace WrathCombo.Combos.PvE;

internal partial class PCT : Caster
{

    #region Single Target Combos
    internal class PCT_ST_SimpleMode : CustomCombo
    {
        protected internal override CustomComboPreset Preset { get; } = CustomComboPreset.PCT_ST_SimpleMode;

        protected override uint Invoke(uint actionID)
        {
            if (actionID is not FireInRed)
                return actionID;

            #region Variants
            // Variant Cure
            if (Variant.CanCure(CustomComboPreset.PCT_Variant_Cure, Config.PCT_VariantCure))
                return Variant.Cure;

            // Variant Rampart
            if (Variant.CanRampart(CustomComboPreset.PCT_Variant_Rampart, WeaveTypes.SpellWeave))
                return Variant.Rampart;
            #endregion

            #region Prepull
            if ((!InCombat()) || (InCombat() && CurrentTarget == null))
            {
                if (CreatureMotifReady)
                    return OriginalHook(CreatureMotif);
                if (WeaponMotifReady)
                    return OriginalHook(WeaponMotif);
                if (LandscapeMotifReady)
                    return OriginalHook(LandscapeMotif);
            }
            #endregion

            if (OccultCrescent.ShouldUsePhantomActions())
                return OccultCrescent.BestPhantomAction();

            #region Burst Window lvl 100 only
            if (BurstPhaseReady)
                return BurstWindow(actionID);

            #endregion

            #region OGCD
            // General Weaves
            if (InCombat() && CanSpellWeave())
            {
                // ScenicMuse pre-100
                if (ScenicMuseReady && !LevelChecked(StarPrism))
                    return OriginalHook(ScenicMuse);

                // LivingMuse
                if (LivingMuseReady &&
                    (!PortraitReady || GetRemainingCharges(LivingMuse) == GetMaxCharges(LivingMuse)) &&
                    (!LevelChecked(ScenicMuse) || ScenicCD > GetCooldownChargeRemainingTime(LivingMuse)))
                    return OriginalHook(LivingMuse);

                // SteelMuse
                if (SteelMuseReady &&
                    (SteelCD < ScenicCD || GetRemainingCharges(SteelMuse) == GetMaxCharges(SteelMuse) ||
                    !LevelChecked(ScenicMuse)))
                    return OriginalHook(SteelMuse);

                // Portrait Mog or Madeen
                if (PortraitReady &&
                    IsOffCooldown(OriginalHook(MogoftheAges)) &&
                    (ScenicCD >= 60 || !LevelChecked(ScenicMuse)))
                    return OriginalHook(MogoftheAges);

                // SubtractivePalette
                if (PaletteReady)
                    return SubtractivePalette;

                //LucidDreaming
                if (Role.CanLucidDream(Config.PCT_ST_AdvancedMode_LucidOption))
                    return Role.LucidDreaming;
            }
            #endregion

            #region Swiftcast Motifs
            // Swiftcast Motifs
            if (HasStatusEffect(Role.Buffs.Swiftcast))
            {
                if (CreatureMotifReady)
                    return OriginalHook(CreatureMotif);
                if (WeaponMotifReady)
                    return OriginalHook(WeaponMotif);
                if (LandscapeMotifReady)
                    return OriginalHook(LandscapeMotif);
            }

            #endregion

            #region Moving
            if (IsMoving() && InCombat())
            {
                if (HyperPhantasiaMovementPaint())
                    return HasStatusEffect(Buffs.MonochromeTones) ? OriginalHook(CometinBlack) : OriginalHook(HolyInWhite);

                if (HammerStamp.LevelChecked() && HasStatusEffect(Buffs.HammerTime))
                    return OriginalHook(HammerStamp);

                if (CometinBlack.LevelChecked() && HasPaint && HasStatusEffect(Buffs.MonochromeTones))
                    return OriginalHook(CometinBlack);

                if (HasStatusEffect(Buffs.RainbowBright))
                    return RainbowDrip;

                if (HolyInWhite.LevelChecked() && HasPaint & !HasStatusEffect(Buffs.MonochromeTones))
                    return OriginalHook(HolyInWhite);

                if (ActionReady(Role.Swiftcast) &&
                    (CreatureMotifReady || WeaponMotifReady || LandscapeMotifReady))
                    return Role.Swiftcast;
            }

            #endregion

            #region Pre_Burst Motifs

            //Prepare for Burst
            if (GetCooldownRemainingTime(ScenicMuse) <= 20)
            {
                if (LandscapeMotifReady && GetTargetHPPercent() > 10)
                    return OriginalHook(LandscapeMotif);

                if (CreatureMotifReady && GetTargetHPPercent() > 10)
                    return OriginalHook(CreatureMotif);

                if (WeaponMotifReady && GetTargetHPPercent() > 10)
                    return OriginalHook(WeaponMotif);
            }

            #endregion

            #region GCDs

            //StarPrism
            if (HasStatusEffect(Buffs.Starstruck))
                return StarPrism;

            //RainbowDrip
            if (HasStatusEffect(Buffs.RainbowBright))
                return RainbowDrip;

            //Comet in Black
            if (CometinBlack.LevelChecked() && HasStatusEffect(Buffs.MonochromeTones) && HasPaint)
                return OriginalHook(CometinBlack);

            //Hammer Stamp Combo
            if (ActionReady(OriginalHook(HammerStamp)) && ScenicCD > 10)
                return OriginalHook(HammerStamp);

            // LandscapeMotif
            if ( LandscapeMotifReady && GetTargetHPPercent() > 10 &&
                GetCooldownRemainingTime(ScenicMuse) <= 20)
                return OriginalHook(LandscapeMotif);

            // CreatureMotif
            if (CreatureMotifReady && GetTargetHPPercent() > 10 &&
                (HasCharges(LivingMuse) || GetCooldownChargeRemainingTime(LivingMuse) <= 8))
                return OriginalHook(CreatureMotif);

            // WeaponMotif
            if (WeaponMotifReady && GetTargetHPPercent() > 10 &&
                (HasCharges(SteelMuse) || GetCooldownChargeRemainingTime(SteelMuse) <= 8))
                return OriginalHook(WeaponMotif);

            //Subtractive Combo
            if (BlizzardIIinCyan.LevelChecked() && HasStatusEffect(Buffs.SubtractivePalette))
                return OriginalHook(BlizzardinCyan);

            return actionID;
            #endregion
        }

    }   

    internal class PCT_ST_AdvancedMode : CustomCombo
    {
        protected internal override CustomComboPreset Preset { get; } = CustomComboPreset.PCT_ST_AdvancedMode;

        protected override uint Invoke(uint actionID)
        {
            if (actionID is not FireInRed)
                return actionID;

            #region Enables and Configs
           
            int creatureStop = Config.PCT_ST_CreatureStop;
            int landscapeStop = Config.PCT_ST_LandscapeStop;
            int weaponStop = Config.PCT_ST_WeaponStop;

            bool prepullEnabled = IsEnabled(CustomComboPreset.PCT_ST_AdvancedMode_PrePullMotifs);
            bool openerEnabled = IsEnabled(CustomComboPreset.PCT_ST_Advanced_Openers);
            bool burstPhaseEnabled = IsEnabled(CustomComboPreset.PCT_ST_AdvancedMode_Burst_Phase);
            bool noTargetMotifEnabled = IsEnabled(CustomComboPreset.PCT_ST_AdvancedMode_NoTargetMotifs);
            bool scenicMuseEnabled = IsEnabled(CustomComboPreset.PCT_ST_AdvancedMode_ScenicMuse);
            bool livingMuseEnabled = IsEnabled(CustomComboPreset.PCT_ST_AdvancedMode_LivingMuse);
            bool steelMuseEnabled = IsEnabled(CustomComboPreset.PCT_ST_AdvancedMode_SteelMuse);
            bool portraitEnabled = IsEnabled(CustomComboPreset.PCT_ST_AdvancedMode_MogOfTheAges);
            bool paletteEnabled = IsEnabled(CustomComboPreset.PCT_ST_AdvancedMode_SubtractivePalette);
            bool lucidDreamingEnabled = IsEnabled(CustomComboPreset.PCT_ST_AdvancedMode_LucidDreaming);
            bool swiftMotifEnabled = IsEnabled(CustomComboPreset.PCT_ST_AdvancedMode_SwiftMotifs);
            bool hammerMovementEnabled = IsEnabled(CustomComboPreset.PCT_ST_AdvancedMode_MovementOption_HammerStampCombo);
            bool cometMovementEnabled = IsEnabled(CustomComboPreset.PCT_ST_AdvancedMode_MovementOption_CometinBlack);
            bool holyMovementEnabled = IsEnabled(CustomComboPreset.PCT_ST_AdvancedMode_MovementOption_HolyInWhite);
            bool swiftcastMotifMovementEnabled = IsEnabled(CustomComboPreset.PCT_ST_AdvancedMode_SwitfcastOption);
            bool landscapeMotifEnabled = IsEnabled(CustomComboPreset.PCT_ST_AdvancedMode_LandscapeMotif);
            bool creatureMotifEnabled = IsEnabled(CustomComboPreset.PCT_ST_AdvancedMode_CreatureMotif);
            bool weaponMotifEnabled = IsEnabled(CustomComboPreset.PCT_ST_AdvancedMode_WeaponMotif);
            bool starPrismEnabled = IsEnabled(CustomComboPreset.PCT_ST_AdvancedMode_StarPrism);
            bool rainbowDripEnabled = IsEnabled(CustomComboPreset.PCT_ST_AdvancedMode_RainbowDrip);
            bool cometEnabled = IsEnabled(CustomComboPreset.PCT_ST_AdvancedMode_CometinBlack);
            bool hammerEnabled = IsEnabled(CustomComboPreset.PCT_ST_AdvancedMode_HammerStampCombo);
            bool blizzardComboEnabled = IsEnabled(CustomComboPreset.PCT_ST_AdvancedMode_BlizzardInCyan);




            #endregion

            #region Variants
            // Variant Cure
            if (Variant.CanCure(CustomComboPreset.PCT_Variant_Cure, Config.PCT_VariantCure))
                return Variant.Cure;

            // Variant Rampart
            if (Variant.CanRampart(CustomComboPreset.PCT_Variant_Rampart, WeaveTypes.SpellWeave))
                return Variant.Rampart;
            #endregion

            #region Prepull
            if ((prepullEnabled && !InCombat()) || (noTargetMotifEnabled && InCombat() && CurrentTarget == null))
            {
                if (CreatureMotifReady)
                    return OriginalHook(CreatureMotif);
                if (WeaponMotifReady)
                    return OriginalHook(WeaponMotif);
                if (LandscapeMotifReady)
                    return OriginalHook(LandscapeMotif);
            }
            #endregion

            if (OccultCrescent.ShouldUsePhantomActions())
                return OccultCrescent.BestPhantomAction();

            #region Opener

            if (openerEnabled && Opener().FullOpener(ref actionID))
                return actionID;

            #endregion

            #region Burst Window
            if (burstPhaseEnabled && BurstPhaseReady)
                return BurstWindow(actionID);

            #endregion

            #region OGCD
            // General Weaves
            if (InCombat() && CanSpellWeave())
            {
                // ScenicMuse
                if (scenicMuseEnabled && ScenicMuseReady && (!burstPhaseEnabled || !LevelChecked(StarPrism)))
                    return OriginalHook(ScenicMuse);
                    
                // LivingMuse
                if (livingMuseEnabled && LivingMuseReady && 
                    (!PortraitReady || GetRemainingCharges(LivingMuse) == GetMaxCharges(LivingMuse)) &&
                    (!LevelChecked(ScenicMuse) || ScenicCD > GetCooldownChargeRemainingTime(LivingMuse)))
                    return OriginalHook(LivingMuse);
                        
                // SteelMuse
                if (steelMuseEnabled && SteelMuseReady &&
                    (SteelCD < ScenicCD || GetRemainingCharges(SteelMuse) == GetMaxCharges(SteelMuse) ||
                    !LevelChecked(ScenicMuse)))
                    return OriginalHook(SteelMuse);
                    
                // Portrait Mog or Madeen
                if (portraitEnabled && PortraitReady && 
                    IsOffCooldown(OriginalHook(MogoftheAges)) && 
                    (ScenicCD >= 60 || !LevelChecked(ScenicMuse)))
                    return OriginalHook(MogoftheAges);
                    
                // SubtractivePalette
                if (paletteEnabled && PaletteReady)
                    return SubtractivePalette;
                        
                //LucidDreaming
                if (lucidDreamingEnabled && Role.CanLucidDream(Config.PCT_ST_AdvancedMode_LucidOption))
                    return Role.LucidDreaming;
            }
            #endregion

            #region Swiftcast Motifs
            // Swiftcast Motifs
            if (swiftMotifEnabled && HasStatusEffect(Role.Buffs.Swiftcast))
            {
                if (CreatureMotifReady && GetTargetHPPercent() > creatureStop)
                    return OriginalHook(CreatureMotif);
                if (WeaponMotifReady && GetTargetHPPercent() > weaponStop)
                    return OriginalHook(WeaponMotif);
                if (LandscapeMotifReady && GetTargetHPPercent() > landscapeStop)
                    return OriginalHook(LandscapeMotif);
            }

            #endregion

            #region Moving
            if (IsMoving() && InCombat())
            {
                if (HyperPhantasiaMovementPaint())
                    return HasStatusEffect(Buffs.MonochromeTones) ? OriginalHook(CometinBlack) : OriginalHook(HolyInWhite);

                if (hammerMovementEnabled && HammerStamp.LevelChecked() && HasStatusEffect(Buffs.HammerTime))
                    return OriginalHook(HammerStamp);

                if (cometMovementEnabled && CometinBlack.LevelChecked() && HasPaint && HasStatusEffect(Buffs.MonochromeTones))
                    return OriginalHook(CometinBlack);

                if (rainbowDripEnabled && HasStatusEffect(Buffs.RainbowBright))
                    return RainbowDrip;

                if (holyMovementEnabled && HolyInWhite.LevelChecked() && HasPaint & !HasStatusEffect(Buffs.MonochromeTones))
                    return OriginalHook(HolyInWhite);

                if (swiftcastMotifMovementEnabled && ActionReady(Role.Swiftcast) &&
                    (CreatureMotifReady || WeaponMotifReady || LandscapeMotifReady))
                    return Role.Swiftcast;
            }

            #endregion

            #region Pre_Burst Motifs

            //Prepare for Burst
            if (GetCooldownRemainingTime(ScenicMuse) <= 20)
            {
                if (landscapeMotifEnabled && LandscapeMotifReady && GetTargetHPPercent() > landscapeStop)
                    return OriginalHook(LandscapeMotif);

                if (creatureMotifEnabled && CreatureMotifReady && GetTargetHPPercent() > creatureStop)
                    return OriginalHook(CreatureMotif);

                if (weaponMotifEnabled && WeaponMotifReady && GetTargetHPPercent() > weaponStop)
                    return OriginalHook(WeaponMotif);
            }

            #endregion

            #region GCDs

            //StarPrism
            if (starPrismEnabled && HasStatusEffect(Buffs.Starstruck))
                return StarPrism;

            //RainbowDrip
            if (rainbowDripEnabled && HasStatusEffect(Buffs.RainbowBright))
                return RainbowDrip;

            //Comet in Black
            if (cometEnabled && CometinBlack.LevelChecked() && HasStatusEffect(Buffs.MonochromeTones) && HasPaint)
                return OriginalHook(CometinBlack);

            //Hammer Stamp Combo
            if (hammerEnabled && ActionReady(OriginalHook(HammerStamp)) && ScenicCD > 10)
                return OriginalHook(HammerStamp);
            
            // LandscapeMotif
            if (landscapeMotifEnabled && LandscapeMotifReady && GetTargetHPPercent() > landscapeStop && 
                GetCooldownRemainingTime(ScenicMuse) <= 20)
                return OriginalHook(LandscapeMotif);

            // CreatureMotif
            if (creatureMotifEnabled && CreatureMotifReady &&  GetTargetHPPercent() > creatureStop &&
                (HasCharges(LivingMuse) || GetCooldownChargeRemainingTime(LivingMuse) <= 8))
                return OriginalHook(CreatureMotif);                  

            // WeaponMotif
            if (weaponMotifEnabled && WeaponMotifReady && GetTargetHPPercent() > weaponStop && 
                (HasCharges(SteelMuse) || GetCooldownChargeRemainingTime(SteelMuse) <= 8))
                return OriginalHook(WeaponMotif);            

            //Subtractive Combo
            if (blizzardComboEnabled && BlizzardIIinCyan.LevelChecked() && HasStatusEffect(Buffs.SubtractivePalette))
                return OriginalHook(BlizzardinCyan);

            return actionID;
            #endregion
        }
    }

    #endregion

    #region AOE Combos

    internal class PCT_AoE_SimpleMode : CustomCombo
    {
        protected internal override CustomComboPreset Preset { get; } = CustomComboPreset.PCT_AoE_SimpleMode;

        protected override uint Invoke(uint actionID)
        {
            if (actionID is not FireIIinRed)
                return actionID;

            #region Enables and Configs

            int creatureStop = 10;
            int landscapeStop = 10;
            int weaponStop = 10;
            int holdPaintCharges = 2;

            #endregion

            #region Variants
            // Variant Cure
            if (Variant.CanCure(CustomComboPreset.PCT_Variant_Cure, Config.PCT_VariantCure))
                return Variant.Cure;

            // Variant Rampart
            if (Variant.CanRampart(CustomComboPreset.PCT_Variant_Rampart, WeaveTypes.SpellWeave))
                return Variant.Rampart;
            #endregion

            #region Prepull
            if ((!InCombat()) || (InCombat() && CurrentTarget == null))
            {
                if (CreatureMotifReady)
                    return OriginalHook(CreatureMotif);
                if (WeaponMotifReady)
                    return OriginalHook(WeaponMotif);
                if (LandscapeMotifReady)
                    return OriginalHook(LandscapeMotif);
            }
            #endregion

            if (OccultCrescent.ShouldUsePhantomActions())
                return OccultCrescent.BestPhantomAction();

            #region OGCD
            // General Weaves
            if (InCombat() && CanSpellWeave())
            {
                // ScenicMuse
                if (ScenicMuseReady)
                    return OriginalHook(ScenicMuse);

                // LivingMuse
                if (LivingMuseReady &&
                    (!PortraitReady || GetRemainingCharges(LivingMuse) == GetMaxCharges(LivingMuse)) &&
                    (!LevelChecked(ScenicMuse) || ScenicCD > GetCooldownChargeRemainingTime(LivingMuse)))
                    return OriginalHook(LivingMuse);

                // SteelMuse
                if (SteelMuseReady &&
                    (SteelCD < ScenicCD || GetRemainingCharges(SteelMuse) == GetMaxCharges(SteelMuse) ||
                    !LevelChecked(ScenicMuse)))
                    return OriginalHook(SteelMuse);

                // Portrait Mog or Madeen
                if (PortraitReady &&
                    IsOffCooldown(OriginalHook(MogoftheAges)) &&
                    (ScenicCD >= 60 || !LevelChecked(ScenicMuse)))
                    return OriginalHook(MogoftheAges);

                // SubtractivePalette
                if (PaletteReady)
                    return SubtractivePalette;

                //LucidDreaming
                if (Role.CanLucidDream(Config.PCT_ST_AdvancedMode_LucidOption))
                    return Role.LucidDreaming;
            }
            #endregion

            #region Swiftcast Motifs
            // Swiftcast Motifs
            if (HasStatusEffect(Role.Buffs.Swiftcast))
            {
                if (CreatureMotifReady && GetTargetHPPercent() > creatureStop)
                    return OriginalHook(CreatureMotif);
                if (WeaponMotifReady && GetTargetHPPercent() > weaponStop)
                    return OriginalHook(WeaponMotif);
                if (LandscapeMotifReady && GetTargetHPPercent() > landscapeStop)
                    return OriginalHook(LandscapeMotif);
            }

            #endregion

            #region Moving
            if (IsMoving() && InCombat())
            {
                if (HyperPhantasiaMovementPaint())
                    return HasStatusEffect(Buffs.MonochromeTones) ? OriginalHook(CometinBlack) : OriginalHook(HolyInWhite);

                if (HammerStamp.LevelChecked() && HasStatusEffect(Buffs.HammerTime))
                    return OriginalHook(HammerStamp);

                if (CometinBlack.LevelChecked() && HasPaint && HasStatusEffect(Buffs.MonochromeTones))
                    return OriginalHook(CometinBlack);

                if (HasStatusEffect(Buffs.RainbowBright))
                    return RainbowDrip;

                if (HolyInWhite.LevelChecked() && HasPaint & !HasStatusEffect(Buffs.MonochromeTones))
                    return OriginalHook(HolyInWhite);

                if (ActionReady(Role.Swiftcast) &&
                    (CreatureMotifReady || WeaponMotifReady || LandscapeMotifReady))
                    return Role.Swiftcast;
            }

            #endregion

            #region Pre_Burst Motifs

            //Prepare for Burst
            if (GetCooldownRemainingTime(ScenicMuse) <= 20)
            {
                if (LandscapeMotifReady && GetTargetHPPercent() > landscapeStop)
                    return OriginalHook(LandscapeMotif);

                if (CreatureMotifReady && GetTargetHPPercent() > creatureStop)
                    return OriginalHook(CreatureMotif);

                if (WeaponMotifReady && GetTargetHPPercent() > weaponStop)
                    return OriginalHook(WeaponMotif);
            }

            #endregion

            #region GCDs

            //StarPrism
            if (HasStatusEffect(Buffs.Starstruck))
                return StarPrism;

            //RainbowDrip
            if (HasStatusEffect(Buffs.RainbowBright))
                return RainbowDrip;

            //Comet in Black
            if (CometinBlack.LevelChecked() && HasStatusEffect(Buffs.MonochromeTones) && HasPaint)
                return OriginalHook(CometinBlack);

            //Hammer Stamp Combo
            if (ActionReady(OriginalHook(HammerStamp)) && ScenicCD > 10)
                return OriginalHook(HammerStamp);

            // LandscapeMotif
            if (LandscapeMotifReady && GetTargetHPPercent() > landscapeStop &&
                GetCooldownRemainingTime(ScenicMuse) <= 20)
                return OriginalHook(LandscapeMotif);

            // CreatureMotif
            if (CreatureMotifReady && GetTargetHPPercent() > creatureStop &&
                (HasCharges(LivingMuse) || GetCooldownChargeRemainingTime(LivingMuse) <= 8))
                return OriginalHook(CreatureMotif);

            // WeaponMotif
            if (WeaponMotifReady && GetTargetHPPercent() > weaponStop &&
                (HasCharges(SteelMuse) || GetCooldownChargeRemainingTime(SteelMuse) <= 8))
                return OriginalHook(WeaponMotif);

            //Subtractive Combo
            if (BlizzardIIinCyan.LevelChecked() && HasStatusEffect(Buffs.SubtractivePalette))
                return OriginalHook(BlizzardIIinCyan);

            //Holy In White
            if (!HasStatusEffect(Buffs.StarryMuse) && !HasStatusEffect(Buffs.MonochromeTones) && gauge.Paint > holdPaintCharges)
                return OriginalHook(HolyInWhite);

            return actionID;

            #endregion

        }
    }

    internal class PCT_AoE_AdvancedMode : CustomCombo
    {
        protected internal override CustomComboPreset Preset { get; } = CustomComboPreset.PCT_AoE_AdvancedMode;

        protected override uint Invoke(uint actionID)
        {
            if (actionID is not FireIIinRed)
                return actionID;

            #region Enables and Configs

            int creatureStop = Config.PCT_AoE_CreatureStop;
            int landscapeStop = Config.PCT_AoE_LandscapeStop;
            int weaponStop = Config.PCT_AoE_WeaponStop;
            int holdPaintCharges = Config.PCT_AoE_AdvancedMode_HolyinWhiteOption;

            bool prepullEnabled = IsEnabled(CustomComboPreset.PCT_AoE_AdvancedMode_PrePullMotifs);
            bool noTargetMotifEnabled = IsEnabled(CustomComboPreset.PCT_AoE_AdvancedMode_NoTargetMotifs);
            bool scenicMuseEnabled = IsEnabled(CustomComboPreset.PCT_AoE_AdvancedMode_ScenicMuse);
            bool livingMuseEnabled = IsEnabled(CustomComboPreset.PCT_AoE_AdvancedMode_LivingMuse);
            bool steelMuseEnabled = IsEnabled(CustomComboPreset.PCT_AoE_AdvancedMode_SteelMuse);
            bool portraitEnabled = IsEnabled(CustomComboPreset.PCT_AoE_AdvancedMode_MogOfTheAges);
            bool paletteEnabled = IsEnabled(CustomComboPreset.PCT_AoE_AdvancedMode_SubtractivePalette);
            bool lucidDreamingEnabled = IsEnabled(CustomComboPreset.PCT_AoE_AdvancedMode_LucidDreaming);
            bool swiftMotifEnabled = IsEnabled(CustomComboPreset.PCT_AoE_AdvancedMode_SwiftMotifs);
            bool hammerMovementEnabled = IsEnabled(CustomComboPreset.PCT_AoE_AdvancedMode_MovementOption_HammerStampCombo);
            bool cometMovementEnabled = IsEnabled(CustomComboPreset.PCT_AoE_AdvancedMode_MovementOption_CometinBlack);
            bool holyMovementEnabled = IsEnabled(CustomComboPreset.PCT_AoE_AdvancedMode_MovementOption_HolyInWhite);
            bool swiftcastMotifMovementEnabled = IsEnabled(CustomComboPreset.PCT_AoE_AdvancedMode_SwitfcastOption);
            bool landscapeMotifEnabled = IsEnabled(CustomComboPreset.PCT_AoE_AdvancedMode_LandscapeMotif);
            bool creatureMotifEnabled = IsEnabled(CustomComboPreset.PCT_AoE_AdvancedMode_CreatureMotif);
            bool weaponMotifEnabled = IsEnabled(CustomComboPreset.PCT_AoE_AdvancedMode_WeaponMotif);
            bool starPrismEnabled = IsEnabled(CustomComboPreset.PCT_AoE_AdvancedMode_StarPrism);
            bool rainbowDripEnabled = IsEnabled(CustomComboPreset.PCT_AoE_AdvancedMode_RainbowDrip);
            bool cometEnabled = IsEnabled(CustomComboPreset.PCT_AoE_AdvancedMode_CometinBlack);
            bool hammerEnabled = IsEnabled(CustomComboPreset.PCT_AoE_AdvancedMode_HammerStampCombo);
            bool blizzardComboEnabled = IsEnabled(CustomComboPreset.PCT_AoE_AdvancedMode_BlizzardInCyan);
            bool holyInWhiteEnabled = IsEnabled(CustomComboPreset.PCT_AoE_AdvancedMode_HolyinWhite);

            #endregion

            #region Variants
            // Variant Cure
            if (Variant.CanCure(CustomComboPreset.PCT_Variant_Cure, Config.PCT_VariantCure))
                return Variant.Cure;

            // Variant Rampart
            if (Variant.CanRampart(CustomComboPreset.PCT_Variant_Rampart, WeaveTypes.SpellWeave))
                return Variant.Rampart;
            #endregion

            #region Prepull
            if ((prepullEnabled && !InCombat()) || (noTargetMotifEnabled && InCombat() && CurrentTarget == null))
            {
                if (CreatureMotifReady)
                    return OriginalHook(CreatureMotif);
                if (WeaponMotifReady)
                    return OriginalHook(WeaponMotif);
                if (LandscapeMotifReady)
                    return OriginalHook(LandscapeMotif);
            }
            #endregion

            if (OccultCrescent.ShouldUsePhantomActions())
                return OccultCrescent.BestPhantomAction();

            #region OGCD
            // General Weaves
            if (InCombat() && CanSpellWeave())
            {
                // ScenicMuse
                if (scenicMuseEnabled && ScenicMuseReady)
                    return OriginalHook(ScenicMuse);

                // LivingMuse
                if (livingMuseEnabled && LivingMuseReady &&
                    (!PortraitReady || GetRemainingCharges(LivingMuse) == GetMaxCharges(LivingMuse)) &&
                    (!LevelChecked(ScenicMuse) || ScenicCD > GetCooldownChargeRemainingTime(LivingMuse)))
                    return OriginalHook(LivingMuse);

                // SteelMuse
                if (steelMuseEnabled && SteelMuseReady &&
                    (SteelCD < ScenicCD || GetRemainingCharges(SteelMuse) == GetMaxCharges(SteelMuse) ||
                    !LevelChecked(ScenicMuse)))
                    return OriginalHook(SteelMuse);

                // Portrait Mog or Madeen
                if (portraitEnabled && PortraitReady &&
                    IsOffCooldown(OriginalHook(MogoftheAges)) &&
                    (ScenicCD >= 60 || !LevelChecked(ScenicMuse)))
                    return OriginalHook(MogoftheAges);

                // SubtractivePalette
                if (paletteEnabled && PaletteReady)
                    return SubtractivePalette;

                //LucidDreaming
                if (lucidDreamingEnabled && Role.CanLucidDream(Config.PCT_ST_AdvancedMode_LucidOption))
                    return Role.LucidDreaming;
            }
            #endregion

            #region Swiftcast Motifs
            // Swiftcast Motifs
            if (swiftMotifEnabled && HasStatusEffect(Role.Buffs.Swiftcast))
            {
                if (CreatureMotifReady && GetTargetHPPercent() > creatureStop)
                    return OriginalHook(CreatureMotif);
                if (WeaponMotifReady && GetTargetHPPercent() > weaponStop)
                    return OriginalHook(WeaponMotif);
                if (LandscapeMotifReady && GetTargetHPPercent() > landscapeStop)
                    return OriginalHook(LandscapeMotif);
            }

            #endregion

            #region Moving
            if (IsMoving() && InCombat())
            {
                if (HyperPhantasiaMovementPaint())
                    return HasStatusEffect(Buffs.MonochromeTones) ? OriginalHook(CometinBlack) : OriginalHook(HolyInWhite);

                if (hammerMovementEnabled && HammerStamp.LevelChecked() && HasStatusEffect(Buffs.HammerTime))
                    return OriginalHook(HammerStamp);

                if (cometMovementEnabled && CometinBlack.LevelChecked() && HasPaint && HasStatusEffect(Buffs.MonochromeTones))
                    return OriginalHook(CometinBlack);

                if (rainbowDripEnabled && HasStatusEffect(Buffs.RainbowBright))
                    return RainbowDrip;

                if (holyMovementEnabled && HolyInWhite.LevelChecked() && HasPaint & !HasStatusEffect(Buffs.MonochromeTones))
                    return OriginalHook(HolyInWhite);

                if (swiftcastMotifMovementEnabled && ActionReady(Role.Swiftcast) &&
                    (CreatureMotifReady || WeaponMotifReady || LandscapeMotifReady))
                    return Role.Swiftcast;
            }

            #endregion

            #region Pre_Burst Motifs

            //Prepare for Burst
            if (GetCooldownRemainingTime(ScenicMuse) <= 20)
            {
                if (landscapeMotifEnabled && LandscapeMotifReady && GetTargetHPPercent() > landscapeStop)
                    return OriginalHook(LandscapeMotif);

                if (creatureMotifEnabled && CreatureMotifReady && GetTargetHPPercent() > creatureStop)
                    return OriginalHook(CreatureMotif);

                if (weaponMotifEnabled && WeaponMotifReady && GetTargetHPPercent() > weaponStop)
                    return OriginalHook(WeaponMotif);
            }

            #endregion

            #region GCDs

            //StarPrism
            if (starPrismEnabled && HasStatusEffect(Buffs.Starstruck))
                return StarPrism;

            //RainbowDrip
            if (rainbowDripEnabled && HasStatusEffect(Buffs.RainbowBright))
                return RainbowDrip;

            //Comet in Black
            if (cometEnabled && CometinBlack.LevelChecked() && HasStatusEffect(Buffs.MonochromeTones) && HasPaint)
                return OriginalHook(CometinBlack);

            //Hammer Stamp Combo
            if (hammerEnabled && ActionReady(OriginalHook(HammerStamp)) && ScenicCD > 10)
                return OriginalHook(HammerStamp);

            // LandscapeMotif
            if (landscapeMotifEnabled && LandscapeMotifReady && GetTargetHPPercent() > landscapeStop &&
                GetCooldownRemainingTime(ScenicMuse) <= 20)
                return OriginalHook(LandscapeMotif);

            // CreatureMotif
            if (creatureMotifEnabled && CreatureMotifReady && GetTargetHPPercent() > creatureStop &&
                (HasCharges(LivingMuse) || GetCooldownChargeRemainingTime(LivingMuse) <= 8))
                return OriginalHook(CreatureMotif);

            // WeaponMotif
            if (weaponMotifEnabled && WeaponMotifReady && GetTargetHPPercent() > weaponStop &&
                (HasCharges(SteelMuse) || GetCooldownChargeRemainingTime(SteelMuse) <= 8))
                return OriginalHook(WeaponMotif);

            //Subtractive Combo
            if (blizzardComboEnabled && BlizzardIIinCyan.LevelChecked() && HasStatusEffect(Buffs.SubtractivePalette))
                return OriginalHook(BlizzardIIinCyan);

            //Holy In White
            if (holyInWhiteEnabled && !HasStatusEffect(Buffs.StarryMuse) && !HasStatusEffect(Buffs.MonochromeTones) && gauge.Paint > holdPaintCharges)
                return OriginalHook(HolyInWhite);

            return actionID;

            #endregion
        }
    }

    #endregion

    #region Smaller Features

    internal class CombinedAetherhues : CustomCombo
    {
        protected internal override CustomComboPreset Preset { get; } = CustomComboPreset.CombinedAetherhues;

        protected override uint Invoke(uint actionID)
        {
            if (actionID is not (FireInRed or FireIIinRed))
                return actionID;

            int choice = Config.CombinedAetherhueChoices;

            if (actionID == FireInRed && choice is 0 or 1)
            {
                if (HasStatusEffect(Buffs.SubtractivePalette))
                    return OriginalHook(BlizzardinCyan);
            }

            if (actionID == FireIIinRed && choice is 0 or 2)
            {
                if (HasStatusEffect(Buffs.SubtractivePalette))
                    return OriginalHook(BlizzardIIinCyan);
            }

            return actionID;
        }
    }

    internal class CombinedMotifs : CustomCombo
    {
        protected internal override CustomComboPreset Preset { get; } = CustomComboPreset.CombinedMotifs;

        protected override uint Invoke(uint actionID)
        {
            PCTGauge gauge = GetJobGauge<PCTGauge>();

            if (actionID == CreatureMotif)
            {
                if ((Config.CombinedMotifsMog && gauge.MooglePortraitReady) || (Config.CombinedMotifsMadeen && gauge.MadeenPortraitReady && IsOffCooldown(OriginalHook(MogoftheAges))))
                    return OriginalHook(MogoftheAges);

                if (gauge.CreatureMotifDrawn)
                    return OriginalHook(LivingMuse);
            }

            if (actionID == WeaponMotif)
            {
                if (Config.CombinedMotifsWeapon && HasStatusEffect(Buffs.HammerTime))
                    return OriginalHook(HammerStamp);

                if (gauge.WeaponMotifDrawn)
                    return OriginalHook(SteelMuse);
            }

            if (actionID == LandscapeMotif)
            {
                if (Config.CombinedMotifsLandscape && HasStatusEffect(Buffs.Starstruck))
                    return OriginalHook(StarPrism);

                if (gauge.LandscapeMotifDrawn)
                    return OriginalHook(ScenicMuse);
            }

            return actionID;
        }
    }

    internal class CombinedPaint : CustomCombo
    {
        protected internal override CustomComboPreset Preset { get; } = CustomComboPreset.CombinedPaint;

        protected override uint Invoke(uint actionID)
        {
            if (actionID != HolyInWhite)
                return actionID;
            if (HasStatusEffect(Buffs.MonochromeTones))
                return CometinBlack;
            return actionID;
        }
    }

    #endregion
}
