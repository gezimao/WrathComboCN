#region

using Dalamud.Game.ClientState.Objects.Types;
using ECommons.DalamudServices;
using ECommons.GameFunctions;
using ECommons.GameHelpers;
using ECommons.Logging;
using System;
using System.Linq;
using WrathCombo.Attributes;
using WrathCombo.Combos.PvE;
using WrathCombo.Core;
using WrathCombo.CustomComboNS.Functions;
using WrathCombo.Data;
using WrathCombo.Extensions;
using WrathCombo.Services;
using EZ = ECommons.Throttlers.EzThrottler;
using TS = System.TimeSpan;

// ReSharper disable CheckNamespace
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable InconsistentNaming
// ReSharper disable UnusedMember.Global

#endregion

namespace WrathCombo.CustomComboNS;

internal static class SimpleTarget
{
    #region Common Target Stacks

    /// <summary>
    ///     A collection of common targeting "stacks", used when you want a number of
    ///     different target options, with fallback values.<br />
    ///     (and overriding values)
    /// </summary>
    internal static class Stack
    {
        /// A stack of Mouse Over targets (including model mouseover).
        public static IGameObject? MouseOver =>
            UIMouseOverTarget ?? ModelMouseOverTarget;

        /// A very common stack that targets an ally or self, if there are no manual
        /// overrides targeted.
        /// <remarks>
        ///     "Overrides" include MouseOver at the top of the stack and the
        ///     Hard Target near the bottom.
        /// </remarks>
        public static IGameObject? OverridesAllies =>
            UIMouseOverTarget ?? FocusTarget.IfFriendly() ??
            SoftTarget.IfFriendly() ?? HardTarget.IfFriendly() ??
            Self;

        /// A very common stack that targets the player, if there are no manual
        /// overrides targeted.
        /// <remarks>
        ///     "Overrides" include MouseOver at the top of the stack and the
        ///     Hard Target near the bottom.
        /// </remarks>
        public static IGameObject? OverridesSelf =>
            UIMouseOverTarget ?? HardTarget ?? Self;

        /// A very common stack that targets an ally or self.
        public static IGameObject? Allies =>
            FocusTarget.IfFriendly() ?? SoftTarget.IfFriendly() ??
            HardTarget.IfFriendly() ?? Self;

        /// A little mask for Plugin Configuration to make the string a bit shorter.
        private static PluginConfiguration cfg =>
            Service.Configuration;

        /// <summary>
        ///     A very common stack to pick a heal target, whether the user is
        ///     using the Default or Custom Heal Stack.
        /// </summary>
        /// <seealso cref="DefaultHealStack"/>
        /// <seealso cref="CustomHealStack"/>
        public static IGameObject? AllyToHeal => GetStack();

        /// <summary>
        ///     The Default Heal Stack, with customization options.
        /// </summary>
        /// <remarks>
        ///     LowestHPPAlly and FocusTarget are the only ones with a range check,
        ///     as the others are "intentional" at the time they are grabbed.
        /// </remarks>
        internal static IGameObject? DefaultHealStack =>
            GetStack(StackOption.DefaultHealStack);

        /// <summary>
        ///     The Custom Heal Stack, fully user-made.
        /// </summary>
        /// <seealso cref="PluginConfiguration.CustomHealStack"/>
        /// <seealso cref="GetStack"/>
        internal static IGameObject? CustomHealStack =>
            GetStack(StackOption.CustomHealStack);

        /// <summary>
        ///     The <see cref="AllyToHeal">Heal Stack</see>, but filtered to
        ///     those with a cleansable status effect.
        /// </summary>
        public static IGameObject? AllyToEsuna =>
            GetStack(logicForEachEntryInStack:
                (target) => target.IfHasCleansable());

        /// <summary>
        ///     The Customizable Raise Stack.
        /// </summary>
        public static IGameObject? AllyToRaise =>
            GetStack(StackOption.RaiseStack);

        #region Custom Stack Resolving

        /// <summary>
        ///     Gets the desired Stack, and applies any custom logic to each entry in
        ///     the stack.
        /// </summary>
        /// <param name="stack">
        ///     Which <see cref="StackOption">Stack</see> to get.<br />
        ///     Defaults to <see cref="StackOption.UserChosenHealStack" />.
        /// </param>
        /// <param name="logicForEachEntryInStack">
        ///     A short method, probably of <see cref="GameObjectExtensions" />,
        ///     to apply to each entry in the stack.<br />
        ///     <see cref="AllyToEsuna"/> for an example.
        /// </param>
        /// <returns>
        ///     The first matching target in the stack, or <see langword="null" />.
        /// </returns>
        private static IGameObject? GetStack
            (StackOption stack = StackOption.UserChosenHealStack,
                Func<IGameObject?, IGameObject?>? logicForEachEntryInStack = null)
        {
            #region Default Heal Stack
            if (stack is StackOption.DefaultHealStack ||
                (stack is StackOption.UserChosenHealStack &&
                 !cfg.UseCustomHealStack))
                return
                    (cfg.UseUIMouseoverOverridesInDefaultHealStack
                        ? CustomLogic(UIMouseOverTarget.IfFriendly())
                        : null) ??
                    (cfg.UseFieldMouseoverOverridesInDefaultHealStack
                        ? CustomLogic(ModelMouseOverTarget.IfFriendly())
                        : null) ??
                    CustomLogic(SoftTarget.IfFriendly()) ??
                    CustomLogic(HardTarget.IfFriendly()) ??
                    (cfg.UseFocusTargetOverrideInDefaultHealStack
                        ? CustomLogic(FocusTarget.IfFriendly().IfWithinRange())
                        : null) ??
                    (cfg.UseLowestHPOverrideInDefaultHealStack
                        ? CustomLogic(LowestHPPAlly.IfWithinRange().IfMissingHP())
                        : null) ??
                    Self;
            #endregion

            #region Custom Heal Stack
            if (stack is StackOption.CustomHealStack ||
                (stack is StackOption.UserChosenHealStack &&
                 cfg.UseCustomHealStack))
            {
                var logging = EZ.Throttle("customHealStackLog", TS.FromSeconds(10));

                foreach (var name in Service.Configuration.CustomHealStack)
                {
                    var resolved = GetSimpleTargetValueFromName(name);
                    var target =
                        CustomLogic(resolved.IfFriendly().IfTargetable().IfWithinRange());

                    // Only include Missing-HP options if they are missing HP
                    if (name.Contains("Missing"))
                        target = target.IfMissingHP();

                    if (logging)
                        PluginLog.Verbose(
                            $"[Custom Heal Stack] {name,-25} => " +
                            $"{resolved?.Name ?? "null",-30}" +
                            $" (friendly: {resolved.IsFriendly(),5}, " +
                            $"within range: {resolved.IsWithinRange(),5}, " +
                            $"missing HP: {resolved.IsMissingHP(),5})"
                        );

                    if (target != null) return target;
                }

                // Fall back to Self, if the stack is small and returned nothing
                if (Service.Configuration.CustomHealStack.Length <= 3)
                    return Self;
            }
            #endregion

            #region Raise Stack
            if (stack is StackOption.RaiseStack)
            {
                var logging = EZ.Throttle("raiseStackLog", TS.FromSeconds(10));

                foreach (var name in Service.Configuration.RaiseStack)
                {
                    var resolved = GetSimpleTargetValueFromName(name);
                    var target =
                        CustomLogic(resolved.IfFriendly().IfTargetable()
                            .IfDead().IfWithinRange(30));

                    if (logging)
                        PluginLog.Verbose(
                            $"[Custom Raise Stack] {name,-25} => " +
                            $"{resolved?.Name ?? "null",-30}" +
                            $" (friendly: {resolved.IsFriendly(),5}, " +
                            $"within range: {resolved.IsWithinRange(),5}, " +
                            $"is dead: {resolved.IsDead(),5})"
                        );

                    if (target != null) return target;
                }

                // Fall back to Hard Target, if the stack is small and returned nothing
                if (Service.Configuration.RaiseStack.Length <= 4)
                    return HardTarget.IfFriendly().IfDead() ?? AnyDeadPartyMember;
            }
            #endregion

            return null;

            IGameObject? CustomLogic(IGameObject? target)
            {
                if (target is null) return null;
                if (logicForEachEntryInStack is null) return target;

                return logicForEachEntryInStack(target);
            }
        }

        private static IGameObject? GetSimpleTargetValueFromName(string name)
        {
            try
            {
                var property = typeof(SimpleTarget).GetProperty(name);
                if (property == null) return null;
                var value = property.GetValue(null);
                return value as IGameObject;
            }
            catch (Exception e)
            {
                PluginLog.Warning($"Error getting target value from name: '{name}'. " +
                                  $"Edited value?\n{e}");
                return null;
            }
        }

        private enum StackOption
        {
            UserChosenHealStack,
            DefaultHealStack,
            CustomHealStack,
            RaiseStack,
        }

        #endregion
    }

    #endregion

    #region Core Targets

    public static IGameObject? Self =>
        Player.Available ? Player.Object : null;

    public static IGameObject? HardTarget =>
        Svc.Targets.Target;

    public static IGameObject? SoftTarget =>
        Svc.Targets.SoftTarget;

    public static IGameObject? SoftTargetIfMissingHP =>
        Svc.Targets.SoftTarget.IfMissingHP();

    public static IGameObject? FocusTarget =>
        Svc.Targets.FocusTarget;

    public static IGameObject? FocusTargetIfMissingHP =>
        Svc.Targets.FocusTarget.IfMissingHP();

    public static IGameObject? TargetsTarget =>
        Svc.Targets.Target is { TargetObjectId: not 0xE0000000 }
            ? Svc.Targets.Target.TargetObject
            : null;

    public static IGameObject? UIMouseOverTarget
    {
        get
        {
            if (!PronounService.PronounsReady) return null;
            unsafe
            {
                return GameObjectExtensions.GetObjectFrom(
                    PronounService.Module->UiMouseOverTarget);
            }
        }
    }

    public static IGameObject? ModelMouseOverTarget =>
        Svc.Targets.MouseOverNameplateTarget ?? Svc.Targets.MouseOverTarget;

    public static IGameObject? Chocobo =>
        Svc.Buddies.CompanionBuddy?.GameObject;

    public static IGameObject? AnyEnemy =>
        Svc.Objects
            .OfType<IBattleChara>()
            .FirstOrDefault(x => x.IsHostile() && x.IsTargetable &&
                                 x.IsWithinRange());

    #region Enemies

    public static IGameObject? LowestHPEnemy =>
        Svc.Objects
            .OfType<IBattleChara>()
            .Where(x => x.IsHostile() && x.IsTargetable && x.IsWithinRange())
            .OrderBy(x => x.CurrentHp)
            .FirstOrDefault();

    public static IGameObject? LowestHPEnemyIfNotInvuln =>
        Svc.Objects
            .OfType<IBattleChara>()
            .Where(x => x.IsHostile() && x.IsTargetable &&
                        x.IsWithinRange() && x.IsNotInvincible())
            .OrderBy(x => x.CurrentHp)
            .FirstOrDefault();

    public static IGameObject? LowestHPPEnemy =>
        Svc.Objects
            .OfType<IBattleChara>()
            .Where(x => x.IsHostile() && x.IsTargetable && x.IsWithinRange())
            .OrderBy(x => x.CurrentHp / x.MaxHp * 100)
            .FirstOrDefault();

    public static IGameObject? LowestHPPEnemyIfNotInvuln =>
        Svc.Objects
            .OfType<IBattleChara>()
            .Where(x => x.IsHostile() && x.IsTargetable &&
                        x.IsWithinRange() && x.IsNotInvincible())
            .OrderBy(x => x.CurrentHp / x.MaxHp * 100)
            .FirstOrDefault();

    public static IGameObject? InterruptableEnemy =>
        Svc.Objects
            .OfType<IBattleChara>()
            .Where(x => x.IsHostile() && x.IsTargetable &&
                        x.IsWithinRange(3) && x.IsCastInterruptible)
            .OrderByDescending(x => Svc.Targets.Target?.GameObjectId == x.GameObjectId)
            .FirstOrDefault();

    public static IGameObject? StunnableEnemy(int restunCheck = 3) =>
        Svc.Objects
            .OfType<IBattleChara>()
            .Where(x => x.IsHostile() && x.IsTargetable &&
                        !x.IsBoss() && x.IsWithinRange(3) && 
                        !CustomComboFunctions.HasStatusEffect(All.Debuffs.Stun, x) && 
                           (ICDTracker.StatusIsExpired(All.Debuffs.Stun, x.GameObjectId) || ICDTracker.Trackers.FirstOrDefault(y => y.StatusID == All.Debuffs.Stun && x.GameObjectId == y.GameObjectId)?.TimesApplied < restunCheck))
            .OrderByDescending(x => Svc.Targets.Target?.GameObjectId == x.GameObjectId)
            .FirstOrDefault();

    #endregion

    #region Previous Targets

    public static IGameObject? LastHardTarget =>
        PronounService.GetIGameObjectFromPronounID(1006);

    public static IGameObject? LastHostileHardTarget =>
        PronounService.GetIGameObjectFromPronounID(1084);

    public static IGameObject? MostRecentAttacker =>
        PronounService.GetIGameObjectFromPronounID(1008);

    #endregion

    #endregion

    #region Party Targets

    public static IGameObject? KardionTarget =>
        Svc.Objects
            .OfType<IBattleChara>()
            .FirstOrDefault(x =>
                CustomComboFunctions.HasStatusEffect(SGE.Buffs.Kardion, x));

    public static IGameObject? AnyDeadPartyMember =>
        CustomComboFunctions
            .GetPartyMembers()
            .Select(x => x.BattleChara)
            .FirstOrDefault(x => x?.IsDead() == true);

    public static IGameObject? AnyDeadNonPartyMember =>
        Svc.Objects
            .Where(x => x.IsAPlayer() && x.IsTargetable &&
                        !x.IsInParty())
            .FirstOrDefault(x => x.IsDead());

    #region HP-Based Targets

    public static IGameObject? LowestHPAlly =>
        CustomComboFunctions
            .GetPartyMembers()
            .Select(x => x.BattleChara)
            .Where(x => x.IsDead() == false)
            .OrderBy(x => x.CurrentHp)
            .FirstOrDefault();

    public static IGameObject? LowestHPAllyIfMissingHP =>
        LowestHPAlly?.IfMissingHP();

    public static IGameObject? LowestHPPAlly =>
        CustomComboFunctions
            .GetPartyMembers()
            .Select(x => x.BattleChara)
            .Where(x => x.IsDead() == false)
            .OrderBy(x => x.CurrentHp / x.MaxHp * 100)
            .FirstOrDefault();

    public static IGameObject? LowestHPPAllyIfMissingHP =>
        LowestHPPAlly?.IfMissingHP();

    #endregion

    #region Party Slots

    public static IGameObject? PartyMember1 => GetPartyMemberInSlotSlot(1);
    public static IGameObject? PartyMember2 => GetPartyMemberInSlotSlot(2);
    public static IGameObject? PartyMember3 => GetPartyMemberInSlotSlot(3);
    public static IGameObject? PartyMember4 => GetPartyMemberInSlotSlot(4);
    public static IGameObject? PartyMember5 => GetPartyMemberInSlotSlot(5);
    public static IGameObject? PartyMember6 => GetPartyMemberInSlotSlot(6);
    public static IGameObject? PartyMember7 => GetPartyMemberInSlotSlot(7);
    public static IGameObject? PartyMember8 => GetPartyMemberInSlotSlot(8);

    /// <summary>
    ///     Tries to get a party member, by slot number (1–8).
    /// </summary>
    /// <param name="slot">
    ///     The party slot (1 for local player, 2–8 for party members).
    /// </param>
    /// <returns>
    ///     An <see cref="IGameObject" /> for the party member if found;
    /// </returns>
    /// <remarks>IDs start at 44 and go to 51</remarks>
    public static IGameObject? GetPartyMemberInSlotSlot(int slot) =>
        slot switch
        {
            < 1 or > 8 => null,
            1 => Self,
            _ => PronounService.GetIGameObjectFromPronounID(42 + slot),
        };

    #endregion

    #endregion

    #region Role Targets (that are not the current player)

    /// Gets any Tank or Healer that is not the player.
    public static IGameObject? AnySupport =>
        CustomComboFunctions
            .GetPartyMembers()
            .Select(x => new { x.BattleChara, x.RealJob })
            .Where(x => x.BattleChara.IsNotThePlayer())
            .FirstOrDefault(x => (CombatRole)(x?.RealJob?.Role ?? -1) is
                CombatRole.Tank or CombatRole.Healer)?.BattleChara;

    /// Gets any living Tank or Healer that is not the player.
    public static IGameObject? AnyLivingSupport =>
        CustomComboFunctions
            .GetPartyMembers()
            .Select(x => new { x.BattleChara, x.RealJob })
            .Where(x => x.BattleChara.IsNotThePlayer() && !x.BattleChara.IsDead)
            .FirstOrDefault(x => (CombatRole)(x?.RealJob?.Role ?? -1) is
                CombatRole.Tank or CombatRole.Healer)?.BattleChara;

    /// Gets any DPS that is not the player.
    public static IGameObject? AnyDPS =>
        CustomComboFunctions
            .GetPartyMembers()
            .Select(x => new { x.BattleChara, x.RealJob })
            .Where(x => x.BattleChara.IsNotThePlayer())
            .FirstOrDefault(x => (CombatRole)(x?.RealJob?.Role ?? -1) is CombatRole.DPS)?.BattleChara;

    #region Slightly More Specific Roles

    /// Gets any Tank that is not the player.
    public static IGameObject? AnyTank =>
        CustomComboFunctions
            .GetPartyMembers()
            .Select(x => new { x.BattleChara, x.RealJob })
            .Where(x => x.BattleChara.IsNotThePlayer())
            .FirstOrDefault(x => (CombatRole)(x?.RealJob?.Role ?? -1) is CombatRole.Tank)?.BattleChara;

    /// Gets any living Tank that is not the player.
    public static IGameObject? AnyLivingTank =>
        CustomComboFunctions
            .GetPartyMembers()
            .Select(x => new { x.BattleChara, x.RealJob })
            .Where(x => x.BattleChara.IsNotThePlayer() && !x.BattleChara.IsDead)
            .FirstOrDefault(x => (CombatRole)(x?.RealJob?.Role ?? -1) is CombatRole.Tank)?.BattleChara;

    /// Gets any Healer that is not the player.
    public static IGameObject? AnyHealer =>
        CustomComboFunctions
            .GetPartyMembers()
            .Select(x => new { x.BattleChara, x.RealJob })
            .Where(x => x.BattleChara.IsNotThePlayer())
            .FirstOrDefault(x => (CombatRole)(x?.RealJob?.Role ?? -1) is CombatRole.Healer)?.BattleChara;

    /// Gets any living Healer that is not the player.
    public static IGameObject? AnyLivingHealer =>
        CustomComboFunctions
            .GetPartyMembers()
            .Select(x => new { x.BattleChara, x.RealJob })
            .Where(x => x.BattleChara.IsNotThePlayer() && !x.BattleChara.IsDead)
            .FirstOrDefault(x => (CombatRole)(x?.RealJob?.Role ?? -1) is CombatRole.Healer)?.BattleChara;

    /// Gets any Raiser (Healer or DPS) that is not the player.
    public static IGameObject? AnyRaiser =>
        CustomComboFunctions
            .GetPartyMembers()
            .Select(x => new { x.BattleChara, x.RealJob })
            .Where(x => x.BattleChara.IsNotThePlayer())
            .FirstOrDefault(x => (CombatRole)(x?.RealJob?.Role ?? -1) is CombatRole.Healer ||
                x?.RealJob?.RowId is SMN.JobID or RDM.JobID)?.BattleChara;

    /// Gets any Raiser DPS that is not the player.
    public static IGameObject? AnyRaiserDPS =>
        CustomComboFunctions
            .GetPartyMembers()
            .Select(x => new { x.BattleChara, x.RealJob })
            .Where(x => x.BattleChara.IsNotThePlayer())
            .FirstOrDefault(x => x?.RealJob?.RowId is SMN.JobID or RDM.JobID)?.BattleChara;

    /// Gets any Melee DPS that is not the player.
    public static IGameObject? AnyMeleeDPS =>
        CustomComboFunctions
            .GetPartyMembers()
            .Select(x => new { x.BattleChara, x.RealJob })
            .Where(x => x.BattleChara.IsNotThePlayer())
            .FirstOrDefault(x => x?.RealJob?.RowId.Role() is 2)?.BattleChara;

    /// Gets any Physical Ranged DPS that is not the player.
    public static IGameObject? AnyRangedDPS =>
        CustomComboFunctions
            .GetPartyMembers()
            .Select(x => new { x.BattleChara, x.RealJob })
            .Where(x => x.BattleChara.IsNotThePlayer())
            .FirstOrDefault(x => x?.RealJob?.RowId.Role() is 3)?.BattleChara;

    /// Gets any Magical DPS that is not the player.
    public static IGameObject? AnyPhysRangeDPS =>
        CustomComboFunctions
            .GetPartyMembers()
            .Select(x => new { x.BattleChara, x.RealJob })
            .Where(x => x.BattleChara.IsNotThePlayer())
            .FirstOrDefault(x =>
                RoleAttribute.GetRoleFromJob(x?.RealJob?.RowId ?? 0) is
                    JobRole.RangedDPS)?.BattleChara;

    /// Gets any Magical DPS that is not the player.
    public static IGameObject? AnyMagicalDPS =>
        CustomComboFunctions
            .GetPartyMembers()
            .Select(x => new { x.BattleChara, x.RealJob })
            .Where(x => x.BattleChara.IsNotThePlayer())
            .FirstOrDefault(x =>
                RoleAttribute.GetRoleFromJob(x?.RealJob?.RowId ?? 0) is
                    JobRole.MagicalDPS)?.BattleChara;

    #endregion

    #region Slightly More Specific Roles, with Additions

    /// Gets any Tank that is dead (but only if all tanks are dead).
    public static IGameObject? AnyDeadTankIfNoneAlive
    {
        get
        {
            var tanks = CustomComboFunctions
                .GetPartyMembers()
                .Select(x => new { x.BattleChara, x.RealJob })
                .Where(x => x.BattleChara.IsNotThePlayer() && (CombatRole)(x?.RealJob?.Role ?? -1) is CombatRole.Tank)
                .ToArray();
            var deadTanks =
                tanks.Where(x => x?.BattleChara.IsDead() == true).ToArray();

            if (deadTanks.Length == 0)
                return null;
            if (tanks.Any(x => x?.BattleChara.IsDead() == false))
                return null;

            return deadTanks.FirstOrDefault()?.BattleChara;
        }
    }

    /// Gets any Healer that is dead (but only if all healers are dead).
    public static IGameObject? AnyDeadHealerIfNoneAlive
    {
        get
        {
            var healers = CustomComboFunctions
                .GetPartyMembers()
                .Select(x => new { x.BattleChara, x.RealJob })
                .Where(x => x.BattleChara.IsNotThePlayer() && (CombatRole)(x?.RealJob?.Role ?? -1) is CombatRole.Healer)
                .ToArray();
            var deadHealers =
                healers.Where(x => x?.BattleChara.IsDead() == true).ToArray();

            if (deadHealers.Length == 0)
                return null;
            if (healers.Any(x => x?.BattleChara.IsDead() == false))
                return null;

            return deadHealers.FirstOrDefault()?.BattleChara;
        }
    }

    /// Gets any Raiser (Healer or DPS) that is dead (but only if all Raisers are dead).
    public static IGameObject? AnyDeadRaiserIfNoneAlive
    {
        get
        {
            var raisers = CustomComboFunctions
                .GetPartyMembers()
                .Select(x => new { x.BattleChara, x.RealJob })
                .Where(x => x.BattleChara.IsNotThePlayer() &&
                            ((CombatRole)(x?.RealJob?.Role ?? -1) is CombatRole.Healer ||
                             x?.RealJob?.RowId is SMN.JobID or RDM.JobID))
                .ToArray();
            var deadRaisers =
                raisers.Where(x => x?.BattleChara.IsDead() == true).ToArray();

            if (deadRaisers.Length == 0)
                return null;
            if (raisers.Any(x => x?.BattleChara.IsDead() == false))
                return null;

            return deadRaisers.FirstOrDefault()?.BattleChara;
        }
    }

    /// Gets any Raiser DPS that is dead (but only if all Raiser DPS are dead).
    public static IGameObject? AnyDeadRaiserDPSIfNoneAlive
    {
        get
        {
            var raisers = CustomComboFunctions
                .GetPartyMembers()
                .Select(x => new { x.BattleChara, x.RealJob })
                .Where(x => x.BattleChara.IsNotThePlayer() &&
                            x?.RealJob?.RowId is SMN.JobID or RDM.JobID)
                .ToArray();
            var deadRaisers =
                raisers.Where(x => x?.BattleChara.IsDead() == true).ToArray();

            if (deadRaisers.Length == 0)
                return null;
            if (raisers.Any(x => x?.BattleChara.IsDead() == false))
                return null;

            return deadRaisers.FirstOrDefault()?.BattleChara;
        }
    }

    #endregion

    #region More Specific Roles

    /// Gets any Pure Healer that is not the player.
    public static IGameObject? AnyPureHealer =>
        CustomComboFunctions
            .GetPartyMembers()
            .Select(x => new { x.BattleChara, x.RealJob })
            .Where(x => x.BattleChara.IsNotThePlayer())
            .FirstOrDefault(x =>
                x?.RealJob?.RowId is WHM.JobID or AST.JobID)?.BattleChara;

    /// Gets any Shield Healer that is not the player.
    public static IGameObject? AnyShieldHealer =>
CustomComboFunctions
            .GetPartyMembers()
            .Select(x => new { x.BattleChara, x.RealJob })
            .Where(x => x.BattleChara.IsNotThePlayer())
            .FirstOrDefault(x =>
                x?.RealJob?.RowId is SCH.JobID or SGE.JobID)?.BattleChara;

    /// Gets any Selfish DPS that is not the player.
    public static IGameObject? AnySelfishDPS =>
        CustomComboFunctions
            .GetPartyMembers()
            .Select(x => new { x.BattleChara, x.RealJob })
            .Where(x => x.BattleChara.IsNotThePlayer())
            .FirstOrDefault(x => x?.RealJob?.RowId is
                SAM.JobID or BLM.JobID or MCH.JobID or VPR.JobID)?.BattleChara;

    #endregion

    #endregion
}
