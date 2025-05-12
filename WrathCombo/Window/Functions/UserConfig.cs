﻿using Dalamud.Interface;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Utility;
using ECommons.ImGuiMethods;
using ImGuiNET;
using System;
using System.Numerics;
using WrathCombo.Combos.PvP;
using WrathCombo.Core;
using WrathCombo.CustomComboNS.Functions;
using WrathCombo.Data;
using WrathCombo.Services;

namespace WrathCombo.Window.Functions
{
    public static class UserConfig
    {
        /// <summary> Draws a slider that lets the user set a given value for their feature. </summary>
        /// <param name="minValue"> The absolute minimum value you'll let the user pick. </param>
        /// <param name="maxValue"> The absolute maximum value you'll let the user pick. </param>
        /// <param name="config"> The config ID. </param>
        /// <param name="sliderDescription"> Description of the slider. Appends to the right of the slider. </param>
        /// <param name="itemWidth"> How long the slider should be. </param>
        /// <param name="sliderIncrement"> How much you want the user to increment the slider by. Uses SliderIncrements as a preset. </param>
        /// <param name="hasAdditionalChoice">True if this config can trigger additional configs depending on value.</param>
        /// <param name="additonalChoiceCondition">What the condition is to convey to the user what triggers it.</param>
        public static bool DrawSliderInt(int minValue, int maxValue, string config, string sliderDescription, float itemWidth = 150, uint sliderIncrement = SliderIncrements.Ones, bool hasAdditionalChoice = false, string additonalChoiceCondition = "")
        {
            ImGui.Indent();
            int output = PluginConfiguration.GetCustomIntValue(config, minValue);
            if (output < minValue)
            {
                output = minValue;
                PluginConfiguration.SetCustomIntValue(config, output);
                Service.Configuration.Save();
            }

            sliderDescription = sliderDescription.Replace("%", "%%");
            float contentRegionMin = ImGui.GetItemRectMax().Y - ImGui.GetItemRectMin().Y;
            float wrapPos = ImGui.GetContentRegionMax().X - 35f;

            InfoBox box = new()
            {
                Color = Colors.White,
                BorderThickness = 1f,
                CurveRadius = 3f,
                AutoResize = true,
                HasMaxWidth = true,
                IsSubBox = true,
                ContentsFunc = () =>
                {
                    bool inputChanged = false;
                    Vector2 currentPos = ImGui.GetCursorPos();
                    ImGui.SetCursorPosX(currentPos.X + itemWidth);
                    ImGui.PushTextWrapPos(wrapPos);
                    ImGui.PushStyleColor(ImGuiCol.Text, ImGuiColors.DalamudWhite);
                    ImGui.Text($"{sliderDescription}");
                    Vector2 height = ImGui.GetItemRectSize();
                    float lines = height.Y / ImGui.GetFontSize();
                    Vector2 textLength = ImGui.CalcTextSize(sliderDescription);
                    string newLines = "";
                    for (int i = 1; i < lines; i++)
                    {
                        if (i % 2 == 0)
                        {
                            newLines += "\n";
                        }
                        else
                        {
                            newLines += "\n\n";
                        }

                    }

                    if (hasAdditionalChoice)
                    {
                        ImGui.SameLine();
                        ImGui.PushStyleColor(ImGuiCol.Text, ImGuiColors.HealerGreen);
                        ImGui.PushFont(UiBuilder.IconFont);
                        ImGuiEx.Spacing(new Vector2(5, 0));
                        ImGui.TextWrapped($"{FontAwesomeIcon.Search.ToIconString()}");
                        ImGui.PopFont();
                        ImGui.PopStyleColor();

                        if (ImGui.IsItemHovered())
                        {
                            ImGui.BeginTooltip();
                            ImGui.TextUnformatted($"This setting has additional options depending on its value.{(string.IsNullOrEmpty(additonalChoiceCondition) ? "" : $"\nCondition: {additonalChoiceCondition}")}");
                            ImGui.EndTooltip();
                        }
                    }

                    ImGui.PopStyleColor();
                    ImGui.PopTextWrapPos();
                    ImGui.SameLine();
                    ImGui.SetCursorPosX(currentPos.X);
                    ImGui.PushItemWidth(itemWidth);
                    inputChanged |= ImGui.SliderInt($"{newLines}###{config}", ref output, minValue, maxValue);

                    if (inputChanged)
                    {
                        if (output % sliderIncrement != 0)
                        {
                            output = output.RoundOff(sliderIncrement);
                            if (output < minValue) output = minValue;
                            if (output > maxValue) output = maxValue;
                        }

                        DebugFile.AddLog($"Set Config {config} to {output}");
                        PluginConfiguration.SetCustomIntValue(config, output);
                        Service.Configuration.Save();
                    }

                    return inputChanged;
                }
            };

            box.Draw();
            DrawResetContextMenu(config);
            ImGui.Spacing();
            ImGui.Unindent();
            return box.FuncRes;
        }

        private static void DrawResetContextMenu(string config, int occurrence = 0)
        {
            if (ImGui.IsItemClicked(ImGuiMouseButton.Right))
                ImGui.OpenPopup($"##ResetConfig{config}{occurrence}");

            using var contextMenu = ImRaii.Popup($"##ResetConfig{config}{occurrence}");
            if (!contextMenu) return;

            if (ImGui.MenuItem("Reset to Default"))
            {
                ResetToDefault(config);
            }
        }

        /// <summary> Draws a slider that lets the user set a given value for their feature. </summary>
        /// <param name="minValue"> The absolute minimum value you'll let the user pick. </param>
        /// <param name="maxValue"> The absolute maximum value you'll let the user pick. </param>
        /// <param name="config"> The config ID. </param>
        /// <param name="sliderDescription"> Description of the slider. Appends to the right of the slider. </param>
        /// <param name="itemWidth"> How long the slider should be. </param>
        /// <param name="hasAdditionalChoice"></param>
        /// <param name="additonalChoiceCondition"></param>
        public static void DrawSliderFloat(float minValue, float maxValue, string config, string sliderDescription, float itemWidth = 150, bool hasAdditionalChoice = false, string additonalChoiceCondition = "")
        {
            float output = PluginConfiguration.GetCustomFloatValue(config, minValue);
            if (output < minValue)
            {
                output = minValue;
                PluginConfiguration.SetCustomFloatValue(config, output);
                Service.Configuration.Save();
            }

            sliderDescription = sliderDescription.Replace("%", "%%");
            float contentRegionMin = ImGui.GetItemRectMax().Y - ImGui.GetItemRectMin().Y;
            float wrapPos = ImGui.GetContentRegionMax().X - 35f;


            InfoBox box = new()
            {
                Color = Colors.White,
                BorderThickness = 1f,
                CurveRadius = 3f,
                AutoResize = true,
                HasMaxWidth = true,
                IsSubBox = true,
                ContentsAction = () =>
                {
                    bool inputChanged = false;
                    Vector2 currentPos = ImGui.GetCursorPos();
                    ImGui.SetCursorPosX(currentPos.X + itemWidth);
                    ImGui.PushTextWrapPos(wrapPos);
                    ImGui.PushStyleColor(ImGuiCol.Text, ImGuiColors.DalamudWhite);
                    ImGui.Text($"{sliderDescription}");
                    Vector2 height = ImGui.GetItemRectSize();
                    float lines = height.Y / ImGui.GetFontSize();
                    Vector2 textLength = ImGui.CalcTextSize(sliderDescription);
                    string newLines = "";
                    for (int i = 1; i < lines; i++)
                    {
                        if (i % 2 == 0)
                        {
                            newLines += "\n";
                        }
                        else
                        {
                            newLines += "\n\n";
                        }

                    }

                    if (hasAdditionalChoice)
                    {
                        ImGui.SameLine();
                        ImGui.PushStyleColor(ImGuiCol.Text, ImGuiColors.HealerGreen);
                        ImGui.PushFont(UiBuilder.IconFont);
                        ImGuiEx.Spacing(new Vector2(5, 0));
                        ImGui.TextWrapped($"{FontAwesomeIcon.Search.ToIconString()}");
                        ImGui.PopFont();
                        ImGui.PopStyleColor();

                        if (ImGui.IsItemHovered())
                        {
                            ImGui.BeginTooltip();
                            ImGui.TextUnformatted($"This setting has additional options depending on its value.{(string.IsNullOrEmpty(additonalChoiceCondition) ? "" : $"\nCondition: {additonalChoiceCondition}")}");
                            ImGui.EndTooltip();
                        }
                    }

                    ImGui.PopStyleColor();
                    ImGui.PopTextWrapPos();
                    ImGui.SameLine();
                    ImGui.SetCursorPosX(currentPos.X);
                    ImGui.PushItemWidth(itemWidth);
                    inputChanged |= ImGui.SliderFloat($"{newLines}###{config}", ref output, minValue, maxValue);

                    if (inputChanged)
                    {
                        DebugFile.AddLog($"Set Config {config} to {output}");
                        PluginConfiguration.SetCustomFloatValue(config, output);
                        Service.Configuration.Save();
                    }
                }
            };

            box.Draw();
            DrawResetContextMenu(config);
            ImGui.Spacing();
        }

        /// <summary> Draws a slider that lets the user set a given value for their feature. </summary>
        /// <param name="minValue"> The absolute minimum value you'll let the user pick. </param>
        /// <param name="maxValue"> The absolute maximum value you'll let the user pick. </param>
        /// <param name="config"> The config ID. </param>
        /// <param name="sliderDescription"> Description of the slider. Appends to the right of the slider. </param>
        /// <param name="itemWidth"> How long the slider should be. </param>
        /// <param name="hasAdditionalChoice"></param>
        /// <param name="additonalChoiceCondition"></param>
        /// <param name="digits"></param>
        public static void DrawRoundedSliderFloat(float minValue, float maxValue, string config, string sliderDescription, float itemWidth = 150, bool hasAdditionalChoice = false, string additonalChoiceCondition = "", int digits = 1)
        {
            float output = PluginConfiguration.GetCustomFloatValue(config, minValue);
            if (output < minValue)
            {
                output = minValue;
                PluginConfiguration.SetCustomFloatValue(config, output);
                Service.Configuration.Save();
            }

            sliderDescription = sliderDescription.Replace("%", "%%");
            float contentRegionMin = ImGui.GetItemRectMax().Y - ImGui.GetItemRectMin().Y;
            float wrapPos = ImGui.GetContentRegionMax().X - 35f;


            InfoBox box = new()
            {
                Color = Colors.White,
                BorderThickness = 1f,
                CurveRadius = 3f,
                AutoResize = true,
                HasMaxWidth = true,
                IsSubBox = true,
                ContentsAction = () =>
                {
                    bool inputChanged = false;
                    Vector2 currentPos = ImGui.GetCursorPos();
                    ImGui.SetCursorPosX(currentPos.X + itemWidth);
                    ImGui.PushTextWrapPos(wrapPos);
                    ImGui.PushStyleColor(ImGuiCol.Text, ImGuiColors.DalamudWhite);
                    ImGui.Text($"{sliderDescription}");
                    Vector2 height = ImGui.GetItemRectSize();
                    float lines = height.Y / ImGui.GetFontSize();
                    Vector2 textLength = ImGui.CalcTextSize(sliderDescription);
                    string newLines = "";
                    for (int i = 1; i < lines; i++)
                    {
                        if (i % 2 == 0)
                        {
                            newLines += "\n";
                        }
                        else
                        {
                            newLines += "\n\n";
                        }

                    }

                    if (hasAdditionalChoice)
                    {
                        ImGui.SameLine();
                        ImGui.PushStyleColor(ImGuiCol.Text, ImGuiColors.HealerGreen);
                        ImGui.PushFont(UiBuilder.IconFont);
                        ImGuiEx.Spacing(new Vector2(5, 0));
                        ImGui.TextWrapped($"{FontAwesomeIcon.Search.ToIconString()}");
                        ImGui.PopFont();
                        ImGui.PopStyleColor();

                        if (ImGui.IsItemHovered())
                        {
                            ImGui.BeginTooltip();
                            ImGui.TextUnformatted($"This setting has additional options depending on its value.{(string.IsNullOrEmpty(additonalChoiceCondition) ? "" : $"\nCondition: {additonalChoiceCondition}")}");
                            ImGui.EndTooltip();
                        }
                    }

                    ImGui.PopStyleColor();
                    ImGui.PopTextWrapPos();
                    ImGui.SameLine();
                    ImGui.SetCursorPosX(currentPos.X);
                    ImGui.PushItemWidth(itemWidth);
                    inputChanged |= ImGui.SliderFloat($"{newLines}###{config}", ref output, minValue, maxValue, $"%.{digits}f");

                    if (inputChanged)
                    {
                        DebugFile.AddLog($"Set Config {config} to {output}");
                        PluginConfiguration.SetCustomFloatValue(config, output);
                        Service.Configuration.Save();
                    }
                }
            };

            box.Draw();
            DrawResetContextMenu(config);
            ImGui.Spacing();
        }

        /// <summary> Draws a checkbox intended to be linked to other checkboxes sharing the same config value. </summary>
        /// <param name="config"> The config ID. </param>
        /// <param name="checkBoxName"> The name of the feature. </param>
        /// <param name="checkboxDescription"> The description of the feature. </param>
        /// <param name="outputValue"> If the user ticks this box, this is the value the config will be set to. </param>
        /// <param name="itemWidth"></param>
        /// <param name="descriptionColor"></param>
        /// <param name="descriptionAsTooltip">Whether to only show the Description as a tooltip</param>
        public static void DrawRadioButton(string config, string checkBoxName, string checkboxDescription, int outputValue, float itemWidth = 150, Vector4 descriptionColor = new Vector4(), bool descriptionAsTooltip = false)
        {
            ImGui.Indent();
            if (descriptionColor == new Vector4()) descriptionColor = ImGuiColors.DalamudYellow;
            int output = PluginConfiguration.GetCustomIntValue(config, outputValue);
            ImGui.PushItemWidth(itemWidth);
            ImGui.SameLine();
            ImGuiEx.Spacing(new Vector2(21, 0));
            bool enabled = output == outputValue;

            if (ImGui.RadioButton($"{checkBoxName}###{config}{outputValue}", enabled))
            {
                DebugFile.AddLog($"Set Config {config} to {output}");
                PluginConfiguration.SetCustomIntValue(config, outputValue);
                Service.Configuration.Save();
            }

            if (!checkboxDescription.IsNullOrEmpty())
            {
                if (descriptionAsTooltip)
                {
                    if (ImGui.IsItemHovered())
                    {
                        ImGui.BeginTooltip();
                        ImGui.TextUnformatted(checkboxDescription);
                        ImGui.EndTooltip();
                    }
                }
                else if (!descriptionAsTooltip)
                {
                    ImGui.PushStyleColor(ImGuiCol.Text, descriptionColor);
                    ImGui.TextWrapped(checkboxDescription);
                    ImGui.PopStyleColor();
                }
            }

            ImGui.Unindent();
            ImGui.Spacing();
        }

        /// <summary> Draws a checkbox in a horizontal configuration intended to be linked to other checkboxes sharing the same config value. </summary>
        /// <param name="config"> The config ID. </param>
        /// <param name="checkBoxName"> The name of the feature. </param>
        /// <param name="checkboxDescription"> The description of the feature. </param>
        /// <param name="outputValue"> If the user ticks this box, this is the value the config will be set to. </param>
        /// <param name="itemWidth"></param>
        /// <param name="descriptionColor"></param>
        public static bool DrawHorizontalRadioButton(string config, string checkBoxName, string checkboxDescription, int outputValue, float itemWidth = 150, Vector4 descriptionColor = new Vector4())
        {
            if (descriptionColor == new Vector4()) descriptionColor = ImGuiColors.DalamudYellow;
            int output = PluginConfiguration.GetCustomIntValue(config);
            ImGui.SameLine();
            ImGui.PushItemWidth(itemWidth);
            var labelW = ImGui.CalcTextSize(checkBoxName);
            var finishPos = ImGui.GetCursorPosX() + labelW.X + ImGui.GetStyle().ItemSpacing.X;
            if (finishPos >= ImGui.GetContentRegionMax().X)
                ImGui.NewLine();

            bool enabled = output == outputValue;

            bool o = false;
            using (ImRaii.PushColor(ImGuiCol.Text, descriptionColor))
            {
                if (ImGui.RadioButton($"{checkBoxName}###{config}{outputValue}", enabled))
                {
                    DebugFile.AddLog($"Set Config {config} to {output}");
                    PluginConfiguration.SetCustomIntValue(config, outputValue);
                    Service.Configuration.Save();
                    o = true;
                }

                if (!checkboxDescription.IsNullOrEmpty() && ImGui.IsItemHovered())
                {
                    ImGui.BeginTooltip();
                    ImGui.TextUnformatted(checkboxDescription);
                    ImGui.EndTooltip();
                }
            }

            DrawResetContextMenu(config, outputValue);

            return o;
        }

        /// <summary>
        ///     Draws a checkbox in a horizontal configuration intended to be linked
        ///     to other checkboxes sharing the same config value. Same as the method
        ///     above, but for <see cref="UserBoolArray">UserBoolArrays</see>.
        /// </summary>
        /// <param name="config"> The config ID. </param>
        /// <param name="checkBoxName"> The name of the feature. </param>
        /// <param name="checkboxDescription"> The description of the feature. </param>
        /// /// <param name="choice"> If the user ticks this box, this is the value the config will be set to. </param>
        /// <param name="itemWidth"></param>
        /// <param name="descriptionColor"></param>
        public static void DrawHorizontalBoolRadioButton(string config, string
                checkBoxName, string checkboxDescription, int choice, float itemWidth = 150, Vector4 descriptionColor = new Vector4())
        {
            if (descriptionColor == new Vector4()) descriptionColor = ImGuiColors.DalamudYellow;
            bool[]? values = PluginConfiguration.GetCustomBoolArrayValue(config);
            ImGui.PushItemWidth(itemWidth);

            using (ImRaii.PushColor(ImGuiCol.Text, descriptionColor))
            {
                if (ImGui.RadioButton($"{checkBoxName}###{config}{choice}", values[choice]))
                {
                    for (var i = 0; i < values.Length; i++)
                        values[i] = false;
                    values[choice] = true;
                    DebugFile.AddLog($"Set Config {config} to {string.Join(", ", values)}");
                    PluginConfiguration.SetCustomBoolArrayValue(config, values);
                    Service.Configuration.Save();
                }

                if (!checkboxDescription.IsNullOrEmpty() && ImGui.IsItemHovered())
                {
                    ImGui.BeginTooltip();
                    ImGui.TextUnformatted(checkboxDescription);
                    ImGui.EndTooltip();
                }
            }

            DrawResetContextMenu(config, choice);

            ImGui.SameLine();
        }

        /// <summary>A true or false configuration. Similar to presets except can be used as part of a condition on another config.</summary>
        /// <param name="config">The config ID.</param>
        /// <param name="checkBoxName">The name of the feature.</param>
        /// <param name="checkboxDescription">The description of the feature</param>
        /// <param name="itemWidth"></param>
        /// <param name="isConditionalChoice"></param>
        public static void DrawAdditionalBoolChoice(string config, string checkBoxName, string checkboxDescription, float itemWidth = 150, bool isConditionalChoice = false)
        {
            bool output = PluginConfiguration.GetCustomBoolValue(config);
            ImGui.PushItemWidth(itemWidth);
            if (!isConditionalChoice)
                ImGui.Indent();
            else
            {
                using (ImRaii.PushColor(ImGuiCol.Text, ImGuiColors.HealerGreen))
                {
                    ImGui.PushFont(UiBuilder.IconFont);
                    ImGui.AlignTextToFramePadding();
                    ImGui.TextWrapped($"{FontAwesomeIcon.Plus.ToIconString()}");
                    ImGui.PopFont();
                }

                ImGui.SameLine();
                ImGuiEx.Spacing(new Vector2(3, 0));
                if (isConditionalChoice) ImGui.Indent(); //Align checkbox after the + symbol
            }
            if (ImGui.Checkbox($"{checkBoxName}##{config}", ref output))
            {
                DebugFile.AddLog($"Set Config {config} to {output}");
                PluginConfiguration.SetCustomBoolValue(config, output);
                Service.Configuration.Save();
            }

            DrawResetContextMenu(config);

            if (!checkboxDescription.IsNullOrEmpty())
            {
                ImGuiEx.TextWrapped(ImGuiColors.DalamudGrey, checkboxDescription);
            }

            //!isConditionalChoice
            ImGui.Unindent();
            ImGui.Spacing();
        }

        /// <summary> Draws multi choice checkboxes in a horizontal configuration. </summary>
        /// <param name="config"> The config ID. </param>
        /// <param name="checkBoxName"> The name of the feature. </param>
        /// <param name="checkboxDescription"> The description of the feature. </param>
        /// <param name="totalChoices"> The total number of options for the feature </param>
        /// /// <param name="choice"> If the user ticks this box, this is the value the config will be set to. </param>
        /// <param name="descriptionColor"></param>
        public static void DrawHorizontalMultiChoice(string config, string checkBoxName, string checkboxDescription, int totalChoices, int choice, Vector4 descriptionColor = new Vector4())
        {
            ImGui.Indent();
            if (descriptionColor == new Vector4()) descriptionColor = ImGuiColors.DalamudWhite;
            bool[]? values = PluginConfiguration.GetCustomBoolArrayValue(config);

            //If new saved options or amount of choices changed, resize and save
            if (values.Length == 0 || values.Length != totalChoices)
            {
                Array.Resize(ref values, totalChoices);
                PluginConfiguration.SetCustomBoolArrayValue(config, values);
                Service.Configuration.Save();
            }

            using (ImRaii.PushColor(ImGuiCol.Text, descriptionColor))
            {
                if (choice > 0)
                {
                    ImGui.SameLine();
                    ImGuiEx.Spacing(new Vector2(12, 0));
                }

                var labelW = ImGui.CalcTextSize(checkBoxName);
                var finishPos = ImGui.GetCursorPosX() + labelW.X + ImGui.GetStyle().ItemSpacing.X;
                if (finishPos >= ImGui.GetContentRegionMax().X)
                    ImGui.NewLine();

                if (ImGui.Checkbox($"{checkBoxName}###{config}{choice}", ref values[choice]))
                {
                    DebugFile.AddLog($"Set Config {config} to {string.Join(", ", values)}");
                    PluginConfiguration.SetCustomBoolArrayValue(config, values);
                    Service.Configuration.Save();
                }

                if (!checkboxDescription.IsNullOrEmpty() && ImGui.IsItemHovered())
                {
                    ImGui.BeginTooltip();
                    ImGui.TextUnformatted(checkboxDescription);
                    ImGui.EndTooltip();
                }
            }

            DrawResetContextMenu(config, choice);
            ImGui.Unindent();
        }

        /// <seealso cref="PvPCommon.QuickPurify.Statuses">
        ///     PvP Purifiable Statuses List
        /// </seealso>
        /// <seealso cref="PvPCommon.Config.QuickPurifyStatuses">
        ///     User-Selected List of Status to Purify
        /// </seealso>
        public static void DrawPvPStatusMultiChoice(string config)
        {
            bool[]? values = PluginConfiguration.GetCustomBoolArrayValue(config);
            Array.Resize(ref values, PvPCommon.QuickPurify.Statuses.Length);

            ImGui.PushStyleColor(ImGuiCol.Text, ImGuiColors.ParsedPink);
            ImGui.Columns(4, $"{config}", false);

            for (var i = 0; i < PvPCommon.QuickPurify.Statuses.Length; i++)
            {
                var status = PvPCommon.QuickPurify.Statuses[i];
                if (ImGui.Checkbox($"{status.label}###{config}{i}", ref values[i]))
                {
                    DebugFile.AddLog($"Set Config {config} to {string.Join(", ", values)}");
                    PluginConfiguration.SetCustomBoolArrayValue(config, values);
                    Service.Configuration.Save();
                }

                ImGui.NextColumn();
            }

            ImGui.Columns(1);
            ImGui.PopStyleColor();
            ImGui.Spacing();
        }

        /// <summary>
        ///     Draws the correct multi choice checkbox method based on the given
        ///     <see cref="ContentCheck.ListSet"/>.
        /// </summary>
        /// <param name="config">
        ///     The <see cref="UserBoolArray"/> config variable for this setting.
        /// </param>
        /// <param name="configListSet">
        ///     Which difficulty list set to draw.
        /// </param>
        /// <param name="overrideText">
        ///     Optional text to override the default description.
        /// </param>
        /// <seealso cref="DrawHalvedDifficultyMultiChoice"/>
        /// <seealso cref="DrawCasualVSHardDifficultyMultiChoice"/>
        /// <seealso cref="DrawCoredDifficultyMultiChoice"/>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public static void DrawDifficultyMultiChoice
            (string config, ContentCheck.ListSet configListSet, string overrideText = "")
        {
            switch (configListSet)
            {
                case ContentCheck.ListSet.Halved:
                    DrawHalvedDifficultyMultiChoice(config, overrideText);
                    break;
                case ContentCheck.ListSet.CasualVSHard:
                    DrawCasualVSHardDifficultyMultiChoice(config, overrideText);
                    break;
                case ContentCheck.ListSet.Cored:
                    DrawCoredDifficultyMultiChoice(config, overrideText);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(configListSet), configListSet, null);
            }
        }

        /// <summary>
        ///     Draws a multi choice checkbox in a horizontal configuration,
        ///     with values for Content Difficulty filtering's Halved Difficulty
        ///     list set.
        /// </summary>
        /// <value>
        ///     <c>[0]true</c> if <see cref="ContentCheck.BottomHalfContent"/>
        ///     is enabled.<br/>
        ///     <c>[1]true</c> if <see cref="ContentCheck.TopHalfContent"/>
        ///     is enabled.
        /// </value>
        /// <param name="config">
        ///     The <see cref="UserBoolArray"/> config variable for this setting.
        /// </param>
        /// <param name="overrideText">
        ///     Optional text to override the default description.
        /// </param>
        /// <seealso cref="ContentCheck.IsInBottomHalfContent"/>
        /// <seealso cref="ContentCheck.IsInTopHalfContent"/>
        private static void DrawHalvedDifficultyMultiChoice
            (string config, string overrideText = "")
        {
            ImGui.PushStyleColor(ImGuiCol.Text, ImGuiColors.DalamudYellow);
            ImGui.Indent();
            ImGui.TextUnformatted(overrideText.IsNullOrEmpty()
                ? "Select what difficulty the above should apply to:"
                : overrideText);
            ImGui.PopStyleColor();
            ImGui.Unindent();

            DrawHorizontalMultiChoice(
                config, "Easiest Content",
                ContentCheck.BottomHalfContentList,
                totalChoices: 2, choice: 0,
                descriptionColor: ImGuiColors.DalamudYellow
            );
            DrawHorizontalMultiChoice(
                config, "Hardest Content",
                ContentCheck.TopHalfContentList,
                totalChoices: 2, choice: 1,
                descriptionColor: ImGuiColors.DalamudYellow
            );
        }

        /// <summary>
        ///     Draws a multi choice checkbox in a horizontal configuration,
        ///     with values for Content Difficulty filtering's Casual vs Hard
        ///     difficulty list set.
        /// </summary>
        /// <value>
        ///     <c>[0]true</c> if <see cref="ContentCheck.CasualContent"/>
        ///     is enabled.<br/>
        ///     <c>[1]true</c> if <see cref="ContentCheck.HardContent"/>
        ///     is enabled.
        /// </value>
        /// <param name="config">
        ///     The <see cref="UserBoolArray"/> config variable for this setting.
        /// </param>
        /// <param name="overrideText">
        ///     Optional text to override the default description.
        /// </param>
        /// <seealso cref="ContentCheck.IsInCasualContent"/>
        /// <seealso cref="ContentCheck.IsInHardContent"/>
        private static void DrawCasualVSHardDifficultyMultiChoice
            (string config, string overrideText = "")
        {
            ImGui.PushStyleColor(ImGuiCol.Text, ImGuiColors.DalamudYellow);
            ImGui.Indent();
            ImGui.TextUnformatted(overrideText.IsNullOrEmpty()
                ? "Select what difficulty the above should apply to:"
                : overrideText);
            ImGui.PopStyleColor();
            ImGui.Unindent();

            DrawHorizontalMultiChoice(
                config, "Casual Content",
                ContentCheck.CasualContentList,
                totalChoices: 2, choice: 0,
                descriptionColor: ImGuiColors.DalamudYellow
            );
            DrawHorizontalMultiChoice(
                config, "'Hard' Content",
                ContentCheck.HardContentList,
                totalChoices: 2, choice: 1,
                descriptionColor: ImGuiColors.DalamudYellow
            );
        }

        /// <summary>
        ///     Draws a multi choice checkbox in a horizontal configuration,
        ///     with values for Content Difficulty filtering's Cored Difficulty
        ///     list set.
        /// </summary>
        /// <value>
        ///     <c>[0]true</c> if <see cref="ContentCheck.SoftCoreContent"/>
        ///     is enabled.<br/>
        ///     <c>[1]true</c> if <see cref="ContentCheck.MidCoreContent"/>
        ///     is enabled.<br/>
        ///     <c>[2]true</c> if <see cref="ContentCheck.HardCoreContent"/>
        ///     is enabled.
        /// </value>
        /// <param name="config">
        ///     The <see cref="UserBoolArray"/> config variable for this setting.
        /// </param>
        /// <param name="overrideText">
        ///     Optional text to override the default description.
        /// </param>
        /// <seealso cref="ContentCheck.IsInSoftCoreContent"/>
        /// <seealso cref="ContentCheck.IsInMidCoreContent"/>
        /// <seealso cref="ContentCheck.IsInHardCoreContent"/>
        private static void DrawCoredDifficultyMultiChoice
            (string config, string overrideText = "")
        {
            ImGui.PushStyleColor(ImGuiCol.Text, ImGuiColors.DalamudYellow);
            ImGui.Indent();
            ImGui.TextUnformatted(overrideText.IsNullOrEmpty()
                ? "Select what difficulty the above should apply to:"
                : overrideText);
            ImGui.PopStyleColor();
            ImGui.Unindent();

            DrawHorizontalMultiChoice(
                config, "SoftCore Content",
                ContentCheck.SoftCoreContentList,
                totalChoices: 3, choice: 0,
                descriptionColor: ImGuiColors.DalamudYellow
            );
            DrawHorizontalMultiChoice(
                config, "MidCore Content",
                ContentCheck.MidCoreContentList,
                totalChoices: 3, choice: 1,
                descriptionColor: ImGuiColors.DalamudYellow
            );
            DrawHorizontalMultiChoice(
                config, "HardCore Content",
                ContentCheck.HardCoreContentList,
                totalChoices: 3, choice: 2,
                descriptionColor: ImGuiColors.DalamudYellow
            );
        }

        /// <summary>
        ///     Draws a multi choice checkbox in a horizontal configuration,
        ///     with values for Content Difficulty filtering's Boss-Only Difficulty
        ///     list set.
        /// </summary>
        /// <remarks>
        ///     TODO: This should become private additional single choice options added.
        /// </remarks>
        /// <value>
        ///     <c>[0]true</c> if in any content<br/>
        ///     <c>[1]true</c> if Boss-Only content is enabled.<br/>
        /// </value>
        /// <param name="config">
        ///     The <see cref="UserBoolArray"/> config variable for this setting.
        /// </param>
        /// <param name="overrideText">
        ///     Optional text to override the default description.
        /// </param>
        /// <seealso cref="ContentCheck.IsInBossOnlyContent"/>
        internal static void DrawBossOnlyChoice(UserBoolArray config, string overrideText = "")
        {
            using (ImRaii.PushColor(ImGuiCol.Text, ImGuiColors.DalamudYellow))
            {
                ImGui.Text(overrideText.IsNullOrEmpty()
                    ? "Select what kind of content this option applies to:"
                    : overrideText);
            }

            DrawHorizontalBoolRadioButton(
                config, "All Content",
                "Applies to all content in the game.",
                choice: 0,
                descriptionColor: ImGuiColors.DalamudYellow
            );

            DrawHorizontalBoolRadioButton(
                config, "Boss Only Content",
                "Only applies in instances where you directly fight a boss. Excludes many A Realm Reborn & Heavensward raids that include trash.",
                choice: 1,
                descriptionColor: ImGuiColors.DalamudYellow
            );

        }

        /// <summary>
        ///     Draws a multi choice checkbox in a horizontal configuration,
        ///     with values for Content Difficulty filtering's Boss-Only Difficulty
        ///     list set.
        /// </summary>
        /// <remarks>
        ///     TODO: This should become private additional single choice options added.
        /// </remarks>
        /// <value>
        ///     <c>[0]true</c> if in any content<br/>
        ///     <c>[1]true</c> if Boss-Only content is enabled.<br/>
        /// </value>
        /// <param name="config">
        ///     The <see cref="UserInt"/> config variable for this setting.
        /// </param>
        /// <param name="overrideText">
        ///     Optional text to override the default description.
        /// </param>
        /// <seealso cref="ContentCheck.IsInBossOnlyContent"/>
        internal static void DrawBossOnlyChoice(UserInt config, string overrideText = "")
        {
            using (ImRaii.PushColor(ImGuiCol.Text, ImGuiColors.DalamudYellow))
            {

                ImGui.TextUnformatted(overrideText.IsNullOrEmpty()
                    ? "Select what kind of content this option applies to:"
                    : overrideText);
            }

            DrawHorizontalRadioButton(
                config, "All Content",
                "Applies to all content in the game.",
                outputValue: 0,
                descriptionColor: ImGuiColors.DalamudYellow
            );
            DrawHorizontalRadioButton(
                config, "Boss Only Content",
                "Only applies in instances where you directly fight a boss. Excludes many A Realm Reborn & Heavensward raids that include trash.",
                outputValue: 1,
                descriptionColor: ImGuiColors.DalamudYellow
            );

        }

        internal static void DrawPriorityInput(UserIntArray config, int maxValues, int currentItem, string customLabel = "")
        {
            if (config.Count != maxValues || config.Any(x => x == 0))
            {
                config.Clear(maxValues);
                for (int i = 1; i <= maxValues; i++)
                {
                    config[i - 1] = i;
                }
            }

            int curVal = config[currentItem];
            int oldVal = config[currentItem];

            InfoBox box = new()
            {
                Color = Colors.Blue,
                BorderThickness = 1f,
                CurveRadius = 3f,
                AutoResize = true,
                HasMaxWidth = true,
                IsSubBox = true,
                ContentsAction = () =>
                {
                    if (customLabel.IsNullOrEmpty())
                    {
                        ImGui.TextUnformatted($"Priority: ");
                    }
                    else
                    {
                        ImGui.TextUnformatted(customLabel);
                    }
                    ImGui.SameLine();
                    ImGui.PushItemWidth(100f);

                    if (ImGui.InputInt($"###Priority{config.Name}{currentItem}", ref curVal))
                    {
                        for (int i = 0; i < maxValues; i++)
                        {
                            if (i == currentItem)
                                continue;

                            if (config[i] == curVal)
                            {
                                config[i] = oldVal;
                                config[currentItem] = curVal;
                                break;
                            }
                        }
                    }
                }
            };

            ImGui.Indent();
            box.Draw();
            if (ImGui.IsItemHovered())
            {
                ImGui.BeginTooltip();
                ImGui.Text("Smaller Number = Higher Priority");
                ImGui.EndTooltip();
            }
            ImGui.Unindent();
            ImGui.Spacing();
        }

        public static int RoundOff(this int i, uint sliderIncrement)
        {
            double sliderAsDouble = Convert.ToDouble(sliderIncrement);
            return ((int)Math.Round(i / sliderAsDouble)) * (int)sliderIncrement;
        }

        private static void ResetToDefault(string config)
        {
            DebugFile.AddLog($"Set Config {config} to default");
            UserData.MasterList[config].ResetToDefault();
        }
    }

    public static class SliderIncrements
    {
        public const uint
            Ones = 1,
            Fives = 5,
            Tens = 10,
            Hundreds = 100,
            Thousands = 1000;
    }
}
