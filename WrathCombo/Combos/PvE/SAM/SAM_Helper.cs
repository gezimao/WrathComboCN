﻿using Dalamud.Game.ClientState.JobGauge.Enums;
using Dalamud.Game.ClientState.JobGauge.Types;
using FFXIVClientStructs.FFXIV.Client.Game;
using System;
using System.Collections.Generic;
using System.Linq;
using WrathCombo.CustomComboNS;
using WrathCombo.CustomComboNS.Functions;
using WrathCombo.Data;
using static WrathCombo.Combos.PvE.SAM.Config;
using static WrathCombo.CustomComboNS.Functions.CustomComboFunctions;
using static WrathCombo.Data.ActionWatching;
using ActionType = FFXIVClientStructs.FFXIV.Client.Game.ActionType;
namespace WrathCombo.Combos.PvE;

internal partial class SAM
{
    internal static SAMOpenerMaxLevel1 Opener1 = new();

    internal static bool RefreshFugetsu => GetStatusEffectRemainingTime(Buffs.Fugetsu) < GetStatusEffectRemainingTime(Buffs.Fuka);

    internal static bool RefreshFuka => GetStatusEffectRemainingTime(Buffs.Fuka) < GetStatusEffectRemainingTime(Buffs.Fugetsu);

    internal static bool EnhancedSenei => TraitLevelChecked(Traits.EnhancedHissatsu);

    internal static int SenCount => GetSenCount();

    internal static bool UseTsubame =>
        LevelChecked(TsubameGaeshi) &&
        (HasStatusEffect(Buffs.TendoKaeshiSetsugekkaReady) ||
         (HasStatusEffect(Buffs.TsubameReady) && (SenCount is 3 || GetCooldownRemainingTime(Senei) > 33)));

    internal static bool M6SReady => !HiddenFeaturesData.IsEnabledWith(CustomComboPreset.SAM_Hid_M6SHoldSquirrelBurst, () =>
        HiddenFeaturesData.Targeting.R6SSquirrel && CombatEngageDuration().TotalSeconds < 275);

    internal static bool UseMeikyo()
    {
        float gcd = ActionManager.GetAdjustedRecastTime(ActionType.Action, Hakaze) / 100f;
        int meikyoUsed = CombatActions.Count(x => x == MeikyoShisui);

        if (ActionReady(MeikyoShisui) && !HasStatusEffect(Buffs.Tendo) && !HasStatusEffect(Buffs.MeikyoShisui) &&
            (JustUsed(Gekko) || JustUsed(Kasha) || JustUsed(Yukikaze)))
        {
            if (EnhancedSenei)
            {
                //if no opener
                if ((IsEnabled(CustomComboPreset.SAM_ST_Opener) && SAM_Balance_Content == 1 && !InBossEncounter() ||
                     IsNotEnabled(CustomComboPreset.SAM_ST_Opener)) &&
                    meikyoUsed < 1 && !HasStatusEffect(Buffs.TsubameReady))
                    return true;

                if (HasStatusEffect(Buffs.TsubameReady))
                {
                    switch (gcd)
                    {
                        //2.14 GCD
                        case >= 2.09f when GetCooldownRemainingTime(Senei) <= 10 &&
                                           (meikyoUsed % 7 is 1 or 2 && SenCount is 3 ||
                                            meikyoUsed % 7 is 3 or 4 && SenCount is 2 ||
                                            meikyoUsed % 7 is 5 or 6 && SenCount is 1):
                        //2.08 gcd
                        case <= 2.08f when GetCooldownRemainingTime(Senei) <= 10 && SenCount is 3:
                            return true;
                    }
                }

                // reset meikyo
                if (gcd >= 2.09f && meikyoUsed % 7 is 0 && JustUsed(Yukikaze))
                    return true;
            }

            //Pre Enhanced Senei
            if (!EnhancedSenei && ActionReady(MeikyoShisui) && !HasStatusEffect(Buffs.TsubameReady))
                return true;
        }

        return false;
    }

    internal static bool UseIaijutsu()
    {
        int higanbanaHPThreshold = SAM_ST_Higanbana_HP_Threshold;
        int higanbanaRefresh = SAM_ST_Higanbana_Refresh;

        if (LevelChecked(Iaijutsu))
        {
            if (IsEnabled(CustomComboPreset.SAM_ST_AdvancedMode) &&

                //Higanbana
                ((SAM_ST_CDs_IaijutsuOption[0] &&
                  SenCount is 1 && GetTargetHPPercent() > higanbanaHPThreshold &&
                  (SAM_ST_Higanbana_Suboption == 0 ||
                   SAM_ST_Higanbana_Suboption == 1 && TargetIsBoss()) &&
                  CanApplyStatus(CurrentTarget, Debuffs.Higanbana) &&
                  ((JustUsed(MeikyoShisui, 15f) && GetStatusEffectRemainingTime(Debuffs.Higanbana, CurrentTarget) <= higanbanaRefresh) ||
                   !HasStatusEffect(Debuffs.Higanbana, CurrentTarget))) ||

                 //Tenka Goken
                 (SAM_ST_CDs_IaijutsuOption[1] && SenCount is 2 && !LevelChecked(MidareSetsugekka)) ||

                 //Midare Setsugekka
                 (SAM_ST_CDs_IaijutsuOption[2] && SenCount is 3 && LevelChecked(MidareSetsugekka) && !HasStatusEffect(Buffs.TsubameReady))))
                return true;

            if (IsEnabled(CustomComboPreset.SAM_ST_SimpleMode) &&

                //Higanbana
                ((SenCount is 1 && GetTargetHPPercent() > 1 && TargetIsBoss() &&
                  CanApplyStatus(CurrentTarget, Debuffs.Higanbana) &&
                  ((JustUsed(MeikyoShisui, 15f) && GetStatusEffectRemainingTime(Debuffs.Higanbana, CurrentTarget) <= 15) ||
                   !HasStatusEffect(Debuffs.Higanbana, CurrentTarget))) ||

                 //Tenka Goken
                 (SenCount is 2 && !LevelChecked(MidareSetsugekka)) ||

                 //Midare Setsugekka
                 (SenCount is 3 && LevelChecked(MidareSetsugekka) && !HasStatusEffect(Buffs.TsubameReady))))
                return true;
        }

        return false;
    }

    #region Openers

    internal static WrathOpener Opener()
    {
        if (Opener1.LevelChecked)
            return Opener1;

        return WrathOpener.Dummy;
    }

    internal class SAMOpenerMaxLevel1 : WrathOpener
    {
        public override int MinOpenerLevel => 100;

        public override int MaxOpenerLevel => 109;

        public override List<uint> OpenerActions { get; set; } =
        [
            MeikyoShisui,
            Role.TrueNorth, //2
            Gekko,
            Kasha,
            Ikishoten,
            Yukikaze,
            TendoSetsugekka,
            Senei,
            TendoKaeshiSetsugekka,
            MeikyoShisui,
            Gekko,
            Zanshin,
            Higanbana,
            OgiNamikiri,
            Shoha,
            KaeshiNamikiri,
            Kasha,
            Shinten,
            Gekko,
            Gyoten, //20
            Gyofu,
            Yukikaze,
            Shinten,
            TendoSetsugekka,
            TendoKaeshiSetsugekka
        ];

        internal override UserData ContentCheckConfig => SAM_Balance_Content;

        public override List<(int[] Steps, Func<int> HoldDelay)> PrepullDelays { get; set; } =
        [
            ([2], () => SAM_Opener_PrePullDelay)
        ];

        public override List<(int[] Steps, uint NewAction, Func<bool> Condition)> SubstitutionSteps { get; set; } =
        [
            ([2], 11, () => !TargetNeedsPositionals())
        ];

        public override bool HasCooldowns() =>
            GetRemainingCharges(MeikyoShisui) is 2 &&
            GetRemainingCharges(Role.TrueNorth) is 2 &&
            IsOffCooldown(Senei) &&
            IsOffCooldown(Ikishoten) &&
            SenCount is 0;
    }

    #endregion

    #region Gauge

    internal static SAMGauge Gauge = GetJobGauge<SAMGauge>();

    internal static bool HasGetsu => Gauge.HasGetsu;

    internal static bool HasSetsu => Gauge.HasSetsu;

    internal static bool HasKa => Gauge.HasKa;

    internal static byte Kenki => Gauge.Kenki;

    internal static byte MeditationStacks => Gauge.MeditationStacks;

    internal static Kaeshi Kaeshi => Gauge.Kaeshi;

    internal static bool NamikiriReady => Kaeshi is Kaeshi.Namikiri;

    private static int GetSenCount()
    {
        int senCount = 0;

        if (HasGetsu)
            senCount++;

        if (HasSetsu)
            senCount++;

        if (HasKa)
            senCount++;

        return senCount;
    }

    #endregion

    #region ID's

    public const byte JobID = 34;

    public const uint
        Hakaze = 7477,
        Yukikaze = 7480,
        Gekko = 7481,
        Enpi = 7486,
        Jinpu = 7478,
        Kasha = 7482,
        Shifu = 7479,
        Mangetsu = 7484,
        Fuga = 7483,
        Oka = 7485,
        Higanbana = 7489,
        TenkaGoken = 7488,
        MidareSetsugekka = 7487,
        Shinten = 7490,
        Kyuten = 7491,
        Hagakure = 7495,
        Guren = 7496,
        Senei = 16481,
        MeikyoShisui = 7499,
        Seigan = 7501,
        ThirdEye = 7498,
        Iaijutsu = 7867,
        TsubameGaeshi = 16483,
        KaeshiHiganbana = 16484,
        Shoha = 16487,
        Ikishoten = 16482,
        Fuko = 25780,
        OgiNamikiri = 25781,
        KaeshiNamikiri = 25782,
        Yaten = 7493,
        Gyoten = 7492,
        KaeshiSetsugekka = 16486,
        TendoGoken = 36965,
        TendoKaeshiSetsugekka = 36968,
        Zanshin = 36964,
        TendoSetsugekka = 36966,
        Tengentsu = 7498,
        Gyofu = 36963;

    public static class Buffs
    {
        public const ushort
            MeikyoShisui = 1233,
            EnhancedEnpi = 1236,
            EyesOpen = 1252,
            OgiNamikiriReady = 2959,
            Fuka = 1299,
            Fugetsu = 1298,
            TsubameReady = 4216,
            TendoKaeshiSetsugekkaReady = 4218,
            KaeshiGokenReady = 3852,
            TendoKaeshiGokenReady = 4217,
            ZanshinReady = 3855,
            Tengentsu = 3853,
            Tendo = 3856;
    }

    public static class Debuffs
    {
        public const ushort
            Higanbana = 1228;
    }

    public static class Traits
    {
        public const ushort
            EnhancedHissatsu = 591,
            EnhancedMeikyoShishui = 443,
            EnhancedMeikyoShishui2 = 593;
    }

    #endregion
}
