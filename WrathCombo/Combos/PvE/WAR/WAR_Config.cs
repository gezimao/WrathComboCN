using WrathCombo.Combos.PvP;
using WrathCombo.CustomComboNS.Functions;
using WrathCombo.Window.Functions;

namespace WrathCombo.Combos.PvE;

internal partial class WAR
{
    internal static class Config
    {
        public static UserInt
            WAR_InfuriateRange = new("WarInfuriateRange"),
            WAR_SurgingRefreshRange = new("WarSurgingRefreshRange"),
            WAR_KeepOnslaughtCharges = new("WarKeepOnslaughtCharges"),
            WAR_KeepInfuriateCharges = new("WarKeepInfuriateCharges"),
            WAR_FellCleaveGauge = new("WAR_FellCleaveGauge"),
            WAR_DecimateGauge = new("WAR_DecimateGauge"),
            WAR_InfuriateSTGauge = new("WAR_InfuriateSTGauge"),
            WAR_InfuriateAoEGauge = new("WAR_InfuriateAoEGauge"),
            WAR_ST_Bloodwhetting_Health = new("WAR_ST_BloodwhettingOption", 90),
            WAR_ST_Bloodwhetting_SubOption = new("WAR_ST_Bloodwhetting_SubOption", 1),
            WAR_ST_Equilibrium_Health = new("WAR_ST_EquilibriumOption", 50),
            WAR_ST_Equilibrium_SubOption = new("WAR_ST_Equilibrium_SubOption", 1),
            WAR_ST_Rampart_Health = new("WAR_ST_Rampart_Health", 80),
            WAR_ST_Rampart_SubOption = new("WAR_ST_Rampart_SubOption", 1),
            WAR_ST_Thrill_Health = new("WAR_ST_Thrill_Health", 70),
            WAR_ST_Thrill_SubOption = new("WAR_ST_Thrill_SubOption", 1),
            WAR_ST_Vengeance_Health = new("WAR_ST_Vengeance_Health", 60),
            WAR_ST_Vengeance_SubOption = new("WAR_ST_Vengeance_SubOption", 1),
            WAR_ST_Holmgang_Health = new("WAR_ST_Holmgang_Health", 30),
            WAR_ST_Holmgang_SubOption = new("WAR_ST_Holmgang_SubOption", 1),
            WAR_AoE_Bloodwhetting_Health = new("WAR_AoE_BloodwhettingOption", 90),
            WAR_AoE_Bloodwhetting_SubOption = new("WAR_AoE_Bloodwhetting_SubOption", 1),
            WAR_AoE_Equilibrium_Health = new("WAR_AoE_EquilibriumOption", 50),
            WAR_AoE_Equilibrium_SubOption = new("WAR_AoE_Equilibrium_SubOption", 1),
            WAR_AoE_Rampart_Health = new("WAR_AoE_Rampart_Health", 80),
            WAR_AoE_Rampart_SubOption = new("WAR_AoE_Rampart_SubOption", 1),
            WAR_AoE_Thrill_Health = new("WAR_AoE_Thrill_Health", 80),
            WAR_AoE_Thrill_SubOption = new("WAR_AoE_Thrill_SubOption", 1),
            WAR_AoE_Vengeance_Health = new("WAR_AoE_Vengeance_Health", 60),
            WAR_AoE_Vengeance_SubOption = new("WAR_AoE_Vengeance_SubOption", 1),
            WAR_AoE_Holmgang_Health = new("WAR_AoE_Holmgang_Health", 30),
            WAR_AoE_Holmgang_SubOption = new("WAR_AoE_Holmgang_SubOption", 1),
            WAR_VariantCure = new("WAR_VariantCure");

        internal static void Draw(CustomComboPreset preset)
        {
            switch (preset)
            {
                case CustomComboPreset.WAR_ST_Advanced_StormsEye:
                    UserConfig.DrawSliderInt(0, 30, WAR_SurgingRefreshRange,
                        "Seconds remaining before refreshing Surging Tempest.");

                    break;

                case CustomComboPreset.WAR_InfuriateFellCleave:
                    UserConfig.DrawSliderInt(0, 50, WAR_InfuriateRange,
                        "Set how much rage to be at or under to use this feature.");

                    break;

                case CustomComboPreset.WAR_ST_Advanced_Onslaught:
                    UserConfig.DrawSliderInt(0, 2, WAR_KeepOnslaughtCharges,
                        "How many charges to keep ready? (0 = Use All)");

                    break;

                case CustomComboPreset.WAR_ST_Advanced_Infuriate:
                    UserConfig.DrawSliderInt(0, 2, WAR_KeepInfuriateCharges,
                        "How many charges to keep ready? (0 = Use All)");

                    UserConfig.DrawSliderInt(0, 50, WAR_InfuriateSTGauge, 
                        "Use when gauge is under or equal to");

                    break;

                case CustomComboPreset.WAR_Variant_Cure:
                    UserConfig.DrawSliderInt(1, 100, WAR_VariantCure,
                        "Player HP% to be \nless than or equal to:", 200);

                    break;

                case CustomComboPreset.WAR_ST_Advanced_FellCleave:
                    UserConfig.DrawSliderInt(50, 100, WAR_FellCleaveGauge, 
                        "Minimum gauge to spend");

                    break;

                case CustomComboPreset.WAR_AoE_Advanced_Decimate:
                    UserConfig.DrawSliderInt(50, 100, WAR_DecimateGauge, 
                        "Minimum gauge to spend");

                    break;

                case CustomComboPreset.WAR_AoE_Advanced_Infuriate:
                    UserConfig.DrawSliderInt(0, 50, WAR_InfuriateAoEGauge, 
                        "Use when gauge is under or equal to");

                    break;

                case CustomComboPreset.WARPvP_BurstMode_Blota:
                    UserConfig.DrawHorizontalRadioButton(WARPvP.Config.WARPVP_BlotaTiming, "Before Primal Rend", "", 0);
                    UserConfig.DrawHorizontalRadioButton(WARPvP.Config.WARPVP_BlotaTiming, "After Primal Rend", "", 1);

                    break;

                case CustomComboPreset.WAR_ST_Advanced_Bloodwhetting:
                    UserConfig.DrawSliderInt(1, 100, WAR_ST_Bloodwhetting_Health,
                        "Player HP% to be \nless than or equal to:", 200);

                    UserConfig.DrawHorizontalRadioButton(WAR_ST_Bloodwhetting_SubOption, 
                        "All Enemies",
                        "Uses Bloodwhetting regardless of targeted enemy type.", 1);

                    UserConfig.DrawHorizontalRadioButton(WAR_ST_Bloodwhetting_SubOption, 
                        "Bosses Only",
                        "Only uses Bloodwhetting when the targeted enemy is a boss.", 2);

                    break;

                case CustomComboPreset.WAR_AoE_Advanced_Bloodwhetting:
                    UserConfig.DrawSliderInt(1, 100, WAR_AoE_Bloodwhetting_Health,
                        "Player HP% to be \nless than or equal to:", 200);

                    UserConfig.DrawHorizontalRadioButton(WAR_AoE_Bloodwhetting_SubOption, 
                        "All Enemies",
                        "Uses Bloodwhetting regardless of targeted enemy type.", 1);

                    UserConfig.DrawHorizontalRadioButton(WAR_AoE_Bloodwhetting_SubOption, 
                        "Bosses Only",
                        "Only uses Bloodwhetting when the targeted enemy is a boss.", 2);

                    break;

                case CustomComboPreset.WAR_ST_Advanced_Equilibrium:
                    UserConfig.DrawSliderInt(1, 100, WAR_ST_Equilibrium_Health,
                        "Player HP% to be \nless than or equal to:", 200);

                    UserConfig.DrawHorizontalRadioButton(WAR_ST_Equilibrium_SubOption,
                        "All Enemies",
                        "Uses Equilibrium regardless of targeted enemy type.", 1);

                    UserConfig.DrawHorizontalRadioButton(WAR_ST_Equilibrium_SubOption,
                        "Bosses Only",
                        "Only uses Equilibrium when the targeted enemy is a boss.", 2);

                    break;

                case CustomComboPreset.WAR_AoE_Advanced_Equilibrium:
                    UserConfig.DrawSliderInt(1, 100, WAR_AoE_Equilibrium_Health,
                        "Player HP% to be \nless than or equal to:", 200);

                    UserConfig.DrawHorizontalRadioButton(WAR_AoE_Equilibrium_SubOption,
                        "All Enemies",
                        "Uses Equilibrium regardless of targeted enemy type.", 1);

                    UserConfig.DrawHorizontalRadioButton(WAR_AoE_Equilibrium_SubOption,
                        "Bosses Only",
                        "Only uses Equilibrium when the targeted enemy is a boss.", 2);

                    break;

                case CustomComboPreset.WAR_ST_Advanced_Rampart:
                    UserConfig.DrawSliderInt(1, 100, WAR_ST_Rampart_Health,
                        "Player HP% to be \nless than or equal to:", 200);

                    UserConfig.DrawHorizontalRadioButton(WAR_ST_Rampart_SubOption, 
                        "All Enemies",
                        "Uses Rampart regardless of targeted enemy type.", 1);

                    UserConfig.DrawHorizontalRadioButton(WAR_ST_Rampart_SubOption, 
                        "Bosses Only",
                        "Only uses Rampart when the targeted enemy is a boss.", 2);

                    break;

                case CustomComboPreset.WAR_ST_Advanced_Thrill:
                    UserConfig.DrawSliderInt(1, 100, WAR_ST_Thrill_Health,
                        "Player HP% to be \nless than or equal to:", 200);

                    UserConfig.DrawHorizontalRadioButton(WAR_ST_Thrill_SubOption,
                        "All Enemies",
                        "Uses Thrill regardless of targeted enemy type.", 1);

                    UserConfig.DrawHorizontalRadioButton(WAR_ST_Thrill_SubOption,
                        "Bosses Only",
                        "Only uses Thrill when the targeted enemy is a boss.", 2);

                    break;

                case CustomComboPreset.WAR_AoE_Advanced_Rampart:
                    UserConfig.DrawSliderInt(1, 100, WAR_AoE_Rampart_Health,
                        "Player HP% to be \nless than or equal to:", 200);

                    UserConfig.DrawHorizontalRadioButton(WAR_AoE_Rampart_SubOption, 
                        "All Enemies",
                        "Uses Rampart regardless of targeted enemy type.", 1);

                    UserConfig.DrawHorizontalRadioButton(WAR_AoE_Rampart_SubOption, 
                        "Bosses Only",
                        "Only uses Rampart when the targeted enemy is a boss.", 2);

                    break;

                case CustomComboPreset.WAR_AoE_Advanced_Thrill:
                    UserConfig.DrawSliderInt(1, 100, WAR_AoE_Thrill_Health,
                        "Player HP% to be \nless than or equal to:", 200);

                    UserConfig.DrawHorizontalRadioButton(WAR_AoE_Thrill_SubOption,
                        "All Enemies",
                        "Uses Thrill regardless of targeted enemy type.", 1);

                    UserConfig.DrawHorizontalRadioButton(WAR_AoE_Thrill_SubOption,
                        "Bosses Only",
                        "Only uses Thrill when the targeted enemy is a boss.", 2);

                    break;

                case CustomComboPreset.WAR_ST_Advanced_Vengeance:
                    UserConfig.DrawSliderInt(1, 100, WAR_ST_Vengeance_Health, 
                        "Player HP% to be \nless than or equal to:", 200);

                    UserConfig.DrawHorizontalRadioButton(WAR_ST_Vengeance_SubOption, 
                        "All Enemies",
                        "Uses Vengeance regardless of targeted enemy type.", 1);

                    UserConfig.DrawHorizontalRadioButton(WAR_ST_Vengeance_SubOption, 
                        "Bosses Only",
                        "Only uses Vengeance when the targeted enemy is a boss.", 2);

                    break;

                case CustomComboPreset.WAR_AoE_Advanced_Vengeance:
                    UserConfig.DrawSliderInt(1, 100, WAR_AoE_Vengeance_Health,
                        "Player HP% to be \nless than or equal to:", 200);

                    UserConfig.DrawHorizontalRadioButton(WAR_AoE_Vengeance_SubOption, 
                        "All Enemies",
                        "Uses Vengeance regardless of targeted enemy type.", 1);

                    UserConfig.DrawHorizontalRadioButton(WAR_AoE_Vengeance_SubOption, 
                        "Bosses Only",
                        "Only uses Vengeance when the targeted enemy is a boss.", 2);

                    break;

                case CustomComboPreset.WAR_ST_Advanced_Holmgang:
                    UserConfig.DrawSliderInt(1, 100, WAR_ST_Holmgang_Health,
                        "Player HP% to be \nless than or equal to:", 200);

                    UserConfig.DrawHorizontalRadioButton(WAR_ST_Holmgang_SubOption, 
                        "All Enemies",
                        "Uses Hallowed Ground regardless of targeted enemy type.", 1);

                    UserConfig.DrawHorizontalRadioButton(WAR_ST_Holmgang_SubOption, 
                        "Bosses Only",
                        "Only uses Hallowed Ground when the targeted enemy is a boss.", 2);

                    break;

                case CustomComboPreset.WAR_AoE_Advanced_Holmgang:
                    UserConfig.DrawSliderInt(1, 100, WAR_AoE_Holmgang_Health,
                        "Player HP% to be \nless than or equal to:", 200);

                    UserConfig.DrawHorizontalRadioButton(WAR_AoE_Holmgang_SubOption, 
                        "All Enemies",
                        "Uses Hallowed Ground regardless of targeted enemy type.", 1);

                    UserConfig.DrawHorizontalRadioButton(WAR_AoE_Holmgang_SubOption, 
                        "Bosses Only",
                        "Only uses Hallowed Ground when the targeted enemy is a boss.", 2);

                    break;
            }
        }
    }
}
