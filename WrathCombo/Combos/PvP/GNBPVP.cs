﻿using WrathCombo.CustomComboNS;
using WrathCombo.CustomComboNS.Functions;
using WrathCombo.Window.Functions;

namespace WrathCombo.Combos.PvP
{
    internal static class GNBPvP
    {
        #region IDS
        public const byte JobID = 37;

        internal class Role : PvPTank;

        public const uint
            KeenEdge = 29098,
            BrutalShell = 29099,
            SolidBarrel = 29100,
            BurstStrike = 29101,
            GnashingFang = 29102,
            SavageClaw = 29103,
            WickedTalon = 29104,
            Continuation = 29106,
            JugularRip = 29108,
            AbdomenTear = 29109,
            EyeGouge = 29110,
            RoughDivide = 29123,
            RelentlessRush = 29130,
            TerminalTrigger = 29131,
            FatedCircle = 41511,
            FatedBrand = 41442,
            BlastingZone = 29128,
            HeartOfCorundum = 41443;


        internal class Debuffs
        {
            internal const ushort
                Stun = 1343;
        }

        internal class Buffs
        {
            internal const ushort
                ReadyToRip = 2002,
                ReadyToTear = 2003,
                ReadyToGouge = 2004,
                ReadyToBlast = 3041,
                NoMercy = 3042,
                PowderBarrel = 3043,
                JugularRip = 3048,
                AbdomenTear = 3049,
                EyeGouge = 3050,
                ReadyToRaze = 4293;

        }

        #endregion

        #region Config
        public static class Config
        {
            public static UserInt
                GNBPvP_CorundumThreshold = new("GNBPvP_CorundumThreshold"),
                GNBPvP_BlastingZoneThreshold = new("GNBPvP_BlastingZoneThreshold"),
                GNBPvP_RampartThreshold = new("GNBPvP_RampartThreshold");

            internal static void Draw(CustomComboPreset preset)
            {
                switch (preset)
                {
                    case CustomComboPreset.GNBPvP_Rampart:
                        UserConfig.DrawSliderInt(1, 100, GNBPvP_RampartThreshold,
                            "Use Rampart below set threshold for self");
                        break;

                    case CustomComboPreset.GNBPvP_Corundum:
                        UserConfig.DrawSliderInt(1, 100,
                            GNBPvP_CorundumThreshold,
                            "HP% to be at or Below to use " +
                            "(100 = Use Always)",
                            itemWidth: 150f, sliderIncrement: SliderIncrements.Fives);
                        break;

                    case CustomComboPreset.GNBPvP_BlastingZone:

                        UserConfig.DrawSliderInt(1, 100,
                            GNBPvP_BlastingZoneThreshold,
                            "Hp % of target to use Blasting zone. Most powerful below 50% " +
                            "(100 = Use Always)",
                            itemWidth: 150f, sliderIncrement: SliderIncrements.Fives);
                        break;

                }
            }
        }
        #endregion
            
        internal class GNBPvP_Burst : CustomCombo
        {
            protected internal override CustomComboPreset Preset { get; } = CustomComboPreset.GNBPvP_Burst;
            
            protected override uint Invoke(uint actionID)
            {
                if (actionID is KeenEdge or BrutalShell or SolidBarrel or BurstStrike)
                {
                    int corundumThreshold = Config.GNBPvP_CorundumThreshold;
                    int blastingZoneThreshold = Config.GNBPvP_BlastingZoneThreshold; 

                    if (CanWeave() && IsEnabled(CustomComboPreset.GNBPvP_Corundum) && PlayerHealthPercentageHp() <= corundumThreshold && IsOffCooldown(HeartOfCorundum))
                        return HeartOfCorundum;

                    if (IsEnabled(CustomComboPreset.GNBPvP_Rampart) && PvPTank.CanRampart(Config.GNBPvP_RampartThreshold))
                        return PvPTank.Rampart;

                    if (!PvPCommon.TargetImmuneToDamage())
                    {
                        if (CanWeave()) //Weave section
                        {
                            //Continuation
                            if (IsEnabled(CustomComboPreset.GNBPvP_ST_Continuation) && OriginalHook(Continuation) != Continuation) // Weaving followup button, whenever it changes to something useable, it will fire
                                return OriginalHook(Continuation);

                            if (IsEnabled(CustomComboPreset.GNBPvP_BlastingZone) && IsOffCooldown(BlastingZone) && GetTargetHPPercent() < blastingZoneThreshold)  // Removed nomercy requirement bc of hp threshold.
                                return BlastingZone;
                        }

                        //RoughDivide overcap protection
                        if (IsEnabled(CustomComboPreset.GNBPvP_RoughDivide))
                        {
                            if (HasCharges(RoughDivide) && !HasStatusEffect(Buffs.NoMercy) && !JustUsed(RoughDivide, 3f) &&
                               (ActionReady(FatedCircle)|| ActionReady(GnashingFang) || GetRemainingCharges(RoughDivide) == 2)) // Will RD for for no mercy when at 2 charges, or before the fated circle or gnashing fang combo
                                return RoughDivide;
                        }

                        //Fated Circle and Followup
                        if (IsEnabled(CustomComboPreset.GNBPvP_FatedCircle))
                        {
                            if (ActionReady(FatedCircle) && HasStatusEffect(Buffs.NoMercy) && OriginalHook(Continuation) == Continuation)
                                return FatedCircle;
                        }

                        //GnashingFang
                        if (IsEnabled(CustomComboPreset.GNBPvP_ST_GnashingFang) && (ActionReady(GnashingFang) || OriginalHook(GnashingFang) != GnashingFang))
                            return OriginalHook(GnashingFang);

                    }
                }

                return actionID;
            }
        }

        internal class GNBPvP_GnashingFang : CustomCombo
        {
            protected internal override CustomComboPreset Preset { get; } = CustomComboPreset.GNBPvP_GnashingFang;

            protected override uint Invoke(uint actionID) =>
                actionID is GnashingFang &&
                    CanWeave() && (HasStatusEffect(Buffs.ReadyToRip) || HasStatusEffect(Buffs.ReadyToTear) || HasStatusEffect(Buffs.ReadyToGouge))
                    ? OriginalHook(Continuation)
                    : actionID;
        }

    }
}
