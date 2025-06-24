using Dalamud.Interface.Colors;
using ECommons.ImGuiMethods;
using ImGuiNET;
using WrathCombo.CustomComboNS.Functions;
using WrathCombo.Data;
using WrathCombo.Extensions;
using WrathCombo.Window.Functions;
using BossAvoidance = WrathCombo.Combos.PvE.All.Enums.BossAvoidance;
using PartyRequirement = WrathCombo.Combos.PvE.All.Enums.PartyRequirement;

namespace WrathCombo.Combos.PvE;

internal partial class WAR
{
    internal static class Config
    {
        public static UserInt
            WAR_Infuriate_Charges = new("WAR_Infuriate_Charges", 0),
            WAR_Infuriate_Range = new("WAR_Infuriate_Range", 0),
            WAR_SurgingRefreshRange = new("WAR_SurgingRefreshRange", 10),
            WAR_EyePath_Refresh = new("WAR_EyePath", 10),
            WAR_ST_Infuriate_Charges = new("WAR_ST_Infuriate_Charges", 0),
            WAR_ST_Infuriate_Gauge = new("WAR_ST_Infuriate_Gauge", 40),
            WAR_ST_FellCleave_Gauge = new("WAR_ST_FellCleave_Gauge", 90),
            WAR_ST_Onslaught_Charges = new("WAR_ST_Onslaught_Charges", 0),
            WAR_ST_Onslaught_Movement = new("WAR_ST_Onslaught_Movement", 0),
            WAR_ST_PrimalRend_Movement = new("WAR_ST_PrimalRend_Movement", 0),
            WAR_ST_PrimalRend_EarlyLate = new("WAR_ST_PrimalRend_EarlyLate", 0),
            WAR_ST_Bloodwhetting_Health = new("WAR_ST_BloodwhettingOption", 90),
            WAR_ST_Bloodwhetting_SubOption = new("WAR_ST_Bloodwhetting_SubOpt", 0),
            WAR_ST_Equilibrium_Health = new("WAR_ST_EquilibriumOption", 50),
            WAR_ST_Equilibrium_SubOption = new("WAR_ST_Equilibrium_SubOpt", 0),
            WAR_ST_Rampart_Health = new("WAR_ST_Rampart_Health", 80),
            WAR_ST_Rampart_SubOption = new("WAR_ST_Rampart_SubOption", 0),
            WAR_ST_Thrill_Health = new("WAR_ST_Thrill_Health", 70),
            WAR_ST_Thrill_SubOption = new("WAR_ST_Thrill_SubOpt", 0),
            WAR_ST_Vengeance_Health = new("WAR_ST_Vengeance_Health", 60),
            WAR_ST_Vengeance_SubOption = new("WAR_ST_Vengeance_SubOpt", 0),
            WAR_ST_Holmgang_Health = new("WAR_ST_Holmgang_Health", 30),
            WAR_ST_Holmgang_SubOption = new("WAR_ST_Holmgang_SubOpt", 0),
            WAR_ST_Reprisal_Health = new("WAR_ST_Reprisal_Health", 80),
            WAR_ST_Reprisal_SubOption = new("WAR_ST_Reprisal_SubOpt", 0),
            WAR_ST_ArmsLength_Health = new("WAR_ST_ArmsLength_Health", 80),
            WAR_ST_MitsOptions = new("WAR_ST_MitsOptions", 0),
            WAR_ST_IRStop = new("WAR_ST_IRStop", 0),
            WAR_AoE_Infuriate_Charges = new("WAR_AoE_Infuriate_Charges", 0),
            WAR_AoE_Infuriate_Gauge = new("WAR_AoE_Infuriate_Gauge", 40),
            WAR_AoE_Decimate_Gauge = new("WAR_AoE_Decimate_Gauge", 90),
            WAR_AoE_Onslaught_Charges = new("WAR_AoE_Onslaught_Charges", 0),
            WAR_AoE_Onslaught_Movement = new("WAR_AoE_Onslaught_Movement", 0),
            WAR_AoE_PrimalRend_Movement = new("WAR_AoE_PrimalRend_Movement", 0),
            WAR_AoE_PrimalRend_EarlyLate = new("WAR_AoE_PrimalRend_EarlyLate", 0),
            WAR_AoE_OrogenyUpheaval = new("WAR_AoE_OrogenyUpheaval", 0),
            WAR_AoE_Bloodwhetting_Health = new("WAR_AoE_BloodwhettingOption", 90),
            WAR_AoE_Bloodwhetting_SubOption = new("WAR_AoE_Bloodwhetting_SubOpt", 0),
            WAR_AoE_Equilibrium_Health = new("WAR_AoE_EquilibriumOption", 50),
            WAR_AoE_Equilibrium_SubOption = new("WAR_AoE_Equilibrium_SubOpt", 0),
            WAR_AoE_Rampart_Health = new("WAR_AoE_Rampart_Health", 80),
            WAR_AoE_Rampart_SubOption = new("WAR_AoE_Rampart_SubOpt", 0),
            WAR_AoE_Thrill_Health = new("WAR_AoE_Thrill_Health", 80),
            WAR_AoE_Thrill_SubOption = new("WAR_AoE_Thrill_SubOpt", 0),
            WAR_AoE_Vengeance_Health = new("WAR_AoE_Vengeance_Health", 60),
            WAR_AoE_Vengeance_SubOption = new("WAR_AoE_Vengeance_SubOpt", 0),
            WAR_AoE_Holmgang_Health = new("WAR_AoE_Holmgang_Health", 30),
            WAR_AoE_Holmgang_SubOption = new("WAR_AoE_Holmgang_SubOpt", 0),
            WAR_AoE_Reprisal_Health = new("WAR_AoE_Reprisal_Health", 80),
            WAR_AoE_Reprisal_SubOption = new("WAR_AoE_Reprisal_SubOpt", 0),
            WAR_AoE_ArmsLength_Health = new("WAR_AoE_ArmsLength_Health", 80),
            WAR_AoE_MitsOptions = new("WAR_AoE_MitsOptions", 0),
            WAR_AoE_IRStop = new("WAR_AoE_IRStop", 0),
            WAR_VariantCure = new("WAR_VariantCure"),
            WAR_BalanceOpener_Content = new("WAR_BalanceOpener_Content", 1),
            WAR_FC_IRStop = new("WAR_FC_IRStop", 0),
            WAR_FC_Infuriate_Charges = new("WAR_FC_Infuriate_Charges", 0),
            WAR_FC_Infuriate_Gauge = new("WAR_FC_Infuriate_Gauge", 40),
            WAR_FC_Onslaught_Charges = new("WAR_FC_Onslaught_Charges", 0),
            WAR_FC_Onslaught_Movement = new("WAR_FC_Onslaught_Movement", 0),
            WAR_FC_PrimalRend_Movement = new("WAR_FC_PrimalRend_Movement", 0),
            WAR_FC_PrimalRend_EarlyLate = new("WAR_FC_PrimalRend_EarlyLate", 0),
            WAR_Mit_Holmgang_Health = new("WAR_Mit_Holmgang_Health", 30),
            WAR_Mit_Bloodwhetting_Health = new("WAR_Mit_Bloodwhetting_Health", 70),
            WAR_Mit_Equilibrium_Health = new("WAR_Mit_Equilibrium_Health", 45),
            WAR_Mit_ThrillOfBattle_Health = new("WAR_Mit_ThrillOfBattle_Health", 60),
            WAR_Mit_Rampart_Health = new("WAR_Mit_Rampart_Health", 65),
            WAR_Mit_ShakeItOff_PartyRequirement = new("WAR_Mit_ShakeItOff_PartyRequirement", (int)PartyRequirement.Yes),
            WAR_Mit_ArmsLength_Boss = new("WAR_Mit_ArmsLength_Boss", (int)BossAvoidance.On),
            WAR_Mit_ArmsLength_EnemyCount = new("WAR_Mit_ArmsLength_EnemyCount", 0),
            WAR_Mit_Vengeance_Health = new("WAR_Mit_Vengeance_Health", 50),
            WAR_Bozja_LostCure_Health = new("WAR_Bozja_LostCure_Health", 50),
            WAR_Bozja_LostCure2_Health = new("WAR_Bozja_LostCure2_Health", 50),
            WAR_Bozja_LostCure3_Health = new("WAR_Bozja_LostCure3_Health", 50),
            WAR_Bozja_LostCure4_Health = new("WAR_Bozja_LostCure4_Health", 50),
            WAR_Bozja_LostAethershield_Health = new("WAR_Bozja_LostAethershield_Health", 70),
            WAR_Bozja_LostReraise_Health = new("WAR_Bozja_LostReraise_Health", 10);

        public static UserFloat
            WAR_ST_Onslaught_Distance = new("WAR_ST_Ons_Distance", 3.0f),
            WAR_ST_PrimalRend_Distance = new("WAR_ST_PR_Distance", 3.0f),
            WAR_AoE_Onslaught_Distance = new("WAR_AoE_Ons_Distance", 3.0f),
            WAR_AoE_PrimalRend_Distance = new("WAR_AoE_PR_Distance", 3.0f),
            WAR_FC_Onslaught_Distance = new("WAR_FC_Ons_Distance", 3.0f),
            WAR_FC_PrimalRend_Distance = new("WAR_FC_PR_Distance", 3.0f),
            WAR_ST_Onslaught_TimeStill = new("WAR_ST_Onslaught_TimeStill", 0),
            WAR_ST_PrimalRend_TimeStill = new("WAR_ST_PrimalRend_TimeStill", 0),
            WAR_AoE_Onslaught_TimeStill = new("WAR_AoE_Onslaught_TimeStill", 0),
            WAR_AoE_PrimalRend_TimeStill = new("WAR_AoE_PrimalRend_TimeStill", 0),
            WAR_FC_Onslaught_TimeStill = new("WAR_FC_Onslaught_TimeStill", 0),
            WAR_FC_PrimalRend_TimeStill = new("WAR_FC_PrimalRend_TimeStill", 0);

        public static UserIntArray
            WAR_Mit_Priorities = new("WAR_Mit_Priorities");

        public static UserBoolArray
            WAR_Mit_Holmgang_Difficulty = new("WAR_Mit_Holmgang_Difficulty", [true, false]);

        public static readonly ContentCheck.ListSet WAR_Mit_Holmgang_DifficultyListSet = ContentCheck.ListSet.Halved;

        private const int NumMitigationOptions = 8;

        internal static void Draw(CustomComboPreset preset)
        {
            switch (preset)
            {
                #region Single-Target
                case CustomComboPreset.WAR_ST_BalanceOpener:
                    UserConfig.DrawBossOnlyChoice(WAR_BalanceOpener_Content);
                    break;

                case CustomComboPreset.WAR_ST_StormsEye:
                    UserConfig.DrawSliderInt(0, 30, WAR_SurgingRefreshRange,
                        $" Seconds remaining before refreshing {Buffs.SurgingTempest.StatusName()} buff:");
                    break;

                case CustomComboPreset.WAR_ST_InnerRelease:
                    UserConfig.DrawSliderInt(0, 75, WAR_ST_IRStop,
                        " Stop usage if Target HP% is below set value.\n To disable this, set value to 0");
                    break;

                case CustomComboPreset.WAR_ST_Onslaught:
                    UserConfig.DrawHorizontalRadioButton(WAR_ST_Onslaught_Movement,
                            "Stationary Only", "Uses Onslaught only while stationary", 0);
                    UserConfig.DrawHorizontalRadioButton(WAR_ST_Onslaught_Movement,
                            "Any Movement", "Uses Onslaught regardless of any movement conditions.\nNOTE: This could possibly get you killed", 1);
                    ImGui.Spacing();
                    if (WAR_ST_Onslaught_Movement == 0)
                    {
                        ImGui.SetCursorPosX(48);
                        UserConfig.DrawSliderFloat(0, 3, WAR_ST_Onslaught_TimeStill,
                            " Stationary Delay Check (in seconds):", decimals: 1);
                    }
                    ImGui.SetCursorPosX(48);
                    UserConfig.DrawSliderInt(0, 2, WAR_ST_Onslaught_Charges,
                        " How many charges to keep ready?\n (0 = Use All)");
                    ImGui.SetCursorPosX(48);
                    UserConfig.DrawSliderFloat(1, 20, WAR_ST_Onslaught_Distance,
                        " Use when Distance from target is less than or equal to:", decimals: 1);
                    break;

                case CustomComboPreset.WAR_ST_Infuriate:
                    UserConfig.DrawSliderInt(0, 2, WAR_ST_Infuriate_Charges,
                        " How many charges to keep ready?\n (0 = Use All)");
                    UserConfig.DrawSliderInt(0, 50, WAR_ST_Infuriate_Gauge,
                        " Use when Beast Gauge is less than or equal to:");
                    break;

                case CustomComboPreset.WAR_ST_FellCleave:
                    UserConfig.DrawSliderInt(50, 100, WAR_ST_FellCleave_Gauge,
                        " Minimum Beast Gauge required to spend:");
                    break;

                case CustomComboPreset.WAR_ST_PrimalRend:
                    UserConfig.DrawHorizontalRadioButton(WAR_ST_PrimalRend_EarlyLate,
                        "Early", "Uses Primal Rend ASAP", 0);
                    UserConfig.DrawHorizontalRadioButton(WAR_ST_PrimalRend_EarlyLate,
                        "Late", "Uses Primal Rend after consumption of all Inner Release stacks", 1);
                    ImGui.NewLine();
                    UserConfig.DrawHorizontalRadioButton(WAR_ST_PrimalRend_Movement,
                        "Stationary Only", "Uses Primal Rend only while stationary", 0);
                    UserConfig.DrawHorizontalRadioButton(WAR_ST_PrimalRend_Movement,
                        "Any Movement", "Uses Primal Rend regardless of any movement conditions.\nNOTE: This could possibly get you killed", 1);
                    ImGui.Spacing();
                    if (WAR_ST_PrimalRend_Movement == 0)
                    {
                        ImGui.SetCursorPosX(48);
                        UserConfig.DrawSliderFloat(0, 3, WAR_ST_PrimalRend_TimeStill,
                            " Stationary Delay Check (in seconds):", decimals: 1);
                    }
                    ImGui.SetCursorPosX(48);
                    UserConfig.DrawSliderFloat(1, 20, WAR_ST_PrimalRend_Distance,
                        " Use when Distance from target is less than or equal to:", decimals: 1);
                    break;
                #endregion

                #region AoE
                case CustomComboPreset.WAR_AoE_Decimate:
                    UserConfig.DrawSliderInt(50, 100, WAR_AoE_Decimate_Gauge,
                        "Minimum gauge required to spend:");
                    break;

                case CustomComboPreset.WAR_AoE_InnerRelease:
                    UserConfig.DrawSliderInt(0, 75, WAR_AoE_IRStop,
                        " Stop usage if Target HP% is below set value.\n To disable this, set value to 0");
                    break;


                case CustomComboPreset.WAR_AoE_Infuriate:
                    UserConfig.DrawSliderInt(0, 2, WAR_AoE_Infuriate_Charges,
                        " How many charges to keep ready?\n (0 = Use All)");
                    UserConfig.DrawSliderInt(0, 50, WAR_AoE_Infuriate_Gauge,
                        "Use when gauge is under or equal to");
                    break;

                case CustomComboPreset.WAR_AoE_Onslaught:
                    UserConfig.DrawHorizontalRadioButton(WAR_AoE_Onslaught_Movement,
                            "Stationary Only", "Uses Onslaught only while stationary", 0);
                    UserConfig.DrawHorizontalRadioButton(WAR_AoE_Onslaught_Movement,
                            "Any Movement", "Uses Onslaught regardless of any movement conditions.\nNOTE: This could possibly get you killed", 1);
                    ImGui.Spacing();
                    if (WAR_AoE_Onslaught_Movement == 0) 
                    {
                        ImGui.SetCursorPosX(48);
                        UserConfig.DrawSliderFloat(0, 3, WAR_AoE_Onslaught_TimeStill,
                            " Stationary Delay Check (in seconds):", decimals: 1);
                    }
                    UserConfig.DrawSliderInt(0, 2, WAR_AoE_Onslaught_Charges,
                        " How many charges to keep ready?\n (0 = Use All)");
                    ImGui.SetCursorPosX(48);
                    UserConfig.DrawSliderFloat(1, 20, WAR_AoE_Onslaught_Distance,
                        " Use when Distance from target is less than or equal to:", decimals: 1);
                    break;

                case CustomComboPreset.WAR_AoE_PrimalRend:
                    UserConfig.DrawHorizontalRadioButton(WAR_AoE_PrimalRend_EarlyLate,
                        "Early", "Uses Primal Rend ASAP", 0);
                    UserConfig.DrawHorizontalRadioButton(WAR_AoE_PrimalRend_EarlyLate,
                        "Late", "Uses Primal Rend after consumption of all Inner Release stacks", 1);
                    ImGui.NewLine();
                    UserConfig.DrawHorizontalRadioButton(WAR_AoE_PrimalRend_Movement,
                        "Stationary Only", "Uses Primal Rend only while stationary", 0);
                    UserConfig.DrawHorizontalRadioButton(WAR_AoE_PrimalRend_Movement,
                        "Any Movement", "Uses Primal Rend regardless of any movement conditions.\nNOTE: This could possibly get you killed", 1);
                    ImGui.Spacing();
                    if (WAR_AoE_PrimalRend_Movement == 0)
                    {
                        ImGui.SetCursorPosX(48);
                        UserConfig.DrawSliderFloat(0, 3, WAR_AoE_PrimalRend_TimeStill,
                            " Stationary Delay Check (in seconds):", decimals: 1);
                    }
                    ImGui.SetCursorPosX(48);
                    UserConfig.DrawSliderFloat(1, 20, WAR_AoE_PrimalRend_Distance,
                        " Use when Distance from target is less than or equal to:", decimals: 1);
                    break;

                case CustomComboPreset.WAR_AoE_Orogeny:
                    UserConfig.DrawHorizontalRadioButton(WAR_AoE_OrogenyUpheaval,
                        "Include Upheaval", "Enables the use of Upheaval in AoE rotation if Orogeny is unavailable", 0);
                    UserConfig.DrawHorizontalRadioButton(WAR_AoE_OrogenyUpheaval,
                        "Exclude Upheaval", "Disables the use of Upheaval in AoE rotation", 1);
                    break;
                #endregion

                #region Mitigations
                case CustomComboPreset.WAR_ST_Bloodwhetting:
                    UserConfig.DrawSliderInt(1, 100, WAR_ST_Bloodwhetting_Health,
                        "Player HP% to be \nless than or equal to:", 200);
                    UserConfig.DrawHorizontalRadioButton(WAR_ST_Bloodwhetting_SubOption,
                        "All Enemies", $"Uses {Bloodwhetting.ActionName()} regardless of targeted enemy type", 0);
                    UserConfig.DrawHorizontalRadioButton(WAR_ST_Bloodwhetting_SubOption,
                        "Bosses Only", $"Only uses {Bloodwhetting.ActionName()} when the targeted enemy is a boss", 1);
                    break;

                case CustomComboPreset.WAR_AoE_Bloodwhetting:
                    UserConfig.DrawSliderInt(1, 100, WAR_AoE_Bloodwhetting_Health,
                        "Player HP% to be \nless than or equal to:", 200);
                    UserConfig.DrawHorizontalRadioButton(WAR_AoE_Bloodwhetting_SubOption,
                        "All Enemies", $"Uses {Bloodwhetting.ActionName()} regardless of targeted enemy type", 0);
                    UserConfig.DrawHorizontalRadioButton(WAR_AoE_Bloodwhetting_SubOption,
                        "Bosses Only", $"Only uses {Bloodwhetting.ActionName()} when the targeted enemy is a boss", 1);
                    break;

                case CustomComboPreset.WAR_ST_Equilibrium:
                    UserConfig.DrawSliderInt(1, 100, WAR_ST_Equilibrium_Health,
                        "Player HP% to be \nless than or equal to:", 200);
                    UserConfig.DrawHorizontalRadioButton(WAR_ST_Equilibrium_SubOption,
                        "All Enemies", $"Uses {Equilibrium.ActionName()} regardless of targeted enemy type", 0);
                    UserConfig.DrawHorizontalRadioButton(WAR_ST_Equilibrium_SubOption,
                        "Bosses Only", $"Only uses {Equilibrium.ActionName()} when the targeted enemy is a boss", 1);
                    break;

                case CustomComboPreset.WAR_AoE_Equilibrium:
                    UserConfig.DrawSliderInt(1, 100, WAR_AoE_Equilibrium_Health,
                        "Player HP% to be \nless than or equal to:", 200);
                    UserConfig.DrawHorizontalRadioButton(WAR_AoE_Equilibrium_SubOption,
                        "All Enemies", $"Uses {Equilibrium.ActionName()} regardless of targeted enemy type", 0);
                    UserConfig.DrawHorizontalRadioButton(WAR_AoE_Equilibrium_SubOption,
                        "Bosses Only", $"Only uses {Equilibrium.ActionName()} when the targeted enemy is a boss", 1);
                    break;

                case CustomComboPreset.WAR_ST_Rampart:
                    UserConfig.DrawSliderInt(1, 100, WAR_ST_Rampart_Health,
                        "Player HP% to be \nless than or equal to:", 200);
                    UserConfig.DrawHorizontalRadioButton(WAR_ST_Rampart_SubOption,
                        "All Enemies", $"Uses {Role.Rampart.ActionName()} regardless of targeted enemy type", 0);
                    UserConfig.DrawHorizontalRadioButton(WAR_ST_Rampart_SubOption,
                        "Bosses Only", $"Only uses {Role.Rampart.ActionName()} when the targeted enemy is a boss", 1);
                    break;

                case CustomComboPreset.WAR_AoE_Rampart:
                    UserConfig.DrawSliderInt(1, 100, WAR_AoE_Rampart_Health,
                        "Player HP% to be \nless than or equal to:", 200);
                    UserConfig.DrawHorizontalRadioButton(WAR_AoE_Rampart_SubOption,
                        "All Enemies", $"Uses {Role.Rampart.ActionName()} regardless of targeted enemy type", 0);
                    UserConfig.DrawHorizontalRadioButton(WAR_AoE_Rampart_SubOption,
                        "Bosses Only", $"Only uses {Role.Rampart.ActionName()} when the targeted enemy is a boss", 1);
                    break;

                case CustomComboPreset.WAR_ST_Thrill:
                    UserConfig.DrawSliderInt(1, 100, WAR_ST_Thrill_Health,
                        "Player HP% to be \nless than or equal to:", 200);
                    UserConfig.DrawHorizontalRadioButton(WAR_ST_Thrill_SubOption,
                        "All Enemies", $"Uses {ThrillOfBattle.ActionName()} regardless of targeted enemy type", 0);
                    UserConfig.DrawHorizontalRadioButton(WAR_ST_Thrill_SubOption,
                        "Bosses Only", $"Only uses {ThrillOfBattle.ActionName()} when the targeted enemy is a boss", 1);
                    break;

                case CustomComboPreset.WAR_AoE_Thrill:
                    UserConfig.DrawSliderInt(1, 100, WAR_AoE_Thrill_Health,
                        "Player HP% to be \nless than or equal to:", 200);
                    UserConfig.DrawHorizontalRadioButton(WAR_AoE_Thrill_SubOption,
                        "All Enemies", $"Uses {ThrillOfBattle.ActionName()} regardless of targeted enemy type", 0);
                    UserConfig.DrawHorizontalRadioButton(WAR_AoE_Thrill_SubOption,
                        "Bosses Only", $"Only uses {ThrillOfBattle.ActionName()} when the targeted enemy is a boss", 1);
                    break;

                case CustomComboPreset.WAR_ST_Vengeance:
                    UserConfig.DrawSliderInt(1, 100, WAR_ST_Vengeance_Health,
                        "Player HP% to be \nless than or equal to:", 200);
                    UserConfig.DrawHorizontalRadioButton(WAR_ST_Vengeance_SubOption,
                        "All Enemies", $"Uses {Vengeance.ActionName()} regardless of targeted enemy type", 0);
                    UserConfig.DrawHorizontalRadioButton(WAR_ST_Vengeance_SubOption,
                        "Bosses Only", $"Only uses {Vengeance.ActionName()} when the targeted enemy is a boss", 1);
                    break;

                case CustomComboPreset.WAR_AoE_Vengeance:
                    UserConfig.DrawSliderInt(1, 100, WAR_AoE_Vengeance_Health,
                        "Player HP% to be \nless than or equal to:", 200);
                    UserConfig.DrawHorizontalRadioButton(WAR_AoE_Vengeance_SubOption,
                        "All Enemies", $"Uses {Vengeance.ActionName()} regardless of targeted enemy type", 0);
                    UserConfig.DrawHorizontalRadioButton(WAR_AoE_Vengeance_SubOption,
                        "Bosses Only", $"Only uses {Vengeance.ActionName()} when the targeted enemy is a boss", 1);
                    break;

                case CustomComboPreset.WAR_ST_Holmgang:
                    UserConfig.DrawSliderInt(1, 100, WAR_ST_Holmgang_Health,
                        "Player HP% to be \nless than or equal to:", 200);
                    UserConfig.DrawHorizontalRadioButton(WAR_ST_Holmgang_SubOption,
                        "All Enemies", $"Uses {Holmgang.ActionName()} regardless of targeted enemy type", 0);
                    UserConfig.DrawHorizontalRadioButton(WAR_ST_Holmgang_SubOption,
                        "Bosses Only", $"Only uses {Holmgang.ActionName()} when the targeted enemy is a boss", 1);
                    break;

                case CustomComboPreset.WAR_AoE_Holmgang:
                    UserConfig.DrawSliderInt(1, 100, WAR_AoE_Holmgang_Health,
                        "Player HP% to be \nless than or equal to:", 200);
                    UserConfig.DrawHorizontalRadioButton(WAR_AoE_Holmgang_SubOption,
                        "All Enemies", $"Uses {Holmgang.ActionName()} regardless of targeted enemy type", 0);
                    UserConfig.DrawHorizontalRadioButton(WAR_AoE_Holmgang_SubOption,
                        "Bosses Only", $"Only uses {Holmgang.ActionName()} when the targeted enemy is a boss", 1);
                    break;

                case CustomComboPreset.WAR_ST_Reprisal:
                    UserConfig.DrawSliderInt(1, 100, WAR_ST_Reprisal_Health,
                        "Player HP% to be \nless than or equal to:", 200);
                    UserConfig.DrawHorizontalRadioButton(WAR_ST_Reprisal_SubOption,
                        "All Enemies", $"Uses {Role.Reprisal.ActionName()} regardless of targeted enemy type", 0);
                    UserConfig.DrawHorizontalRadioButton(WAR_ST_Reprisal_SubOption,
                        "Bosses Only", $"Only uses {Role.Reprisal.ActionName()} when the targeted enemy is a boss", 1);
                    break;

                case CustomComboPreset.WAR_AoE_Reprisal:
                    UserConfig.DrawSliderInt(1, 100, WAR_AoE_Reprisal_Health,
                        "Player HP% to be \nless than or equal to:", 200);
                    UserConfig.DrawHorizontalRadioButton(WAR_AoE_Reprisal_SubOption,
                        "All Enemies", $"Uses {Role.Reprisal.ActionName()} regardless of targeted enemy type", 0);
                    UserConfig.DrawHorizontalRadioButton(WAR_AoE_Reprisal_SubOption,
                        "Bosses Only", $"Only uses {Role.Reprisal.ActionName()} when the targeted enemy is a boss", 1);
                    break;

                #region One-Button Mitigation

                case CustomComboPreset.WAR_Mit_Holmgang_Max:
                    UserConfig.DrawDifficultyMultiChoice(WAR_Mit_Holmgang_Difficulty, WAR_Mit_Holmgang_DifficultyListSet,
                        "Select what difficulties Holmgang should be used in:");

                    UserConfig.DrawSliderInt(1, 100, WAR_Mit_Holmgang_Health,
                        "Player HP% to be \nless than or equal to:", 200, SliderIncrements.Fives);
                    break;

                case CustomComboPreset.WAR_Mit_Bloodwhetting:
                    UserConfig.DrawSliderInt(1, 100, WAR_Mit_Bloodwhetting_Health,
                        "HP% to use at or below", sliderIncrement: SliderIncrements.Ones);

                    UserConfig.DrawPriorityInput(WAR_Mit_Priorities, NumMitigationOptions, 0,
                        "Bloodwhetting Priority:");
                    break;

                case CustomComboPreset.WAR_Mit_Equilibrium:
                    UserConfig.DrawSliderInt(1, 100, WAR_Mit_Equilibrium_Health,
                        "HP% to use at or below", sliderIncrement: SliderIncrements.Ones);

                    UserConfig.DrawPriorityInput(WAR_Mit_Priorities, NumMitigationOptions, 1,
                        "Equilibrium Priority:");
                    break;

                case CustomComboPreset.WAR_Mit_Reprisal:
                    UserConfig.DrawPriorityInput(WAR_Mit_Priorities, NumMitigationOptions, 2,
                        "Reprisal Priority:");
                    break;

                case CustomComboPreset.WAR_Mit_ThrillOfBattle:
                    UserConfig.DrawSliderInt(1, 100, WAR_Mit_ThrillOfBattle_Health,
                        "HP% to use at or below (100 = Disable check)", sliderIncrement: SliderIncrements.Ones);

                    UserConfig.DrawPriorityInput(WAR_Mit_Priorities, NumMitigationOptions, 3,
                        "Thrill Of Battle Priority:");
                    break;

                case CustomComboPreset.WAR_Mit_Rampart:
                    UserConfig.DrawSliderInt(1, 100, WAR_Mit_Rampart_Health,
                        "HP% to use at or below (100 = Disable check)", sliderIncrement: SliderIncrements.Ones);

                    UserConfig.DrawPriorityInput(WAR_Mit_Priorities, NumMitigationOptions, 4,
                        "Rampart Priority:");
                    break;

                case CustomComboPreset.WAR_Mit_ShakeItOff:
                    ImGui.Indent();
                    UserConfig.DrawHorizontalRadioButton(WAR_Mit_ShakeItOff_PartyRequirement,
                        "Require party", "Will not use Shake It Off unless there are 2 or more party members.",
                        outputValue: (int)PartyRequirement.Yes);
                    UserConfig.DrawHorizontalRadioButton(WAR_Mit_ShakeItOff_PartyRequirement,
                        "Use Always", "Will not require a party for Shake It Off.",
                        outputValue: (int)PartyRequirement.No);
                    ImGui.Unindent();

                    ImGui.NewLine();
                    UserConfig.DrawPriorityInput(WAR_Mit_Priorities, NumMitigationOptions, 5,
                        "Shake It Off Priority:");
                    break;

                case CustomComboPreset.WAR_Mit_ArmsLength:
                    ImGui.Indent();
                    UserConfig.DrawHorizontalRadioButton(WAR_Mit_ArmsLength_Boss,
                        "All Enemies", "Will use Arm's Length regardless of the type of enemy.",
                        outputValue: (int)BossAvoidance.Off, itemWidth: 125f);
                    UserConfig.DrawHorizontalRadioButton(WAR_Mit_ArmsLength_Boss,
                        "Avoid Bosses", "Will try not to use Arm's Length when in a boss fight.",
                        outputValue: (int)BossAvoidance.On, itemWidth: 125f);
                    ImGui.Unindent();
                    ImGui.NewLine();
                    UserConfig.DrawSliderInt(0, 3, WAR_Mit_ArmsLength_EnemyCount,
                        "How many enemies should be nearby? (0 = No Requirement)");
                    UserConfig.DrawPriorityInput(WAR_Mit_Priorities, NumMitigationOptions, 6, "Arm's Length Priority:");
                    break;

                case CustomComboPreset.WAR_Mit_Vengeance:
                    UserConfig.DrawSliderInt(1, 100, WAR_Mit_Vengeance_Health,
                        "HP% to use at or below (100 = Disable check)",
                        sliderIncrement: SliderIncrements.Ones);
                    UserConfig.DrawPriorityInput(WAR_Mit_Priorities, NumMitigationOptions, 7, "Vengeance Priority:");
                    break;
                #endregion

                #endregion

                #region Other
                case CustomComboPreset.WAR_FC_InnerRelease:
                    UserConfig.DrawSliderInt(0, 75, WAR_FC_IRStop,
                        " Stop usage if Target HP% is below set value.\n To disable this, set value to 0");
                    break;

                case CustomComboPreset.WAR_FC_Onslaught:
                    UserConfig.DrawHorizontalRadioButton(WAR_FC_Onslaught_Movement,
                        "Stationary Only", "Uses Onslaught only while stationary", 0);
                    UserConfig.DrawHorizontalRadioButton(WAR_FC_Onslaught_Movement,
                        "Any Movement", "Uses Onslaught regardless of any movement conditions.\nNOTE: This could possibly get you killed", 1);
                    ImGui.Spacing();
                    if (WAR_FC_Onslaught_Movement == 0)
                    {
                        ImGui.SetCursorPosX(48);
                        UserConfig.DrawSliderFloat(0, 3, WAR_FC_Onslaught_TimeStill,
                            " Stationary Delay Check (in seconds):", decimals: 1);
                    }
                    UserConfig.DrawSliderInt(0, 2, WAR_FC_Onslaught_Charges,
                        " How many charges to keep ready?\n (0 = Use All)");
                    ImGui.SetCursorPosX(48);
                    UserConfig.DrawSliderFloat(1, 20, WAR_FC_Onslaught_Distance,
                        " Use when Distance from target is less than or equal to:", decimals: 1);
                    break;

                case CustomComboPreset.WAR_FC_Infuriate:
                    UserConfig.DrawSliderInt(0, 2, WAR_FC_Infuriate_Charges,
                        " How many charges to keep ready?\n (0 = Use All)");
                    UserConfig.DrawSliderInt(0, 50, WAR_FC_Infuriate_Gauge,
                        " Use when Beast Gauge is less than or equal to:");
                    break;

                case CustomComboPreset.WAR_FC_PrimalRend:
                    UserConfig.DrawHorizontalRadioButton(WAR_FC_PrimalRend_EarlyLate,
                        "Early", "Uses Primal Rend ASAP", 0);
                    UserConfig.DrawHorizontalRadioButton(WAR_FC_PrimalRend_EarlyLate,
                        "Late", "Uses Primal Rend after consumption of all Inner Release stacks", 1);
                    ImGui.NewLine();
                    UserConfig.DrawHorizontalRadioButton(WAR_FC_PrimalRend_Movement,
                        "Stationary Only", "Uses Primal Rend only while stationary", 0);
                    UserConfig.DrawHorizontalRadioButton(WAR_FC_PrimalRend_Movement,
                        "Any Movement", "Uses Primal Rend regardless of any movement conditions.\nNOTE: This could possibly get you killed", 1);
                    ImGui.Spacing();
                    if (WAR_FC_PrimalRend_Movement == 0)
                    {
                        ImGui.SetCursorPosX(48);
                        UserConfig.DrawSliderFloat(0, 3, WAR_FC_PrimalRend_TimeStill,
                            " Stationary Delay Check (in seconds):", decimals: 1);
                    }
                    ImGui.SetCursorPosX(48);
                    UserConfig.DrawSliderFloat(1, 20, WAR_FC_PrimalRend_Distance,
                        " Use when Distance from target is less than or equal to:", decimals: 1);
                    break;

                case CustomComboPreset.WAR_ST_Simple:
                    UserConfig.DrawHorizontalRadioButton(WAR_ST_MitsOptions,
                        "Include Mitigations", "Enables the use of mitigations in Simple Mode.", 0);
                    UserConfig.DrawHorizontalRadioButton(WAR_ST_MitsOptions,
                        "Exclude Mitigations", "Disables the use of mitigations in Simple Mode.", 1);
                    break;

                case CustomComboPreset.WAR_AoE_Simple:
                    UserConfig.DrawHorizontalRadioButton(WAR_AoE_MitsOptions,
                        "Include Mitigations", "Enables the use of mitigations in Simple Mode.", 0);
                    UserConfig.DrawHorizontalRadioButton(WAR_AoE_MitsOptions,
                        "Exclude Mitigations", "Disables the use of mitigations in Simple Mode.", 1);
                    break;

                case CustomComboPreset.WAR_InfuriateFellCleave:
                    UserConfig.DrawSliderInt(0, 2, WAR_Infuriate_Charges,
                        " How many charges to keep ready?\n (0 = Use All)");
                    UserConfig.DrawSliderInt(0, 50, WAR_Infuriate_Range,
                        " Use when Beast Gauge is\n less than or equal to:");
                    break;

                case CustomComboPreset.WAR_EyePath:
                    UserConfig.DrawSliderInt(0, 30, WAR_EyePath_Refresh,
                        $" Seconds remaining before refreshing {Buffs.SurgingTempest.StatusName()} buff:");
                    break;

                case CustomComboPreset.WAR_Variant_Cure:
                    UserConfig.DrawSliderInt(1, 100, WAR_VariantCure,
                        " Player HP% to be less than or equal to:", 200);
                    break;

                case CustomComboPreset.WAR_RawIntuition_Targeting_TT:
                    ImGui.Indent();
                    ImGuiEx.TextWrapped(ImGuiColors.DalamudGrey,
                        "Note: If you are Off-Tanking, and want to use Bloodwhetting on yourself, the expectation would be that you do so via the One-Button Mitigation Feature or the Mitigation options in your rotation.\n" +
                        "You could also mouseover yourself in the party to use Bloodwhetting or raw Intuition in this case.\n" +
                        "If you don't, Nascent Flash would replace the combo, and it would go to the main tank.\n" +
                        "If you don't use those Features for your personal mitigation, you may not want to enable this.");
                    ImGui.Unindent();
                    break;
                    #endregion
            }
        }
    }
}
