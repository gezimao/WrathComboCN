using ImGuiNET;
using WrathCombo.CustomComboNS;
using WrathCombo.CustomComboNS.Functions;
using WrathCombo.Window.Functions;

namespace WrathCombo.Combos.PvP
{
    internal static class MCHPvP
    {
        #region IDS

        public const byte JobID = 31;

        internal class Role : PvPPhysRanged;

        public const uint
            BlastCharge = 29402,
            BlazingShot = 41468,
            Scattergun = 29404,
            Drill = 29405,
            BioBlaster = 29406,
            AirAnchor = 29407,
            ChainSaw = 29408,
            Wildfire = 29409,
            BishopTurret = 29412,
            AetherMortar = 29413,
            Analysis = 29414,
            MarksmanSpite = 29415,
            FullMetalField = 41469;

        public static class Buffs
        {
            public const ushort
                Heat = 3148,
                Overheated = 3149,
                DrillPrimed = 3150,
                BioblasterPrimed = 3151,
                AirAnchorPrimed = 3152,
                ChainSawPrimed = 3153,
                Analysis = 3158;
        }

        public static class Debuffs
        {
            public const ushort
                Wildfire = 1323;
        }

        #endregion

        #region Config
        public static class Config
        {
            public static UserInt
                MCHPvP_MarksmanSpite = new("MCHPvP_MarksmanSpite"),
                MCHPvP_FMFOption = new("MCHPvP_FMFOption"),
                MCHPvP_EagleThreshold = new("MCHPvP_EagleThreshold");

            internal static void Draw(CustomComboPreset preset)
            {
                switch (preset)
                {
                    case CustomComboPreset.MCHPvP_BurstMode_MarksmanSpite:
                        UserConfig.DrawSliderInt(0, 36000, MCHPvP_MarksmanSpite,
                            "Use Marksman's Spite when the target is below set HP");

                        break;

                    case CustomComboPreset.MCHPvP_BurstMode_FullMetalField:
                        ImGui.Indent();
                        UserConfig.DrawHorizontalRadioButton(MCHPvP_FMFOption, "Full Metal Field Wildfire combo",
                            "Uses Full Metal Field when Wildfire is ready.", 1);

                        UserConfig.DrawHorizontalRadioButton(MCHPvP_FMFOption, "Full Metal Field only when Overheated",
                            "Only uses Full Metal Field while Overheated.", 2);
                        ImGui.Unindent();

                        break;

                    case CustomComboPreset.MCHPvP_Eagle:
                        UserConfig.DrawSliderInt(0, 100, MCHPvP_EagleThreshold,
                            "Target HP percent threshold to use Eagle Eye Shot Below.");

                        break;


                }
            }            
        }
#endregion

        internal class MCHPvP_BurstMode : CustomCombo
        {
            protected internal override CustomComboPreset Preset { get; } = CustomComboPreset.MCHPvP_BurstMode;

            protected override uint Invoke(uint actionID)
            {
                if (actionID == BlastCharge)
                {
                    #region Variables

                    var canWeave = CanWeave();
                    var analysisStacks = GetRemainingCharges(Analysis);
                    var bigDamageStacks = GetRemainingCharges(OriginalHook(Drill));
                    var overheated = HasStatusEffect(Buffs.Overheated);

                    #endregion

                    if (IsEnabled(CustomComboPreset.MCHPvP_Eagle) && PvPPhysRanged.CanEagleEyeShot() && (PvPCommon.TargetImmuneToDamage() || GetTargetHPPercent() <= Config.MCHPvP_EagleThreshold))
                        return PvPPhysRanged.EagleEyeShot;

                    if (!PvPCommon.TargetImmuneToDamage() && HasBattleTarget())
                    {
                        // MarksmanSpite execute condition - todo add config
                        if (IsEnabled(CustomComboPreset.MCHPvP_BurstMode_MarksmanSpite) && HasBattleTarget() && EnemyHealthCurrentHp() < Config.MCHPvP_MarksmanSpite && IsLB1Ready)
                            return MarksmanSpite;

                        if (IsEnabled(CustomComboPreset.MCHPvP_BurstMode_Wildfire) && canWeave && overheated && IsOffCooldown(Wildfire))
                            return OriginalHook(Wildfire);

                        // FullMetalField condition when not overheated or if overheated and FullMetalField is off cooldown
                        if (IsEnabled(CustomComboPreset.MCHPvP_BurstMode_FullMetalField) && IsOffCooldown(FullMetalField))
                        {
                            if (Config.MCHPvP_FMFOption == 1)
                            {
                                if (!overheated && IsOffCooldown(Wildfire))
                                    return FullMetalField;
                            }
                            if (Config.MCHPvP_FMFOption == 2)
                            {
                                if (overheated)
                                    return FullMetalField;
                            }
                        }

                        // Check if primed buffs and analysis conditions are met
                        bool hasPrimedBuffs = HasStatusEffect(Buffs.DrillPrimed) ||
                                              (HasStatusEffect(Buffs.ChainSawPrimed) && !IsEnabled(CustomComboPreset.MCHPvP_BurstMode_AltAnalysis)) ||
                                              (HasStatusEffect(Buffs.AirAnchorPrimed) && IsEnabled(CustomComboPreset.MCHPvP_BurstMode_AltAnalysis));

                        if (IsEnabled(CustomComboPreset.MCHPvP_BurstMode_Analysis))
                        {
                            if (hasPrimedBuffs && !HasStatusEffect(Buffs.Analysis) && analysisStacks > 0 &&
                                (!IsEnabled(CustomComboPreset.MCHPvP_BurstMode_AltDrill) || IsOnCooldown(Wildfire)) &&
                                !canWeave && !overheated && bigDamageStacks > 0)
                            {
                                return OriginalHook(Analysis);
                            }
                        }

                        // BigDamageStacks logic with checks for primed buffs
                        if (bigDamageStacks > 0)
                        {
                            if (IsEnabled(CustomComboPreset.MCHPvP_BurstMode_Drill) && HasStatusEffect(Buffs.DrillPrimed))
                                return OriginalHook(Drill);

                            if (IsEnabled(CustomComboPreset.MCHPvP_BurstMode_BioBlaster) && HasStatusEffect(Buffs.BioblasterPrimed) && HasBattleTarget() && GetTargetDistance() <= 12)
                                return OriginalHook(BioBlaster);

                            if (IsEnabled(CustomComboPreset.MCHPvP_BurstMode_AirAnchor) && HasStatusEffect(Buffs.AirAnchorPrimed))
                                return OriginalHook(AirAnchor);

                            if (IsEnabled(CustomComboPreset.MCHPvP_BurstMode_ChainSaw) && HasStatusEffect(Buffs.ChainSawPrimed))
                                return OriginalHook(ChainSaw);
                        }
                    }

                }

                return actionID;
            }
        }
    }
}
