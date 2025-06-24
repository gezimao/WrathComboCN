﻿using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Hooking;
using Dalamud.Utility;
using ECommons;
using ECommons.DalamudServices;
using ECommons.GameFunctions;
using ECommons.Logging;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using FFXIVClientStructs.FFXIV.Client.Game.Object;
using Lumina.Excel.Sheets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using WrathCombo.Combos.PvE;
using WrathCombo.Core;
using WrathCombo.CustomComboNS;
using WrathCombo.Extensions;
using WrathCombo.Services;
using static FFXIVClientStructs.FFXIV.Client.Game.Character.ActionEffectHandler;
using static WrathCombo.CustomComboNS.Functions.CustomComboFunctions;

namespace WrathCombo.Data
{
    public static class ActionWatching
    {
        internal static Dictionary<uint, Lumina.Excel.Sheets.Action> ActionSheet = Svc.Data.GetExcelSheet<Lumina.Excel.Sheets.Action>()!
            .Where(i => i.RowId is not 7)
            .ToDictionary(i => i.RowId, i => i);

        internal static Dictionary<uint, Trait> TraitSheet = Svc.Data.GetExcelSheet<Trait>()!
            .Where(i => i.ClassJobCategory.IsValid) //All player traits are assigned to a category. Chocobo and other garbage lacks this, thus excluded.
            .ToDictionary(i => i.RowId, i => i);
        private static uint lastAction = 0;

        internal static readonly Dictionary<uint, long> ChargeTimestamps = [];
        internal static readonly Dictionary<uint, long> ActionTimestamps = [];
        internal static readonly Dictionary<uint, long> LastSuccessfulUseTime = [];
        internal static readonly Dictionary<(uint, ulong), long> UsedOnDict = [];

        internal readonly static List<uint> CombatActions = [];

        public delegate void LastActionChangeDelegate();
        public static event LastActionChangeDelegate? OnLastActionChange;

        public delegate void ActionSendDelegate();
        public static event ActionSendDelegate? OnActionSend;

        private unsafe delegate void ReceiveActionEffectDelegate(uint casterEntityId, Character* casterPtr, Vector3* targetPos, Header* header, TargetEffects* effects, GameObjectId* targetEntityIds);
        private readonly static Hook<ReceiveActionEffectDelegate>? ReceiveActionEffectHook;

        private unsafe delegate bool UseActionDelegate(ActionManager* actionManager, ActionType actionType, uint actionId, ulong targetId, uint extraParam, ActionManager.UseActionMode mode, uint comboRouteId, bool* outOptAreaTargeted);
        private readonly static Hook<UseActionDelegate>? UseActionHook;

        private unsafe static void ReceiveActionEffectDetour(uint casterEntityId, Character* casterPtr, Vector3* targetPos, Header* header, TargetEffects* effects, GameObjectId* targetEntityIds)
        {
            ReceiveActionEffectHook!.Original(casterEntityId, casterPtr, targetPos, header, effects, targetEntityIds);

            try
            {
                var rawEffects = (ulong*)effects;
                List<(ulong id, ActionEffects effects)> targets = new();
                for (int i = 0; i < header->NumTargets; ++i)
                {
                    var targetEffects = new ActionEffects();
                    for (int j = 0; j < ActionEffects.MaxCount; ++j)
                        targetEffects[j] = rawEffects[i * 8 + j];

                    targets.Add(new(targetEntityIds[i], targetEffects));
                }

                foreach (var target in targets)
                {
                    foreach (var eff in target.effects)
                    {
#if DEBUG
                        Svc.Log.Verbose($"{eff.Type}, Val:{eff.Value} 0:{eff.Param0}, 1:{eff.Param1}, 2:{eff.Param2}, 3:{eff.Param3}, 4:{eff.Param4} | ({header->ActionId.ActionName()}) -> {Svc.Objects.FirstOrDefault(x => x.GameObjectId == target.id)?.Name}, {eff.AtSource}/{eff.FromTarget}");
#endif
                        if (eff.Type is ActionEffectType.Heal or ActionEffectType.Damage)
                        {
                            if (GetPartyMembers().Any(x => x.GameObjectId == target.id))
                            {
                                var member = GetPartyMembers().First(x => x.GameObjectId == target.id);
                                member.CurrentHP = eff.Type == ActionEffectType.Damage ? Math.Min(member.BattleChara.MaxHp, member.CurrentHP - eff.Value) : Math.Min(member.BattleChara.MaxHp, member.CurrentHP + eff.Value);
                                member.HPUpdatePending = true;
                                Svc.Framework.RunOnTick(() => member.HPUpdatePending = false, TimeSpan.FromSeconds(1.5));
                            }
                        }
                        if (eff.Type is ActionEffectType.MpGain or ActionEffectType.MpLoss)
                        {
                            if (GetPartyMembers().Any(x => x.GameObjectId == (eff.AtSource ? casterEntityId : target.id)))
                            {
                                var member = GetPartyMembers().First(x => x.GameObjectId == (eff.AtSource ? casterEntityId : target.id));
                                member.CurrentMP = eff.Type == ActionEffectType.MpLoss ? Math.Min(member.BattleChara.MaxMp, member.CurrentMP - eff.Value) : Math.Min(member.BattleChara.MaxMp, member.CurrentMP + eff.Value);
                                member.MPUpdatePending = true;
                                Svc.Framework.RunOnTick(() => member.MPUpdatePending = false, TimeSpan.FromSeconds(1.5));
                            }
                        }
                        if (eff.Type is ActionEffectType.ApplyStatusEffectSource)
                        {
                            if (GetPartyMembers().Any(x => x.GameObjectId == casterEntityId))
                            {
                                var member = GetPartyMembers().First(x => x.GameObjectId == (eff.AtSource ? casterEntityId : target.id));
                                member.BuffsGainedAt[eff.Value] = Environment.TickCount64;
                            }
                        }
                        if (eff.Type is ActionEffectType.ApplyStatusEffectTarget)
                        {
                            if (ICDTracker.Trackers.TryGetFirst(x => x.StatusID == eff.Value && x.GameObjectId == (eff.AtSource ? casterEntityId : target.id), out var icd))
                            {
                                icd.ICDClearedTime = DateTime.Now + TimeSpan.FromSeconds(60);
                                icd.TimesApplied += 1;
                            }
                            else
                                ICDTracker.Trackers.Add(new(eff.Value, (eff.AtSource ? casterEntityId : target.id), TimeSpan.FromSeconds(60)));
                        }

                    }
                }



                if ((byte)header->ActionType is 13 or 2) return;
                if (header->ActionId != 7 &&
                    header->ActionId != 8 &&
                    casterEntityId == Svc.ClientState.LocalPlayer.GameObjectId)
                {
                    LastAction = header->ActionId;
                    TimeLastActionUsed = DateTime.Now;
                    if (header->ActionId != CombatActions.LastOrDefault())
                        LastActionUseCount = 1;
                    else
                        LastActionUseCount++;

                    CombatActions.Add(header->ActionId);
                    LastSuccessfulUseTime[header->ActionId] = Environment.TickCount64;

                    if (ActionSheet.TryGetValue(header->ActionId, out var sheet))
                    {
                        switch (sheet.ActionCategory.Value.RowId)
                        {
                            case 2: //Spell
                                LastSpell = header->ActionId;
                                break;
                            case 3: //Weaponskill
                                LastWeaponskill = header->ActionId;
                                break;
                            case 4: //Ability
                                LastAbility = header->ActionId;
                                break;
                        }

                        if (sheet.TargetArea)
                            WrathOpener.CurrentOpener?.ProgressOpener(header->ActionId);
                    }

                    if (Service.Configuration.EnabledOutputLog)
                        OutputLog();
                }
            }
            catch (Exception ex)
            {
                ex.Log();
            }
        }

        private delegate void SendActionDelegate(ulong targetObjectId, byte actionType, uint actionId, ushort sequence, long a5, long a6, long a7, long a8, long a9);
        private static readonly Hook<SendActionDelegate>? SendActionHook;
        private unsafe static void SendActionDetour(ulong targetObjectId, byte actionType, uint actionId, ushort sequence, long a5, long a6, long a7, long a8, long a9)
        {
            try
            {
                OnActionSend?.Invoke();

                if (!InCombat())
                    CombatActions.Clear();

                if (actionType == 1 && GetMaxCharges(actionId) > 0)
                    ChargeTimestamps[actionId] = Environment.TickCount64;

                if (actionType == 1)
                    ActionTimestamps[actionId] = Environment.TickCount64;

                TimeLastActionUsed = DateTime.Now + TimeSpan.FromMilliseconds(ActionManager.GetAdjustedCastTime((ActionType)actionType, actionId));
                LastAction = actionId;
                ActionType = actionType;
                WrathOpener.CurrentOpener?.ProgressOpener(actionId);
                UpdateHelpers(actionId);
                UsedOnDict[(actionId, targetObjectId)] = Environment.TickCount64;
                SendActionHook!.Original(targetObjectId, actionType, actionId, sequence, a5, a6, a7, a8, a9);

                Svc.Log.Verbose($"{actionId} {sequence} {a5} {a6} {a7} {a8} {a9}");
            }
            catch (Exception ex)
            {
                Svc.Log.Error(ex, "SendActionDetour");
                SendActionHook!.Original(targetObjectId, actionType, actionId, sequence, a5, a6, a7, a8, a9);
            }
        }

        private static void UpdateHelpers(uint actionId)
        {
            if (actionId is NIN.Ten or NIN.Chi or NIN.Jin or NIN.TenCombo or NIN.ChiCombo or NIN.JinCombo)
                NIN.InMudra = true;
            else
                NIN.InMudra = false;
        }

        private static bool CheckForChangedTarget(uint actionId, ref ulong targetObjectId, out uint replacedWith)
        {
            replacedWith = actionId;
            if (!P.ActionRetargeting.TryGetTargetFor(actionId, out var target, out replacedWith) ||
                target is null)
                return false;

            targetObjectId = target.GameObjectId;
            return true;
        }

        public static unsafe bool OutOfRange(uint actionId, IGameObject source, IGameObject target)
        {
            return ActionManager.GetActionInRangeOrLoS(actionId, source.Struct(), target.Struct()) is 566;
        }

        /// <summary>
        /// Returns the amount of time since an action was last used.
        /// </summary>
        /// <param name="actionId"></param>
        /// <returns>Time in milliseconds if found, else -1.</returns>
        public static float TimeSinceActionUsed(uint actionId)
        {
            if (ActionTimestamps.TryGetValue(actionId, out long timestamp))
                return Environment.TickCount64 - timestamp;

            return -1f;
        }

        public static float TimeSinceLastSuccessfulCast(uint actionId)
        {
            if (LastSuccessfulUseTime.TryGetValue(actionId, out long timestamp))
                return Environment.TickCount64 - timestamp;

            return -1f;
        }

        public static uint WhichOfTheseActionsWasLast(params uint[] actions)
        {
            if (CombatActions.Count == 0) return 0;

            int currentLastIndex = 0;
            foreach (var action in actions)
            {
                if (CombatActions.Any(x => x == action))
                {
                    int index = CombatActions.LastIndexOf(action);

                    if (index > currentLastIndex) currentLastIndex = index;
                }
            }

            return CombatActions[currentLastIndex];
        }

        public static int HowManyTimesUsedAfterAnotherAction(uint lastUsedIDToCheck, uint idToCheckAgainst)
        {
            if (CombatActions.Count < 2) return 0;
            if (WhichOfTheseActionsWasLast(lastUsedIDToCheck, idToCheckAgainst) != lastUsedIDToCheck) return 0;

            int startingIndex = CombatActions.LastIndexOf(idToCheckAgainst);
            if (startingIndex == -1) return 0;

            int count = 0;
            for (int i = startingIndex + 1; i < CombatActions.Count; i++)
            {
                if (CombatActions[i] == lastUsedIDToCheck) count++;
            }

            return count;
        }

        public static bool HasDoubleWeaved()
        {
            if (CombatActions.Count < 2) return false;
            var lastAction = CombatActions.Last();
            var secondLastAction = CombatActions[^2];

            return (GetAttackType(lastAction) == GetAttackType(secondLastAction) && GetAttackType(lastAction) == ActionAttackType.Ability);
        }

        public static bool HasWeaved()
        {
            if (CombatActions.Count < 1) return false;
            var lastAction = CombatActions.Last();

            return GetAttackType(lastAction) == ActionAttackType.Ability;
        }

        public static int NumberOfGcdsUsed => CombatActions.Count(x => GetAttackType(x) == ActionAttackType.Weaponskill || GetAttackType(x) == ActionAttackType.Spell);
        public static uint LastAction
        {
            get => lastAction;
            set
            {
                if (lastAction != value)
                {
                    OnLastActionChange?.Invoke();
                    lastAction = value;
                }
            }
        }
        public static int LastActionUseCount { get; set; } = 0;
        public static uint ActionType { get; set; } = 0;
        public static uint LastWeaponskill { get; set; } = 0;
        public static uint LastAbility { get; set; } = 0;
        public static uint LastSpell { get; set; } = 0;

        public static TimeSpan TimeSinceLastAction => DateTime.Now - TimeLastActionUsed;

        public static DateTime TimeLastActionUsed { get; set; } = DateTime.Now;

        public static void OutputLog()
        {
            DuoLog.Information($"You just used: {CombatActions.LastOrDefault().ActionName()} x{LastActionUseCount}");
        }

        public static void Dispose()
        {
            Disable();
            ReceiveActionEffectHook?.Dispose();
            SendActionHook?.Dispose();
            UseActionHook?.Dispose();
        }

        static unsafe ActionWatching()
        {
            ReceiveActionEffectHook ??= Svc.Hook.HookFromAddress<ReceiveActionEffectDelegate>(Addresses.Receive.Value, ReceiveActionEffectDetour);
            SendActionHook ??= Svc.Hook.HookFromSignature<SendActionDelegate>("48 89 5C 24 ?? 48 89 6C 24 ?? 48 89 74 24 ?? 57 48 81 EC ?? ?? ?? ?? 48 8B 05 ?? ?? ?? ?? 48 33 C4 48 89 84 24 ?? ?? ?? ?? 48 8B E9 41 0F B7 D9", SendActionDetour);
            UseActionHook ??= Svc.Hook.HookFromAddress<UseActionDelegate>(ActionManager.Addresses.UseAction.Value, UseActionDetour);
        }

        private unsafe static bool UseActionDetour(ActionManager* actionManager, ActionType actionType, uint actionId, ulong targetId, uint extraParam, ActionManager.UseActionMode mode, uint comboRouteId, bool* outOptAreaTargeted)
        {
            try
            {
                if (actionType is FFXIVClientStructs.FFXIV.Client.Game.ActionType.Action or FFXIVClientStructs.FFXIV.Client.Game.ActionType.Ability)
                {
                    var original = actionId; //Save the original action, do not modify
                    var originalTargetId = targetId; //Save the original target, do not modify

                    if (Service.Configuration.PerformanceMode) //Performance mode only logic, to modify the actionId
                    {
                        var result = actionId;
                        foreach (var combo in ActionReplacer.FilteredCombos)
                        {
                            if (combo.TryInvoke(actionId, out result))
                            {
                                actionId = Service.ActionReplacer.LastActionInvokeFor[actionId] = result; //Sets actionId and the LastActionInvokeFor dictionary entry to the result of the combo
                                break;
                            }
                        }
                    }

                    var changed = CheckForChangedTarget(original, ref targetId,
                        out var replacedWith); //Passes the original action to the retargeting framework, outputs a targetId and a replaced action

                    var areaTargeted = Svc.Data
                        .GetExcelSheet<Lumina.Excel.Sheets.Action>()
                        .GetRow(replacedWith).TargetArea;

                    if (changed && !areaTargeted) //Check if the action can be used on the target, and if not revert to original
                        if (!ActionManager.CanUseActionOnTarget(replacedWith,
                            Svc.Objects
                                .FirstOrDefault(x => x.GameObjectId == targetId)
                                .Struct()))
                            targetId = originalTargetId;

                    //Important to pass actionId here and not replaced. Performance mode = result from earlier, which could be modified. Non-performance mode = original action, which gets modified by the hook. Same result.
                    var hookResult = UseActionHook.Original(actionManager, actionType, actionId, targetId, extraParam, mode, comboRouteId, outOptAreaTargeted);

                    // If the target was changed, support changing the target for ground actions, too
                    if (changed)
                        ActionManager.Instance()->AreaTargetingExecuteAtObject =
                            targetId;

                    return hookResult;
                }
                else
                {
                    return UseActionHook.Original(actionManager, actionType, actionId, targetId, extraParam, mode, comboRouteId, outOptAreaTargeted);
                }
            }
            catch
            {
                return UseActionHook.Original(actionManager, actionType, actionId, targetId, extraParam, mode, comboRouteId, outOptAreaTargeted);
            }
        }

        public static void Enable()
        {
            ReceiveActionEffectHook?.Enable();
            SendActionHook?.Enable();
            UseActionHook?.Enable();
            Svc.Condition.ConditionChange += ResetActions;
        }

        private static void ResetActions(ConditionFlag flag, bool value)
        {
            if (flag == ConditionFlag.InCombat && !value)
            {
                CombatActions.Clear();
                ActionTimestamps.Clear();
                LastAbility = 0;
                LastAction = 0;
                LastWeaponskill = 0;
                LastSpell = 0;
                UsedOnDict.Clear();
            }
        }

        public static void Disable()
        {
            ReceiveActionEffectHook.Disable();
            SendActionHook?.Disable();
            UseActionHook?.Disable();
            Svc.Condition.ConditionChange -= ResetActions;
        }

        public static int GetLevel(uint id) => ActionSheet.TryGetValue(id, out var action) && action.ClassJobCategory.IsValid ? action.ClassJobLevel : 255;
        public static float GetActionCastTime(uint id) => ActionSheet.TryGetValue(id, out var action) ? action.Cast100ms / (float)10 : 0;
        public unsafe static int GetActionRange(uint id) => (int)ActionManager.GetActionRange(id);
        public static int GetActionEffectRange(uint id) => ActionSheet.TryGetValue(id, out var action) ? action.EffectRange : -1;
        public static int GetTraitLevel(uint id) => TraitSheet.TryGetValue(id, out var trait) ? trait.Level : 255;
        public static string GetActionName(uint id) => ActionSheet.TryGetValue(id, out var action) ? action.Name.ToDalamudString().ToString() : "UNKNOWN ABILITY";

        public static string GetBLUIndex(uint id)
        {
            var aozKey = Svc.Data.GetExcelSheet<AozAction>()!.First(x => x.Action.RowId == id).RowId;
            var index = Svc.Data.GetExcelSheet<AozActionTransient>().GetRow(aozKey).Number;

            return $"#{index} ";
        }

        public static ActionAttackType GetAttackType(uint id)
        {
            if (!ActionSheet.TryGetValue(id, out var action)) return ActionAttackType.Unknown;

            return action.ActionCategory.RowId switch
            {
                2 => ActionAttackType.Spell,
                3 => ActionAttackType.Weaponskill,
                4 => ActionAttackType.Ability,
                _ => ActionAttackType.Unknown
            };
        }

        public enum ActionAttackType
        {
            Ability,
            Spell,
            Weaponskill,
            Unknown
        }
    }
}
