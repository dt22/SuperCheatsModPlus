﻿using Base.Core;
using Base.Defs;
using Base.Utils.Maths;
using com.ootii.Helpers;
using HarmonyLib;
using PhoenixPoint.Common.Core;
using PhoenixPoint.Common.Entities.GameTags;
using PhoenixPoint.Common.Entities.GameTagsSharedData;
using PhoenixPoint.Geoscape.Entities;
using PhoenixPoint.Geoscape.Events;
using PhoenixPoint.Geoscape.Levels;
using PhoenixPoint.Tactical.Entities;
using PhoenixPoint.Tactical.Entities.Abilities;
using PhoenixPoint.Tactical.Entities.Equipments;
using PhoenixPoint.Tactical.Entities.Weapons;
using PhoenixPoint.Tactical.Levels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using System.Collections;
using PhoenixPoint.Tactical.View.ViewControllers;
using PhoenixPoint.Tactical.View.ViewStates;
using UnityEngine.UI;
using PhoenixPoint.Geoscape.View.ViewControllers.Roster;
using PhoenixPoint.Geoscape.View.ViewStates;
using PhoenixPoint.Geoscape.View;
using PhoenixPoint.Geoscape.View.ViewModules;
using UnityEngine.EventSystems;
using Base.UI;
using PhoenixPoint.Common.Entities.Items;
using Base.UI.MessageBox;
using System.Reflection;
using Base.UI.VideoPlayback;
using PhoenixPoint.Home.View.ViewStates;
using PhoenixPoint.Common.Game;
using Base.Levels;

namespace SuperCheatsModPlus
{
    internal class AAPatches
    {
        internal static Dictionary<TacticalActor, int> returnFireCounter = new Dictionary<TacticalActor, int>();
        internal static KeyValuePair<bool, string> stepOutTracker = new KeyValuePair<bool, string>(false, "");
        internal static Color emptySlotDefaultColor = new Color32(0, 0, 0, 128);
        internal static string emptySlotDefaultText = "EMPTY";
        internal static string emptySlotScrapText = "SCRAP AIRCRAFT?";
        internal static MethodInfo ___UpdateResourceInfo = typeof(UIModuleInfoBar).GetMethod("UpdateResourceInfo", BindingFlags.NonPublic | BindingFlags.Instance);

        private class ContainerInfo
        {
            public ContainerInfo(string name, int index)
            {
                this.Name = name;
                this.Index = index;
            }
            public string Name { get; }
            public int Index { get; }
        }
        public static void Apply()
        {
            DefRepository defRepository = GameUtl.GameComponent<DefRepository>();
            SuperCheatsModPlusConfig Config = (SuperCheatsModPlusConfig)SuperCheatsModPlusMain.Main.Config;
            List<GameDifficultyLevelDef> gameDifficultyLevelDefDefs = defRepository.DefRepositoryDef.AllDefs.OfType<GameDifficultyLevelDef>().ToList();

            foreach (GameDifficultyLevelDef gdlDef in gameDifficultyLevelDefDefs)
            {
                gdlDef.RecruitsGenerationParams.HasArmor = Config.RecruitGenerationHasArmor;
                gdlDef.RecruitsGenerationParams.HasWeapons = Config.RecruitGenerationHasWeapons;
                gdlDef.RecruitsGenerationParams.HasConsumableItems = Config.RecruitGenerationHasConsumableItems;
                gdlDef.RecruitsGenerationParams.HasInventoryItems = Config.RecruitGenerationHasInventoryItems;
                gdlDef.RecruitsGenerationParams.CanHaveAugmentations = Config.RecruitGenerationCanHaveAugmentations;
            }

            foreach (GeoFactionDef gfDef in defRepository.DefRepositoryDef.AllDefs.OfType<GeoFactionDef>())
            {
                if (gfDef.name.Contains("Anu") || gfDef.name.Contains("NewJericho") || gfDef.name.Contains("Synedrion"))
                {
                    gfDef.RecruitIntervalCheckDays = Config.RecruitIntervalCheckDays;
                }
            }

            List<GeoVehicleDef> geoVehicleDefs = defRepository.DefRepositoryDef.AllDefs.OfType<GeoVehicleDef>().ToList();
            foreach (GeoVehicleDef gvDef in geoVehicleDefs)
            {
                if (gvDef.name.Contains("Blimp"))
                {
                    gvDef.BaseStats.Speed.Value = Config.AircraftBlimpSpeed;
                    gvDef.BaseStats.SpaceForUnits = Config.AircraftBlimpSpace;
                    gvDef.BaseStats.MaximumRange.Value = Config.AircraftBlimpRange;
                }
                else if (gvDef.name.Contains("Thunderbird"))
                {
                    gvDef.BaseStats.Speed.Value = Config.AircraftThunderbirdSpeed;
                    gvDef.BaseStats.SpaceForUnits = Config.AircraftThunderbirdSpace;
                    gvDef.BaseStats.MaximumRange.Value = Config.AircraftThunderbirdRange;
                }
                else if (gvDef.name.Contains("Manticore"))
                {
                    gvDef.BaseStats.Speed.Value = Config.AircraftManticoreSpeed;
                    gvDef.BaseStats.SpaceForUnits = Config.AircraftManticoreSpace;
                    gvDef.BaseStats.MaximumRange.Value = Config.AircraftManticoreRange;
                }
                else if (gvDef.name.Contains("Helios"))
                {
                    gvDef.BaseStats.Speed.Value = Config.AircraftHeliosSpeed;
                    gvDef.BaseStats.SpaceForUnits = Config.AircraftHeliosSpace;
                    gvDef.BaseStats.MaximumRange.Value = Config.AircraftHeliosRange;
                }
            }

            foreach (TacCharacterDef tcDef in defRepository.DefRepositoryDef.AllDefs.OfType<TacCharacterDef>().Where(d => d.IsVehicle || d.IsMutog))
            {
                if (tcDef.name.Contains("CharacterTemplateDef"))
                {
                    if (tcDef.name.Contains("Mutog"))
                    {
                        tcDef.Volume = Config.OccupyingSpaceMutog;
                    }
                    else if (tcDef.name.Contains("Armadillo"))
                    {
                        tcDef.Volume = Config.OccupyingSpaceArmadillo;
                    }
                    else if (tcDef.name.Contains("Scarab"))
                    {
                        tcDef.Volume = Config.OccupyingSpaceScarab;
                    }
                    else if (tcDef.name.Contains("Aspida"))
                    {
                        tcDef.Volume = Config.OccupyingSpaceAspida;
                    }
                }
            }
            List<string> turretsToChange = new List<string>() { "Armadillo_Gauss_Turret", "Scarab_Missile_Turret", "Aspida_Arms" };
            //AAPatches.Prefix_PhoenixGame_RunGameLevel();
        }
        private static bool CanReturnFireFromAngle(TacticalActor shooter, TacticalActorBase target, float reactionAngleCos)
        {
            // Turrets cannot be flanked
            if (target.IsMetallic)
            {
                return true;
            }

            if (reactionAngleCos > 0.99)
            {
                return true;
            }

            Vector3 targetForward = target.transform.TransformDirection(Vector3.forward);
            Vector3 targetToShooter = (shooter.Pos - target.Pos).normalized;
            float angleCos = Vector3.Dot(targetForward, targetToShooter);

            return Utl.GreaterThanOrEqualTo(angleCos, reactionAngleCos);
        }

        private static bool HasReachedReturnFireLimit(TacticalActor target)
        {
            SuperCheatsModPlusConfig Config = (SuperCheatsModPlusConfig)SuperCheatsModPlusMain.Main.Config;
            // Turrets have no limit
            if (target.IsMetallic)
            {
                return false;
            }

            int returnFireLimit = Config.ReturnFireLimit;
            if (returnFireLimit > 0)
            {
                returnFireCounter.TryGetValue(target, out var currentCount);
                return currentCount >= returnFireLimit;
            }
            else
            {
                return true;
            }
        }
        //public static bool Prefix_PhoenixGame_RunGameLevel(PhoenixGame __instance, LevelSceneBinding levelSceneBinding, ref IEnumerator<NextUpdate> __result)
        //{
        //    try
        //    {
        //        if (levelSceneBinding == __instance.Def.IntroLevelSceneDef.Binding)
        //        {
        //            __result = Enumerable.Empty<NextUpdate>().GetEnumerator();
        //            return false;
        //        }
        //        return true;
        //    }
        //    catch (Exception e)
        //    {
        //        return true;
        //    }
        //}
        //
        //public static void Postfix_UIStateHomeScreenCutscene_EnterState(UIStateHomeScreenCutscene __instance, VideoPlaybackSourceDef ____sourcePlaybackDef)
        //{
        //    try
        //    {
        //        if (____sourcePlaybackDef == null)
        //        {
        //            return;
        //        }
        //
        //        if (____sourcePlaybackDef.ResourcePath.Contains("Game_Intro_Cutscene"))
        //        {
        //            typeof(UIStateHomeScreenCutscene).GetMethod("OnCancel", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(__instance, null);
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //    }
        //}
        //
        //public static void Postfix_UIStateTacticalCutscene_EnterState(UIStateTacticalCutscene __instance, VideoPlaybackSourceDef ____sourcePlaybackDef)
        //{
        //    try
        //    {
        //        if (____sourcePlaybackDef == null)
        //        {
        //            return;
        //        }
        //
        //
        //        if (____sourcePlaybackDef.ResourcePath.Contains("LandingSequences"))
        //        {
        //            typeof(UIStateTacticalCutscene).GetMethod("OnCancel", BindingFlags.NonPublic | BindingFlags.Instance)?.Invoke(__instance, null);
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //    }
        //}


        [HarmonyPatch(typeof(GeoscapeEventSystem), "PhoenixFaction_OnSiteFirstTimeVisited")]
    public static class GeoscapeEventSystem_PhoenixFaction_OnSiteFirstTimeVisited_Patch
    {

        public static bool Prepare()
        {
            SuperCheatsModPlusConfig Config = (SuperCheatsModPlusConfig)SuperCheatsModPlusMain.Main.Config;
            return Config.DisableAmbushes;
        }

        public static void Prefix(GeoscapeEventSystem __instance, GeoSite site, ref int ____ambushProtection)
        {
            SuperCheatsModPlusConfig Config = (SuperCheatsModPlusConfig)SuperCheatsModPlusMain.Main.Config;
            try
            {            

                if (site == null)
                {
                    return;
                }

                if (Config.RetainAmbushesInsideMist && site.IsInMist)
                {
                    return;
                }

                // This gets subtracted by one and then checked to be zero or below in original method...
                // Resetting it to two effectively disables ambushes.
                if (____ambushProtection < 2)
                {
                    ____ambushProtection = 2;
                }
            }
            catch (Exception e)
            {              
            }
        }
    }
    [HarmonyPatch(typeof(DieAbility), "ShouldDestroyItem")]
    public static class DieAbility_ShouldDestroyItem_Patch
    {      

        public static void Prefix(DieAbility __instance, TacticalItem item)
        {
            SuperCheatsModPlusConfig Config = (SuperCheatsModPlusConfig)SuperCheatsModPlusMain.Main.Config;
            try
            {
                if (item.TacticalItemDef is WeaponDef wDef)
                {
                    if (!Config.AllowWeaponDrops)
                    {
                        return;
                    }
                    else
                    {

                        item.TacticalItemDef.DestroyOnActorDeathPerc = Config.FlatWeaponDestructionChance;
                        
                    }
                }
                else
                {
                    item.TacticalItemDef.DestroyOnActorDeathPerc = Config.ItemDestructionChance;
                }

                // If items should NEVER get destroyed by chance, modifiy result to false and skip original method
                /*
                if (Config.DestroyOnActorDeathPercent <= 0)
                {
                    __result = false;
                    return false;
                }
                */
            }
            catch (Exception e)
            {
            }
        }

        public static void Postfix(DieAbility __instance, bool __result, TacticalItem item)
        {
            string result = __result ? "destroyed" : "dropped";
        }
    }



    // Drop armor too
    [HarmonyPatch(typeof(DieAbility), "DropItems")]
    public static class DieAbility_DropItems_Patch
    {
        public static bool Prepare()
        {
            SuperCheatsModPlusConfig Config = (SuperCheatsModPlusConfig)SuperCheatsModPlusMain.Main.Config;
            return Config.AllowArmorDrops;
        }

        public static void Postfix(DieAbility __instance)
        {
            SuperCheatsModPlusConfig Config = (SuperCheatsModPlusConfig)SuperCheatsModPlusMain.Main.Config;
            try
            {
                TacticalActor actor = __instance.TacticalActor;

                if (actor.DisplayName.Contains("decoy", StringComparison.OrdinalIgnoreCase) || __instance.AbilityDef.name.Contains("decoy", StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }

                IEnumerable<TacticalItem> items = actor?.BodyState?.GetArmourItems();
                if (items?.Any() != true)
                {
                    return;
                }

                SharedData sharedData = SharedData.GetSharedDataFromGame();
                SharedGameTagsDataDef sharedGameTags = sharedData.SharedGameTags;
                GameTagDef armor = sharedGameTags.ArmorTag, manufacturable = sharedGameTags.ManufacturableTag, mounted = sharedGameTags.MountedTag;

                int count = 0;
                foreach (TacticalItem item in items)
                {
                    TacticalItemDef def = item.TacticalItemDef;
                    GameTagsList tags = def?.Tags;
                    if (tags == null || tags.Count == 0 || !tags.Contains(manufacturable) || def.IsPermanentAugment)
                    {
                        continue;
                    }
                    if (tags.Contains(armor) || tags.Contains(mounted))
                    {
                        int randomPercent = new System.Random().Next(0, 101);
                        bool willDrop = randomPercent > Config.FlatArmorDestructionChance;
                        if (willDrop)
                        {
                            item.Drop(sharedData.FallDownItemContainerDef, actor);
                            count++;
                        }
                    }
                }

                if (count > 0)
                {
                }
            }
            catch (Exception e)
            {
            }
        }
    }

    // Prevent dupes from squad member deaths
    [HarmonyPatch(typeof(GeoMission), "GetDeadSquadMembersArmour")]
    public static class GeoMission_GetDeadSquadMembersArmour_Patch
    {
        public static bool Prepare()
        {
            SuperCheatsModPlusConfig Config = (SuperCheatsModPlusConfig)SuperCheatsModPlusMain.Main.Config;
            return Config.AllowArmorDrops;
        }

        // Override!
        public static bool Prefix(GeoMission __instance, ref IEnumerable<GeoItem> __result)
        {
            try
            {
                __result = Enumerable.Empty<GeoItem>();

                return false;
            }
            catch (Exception e)
            {
                return true;
            }
        }
    }
    [HarmonyPatch(typeof(TacticalFaction), "PlayTurnCrt")]
    public static class TacticalFaction_PlayTurnCrt_Patch
    {
        internal static Dictionary<TacticalActor, int> returnFireCounter = new Dictionary<TacticalActor, int>();      
        public static void Prefix(TacticalFaction __instance)
        {
            SuperCheatsModPlusConfig Config = (SuperCheatsModPlusConfig)SuperCheatsModPlusMain.Main.Config;
            float reactionAngleCos = (float)Math.Cos(Config.ReturnFireAngle * Math.PI / 180d / 2d);

            try
            {
                // Only track enemies of the faction starting its turn
                returnFireCounter = returnFireCounter.Where(tacticalActor => tacticalActor.Key.TacticalFaction != __instance).ToDictionary(tacticalActor => tacticalActor.Key, tacticalActor => tacticalActor.Value);
            }
            catch (Exception e)
            {
            }
        }
    }



    [HarmonyPatch(typeof(TacticalLevelController), "FireWeaponAtTargetCrt")]
    public static class TacticalLevelController_FireWeaponAtTargetCrt_Patch
    {
        internal static Dictionary<TacticalActor, int> returnFireCounter = new Dictionary<TacticalActor, int>();
        internal static KeyValuePair<bool, string> stepOutTracker = new KeyValuePair<bool, string>(false, "");

        public static void Prefix(TacticalLevelController __instance, Weapon weapon, TacticalAbilityTarget abilityTarget)
        {
            SuperCheatsModPlusConfig Config = (SuperCheatsModPlusConfig)SuperCheatsModPlusMain.Main.Config;
            try
            {
                TacticalActor tacticalActor = weapon.TacticalActor;

                if (abilityTarget.AttackType == AttackType.ReturnFire)
                {
                    returnFireCounter.TryGetValue(tacticalActor, out var currentCount);
                    returnFireCounter[tacticalActor] = currentCount + 1;
                }

                if (Config.NoReturnFireWhenSteppingOut && abilityTarget.AttackType == AttackType.Regular)
                {
                    bool shooterStepsOut = Vector3.SqrMagnitude(tacticalActor.Pos - abilityTarget.ShootFromPos) > 0.01f;
                    if (shooterStepsOut)
                    {
                        string msg = $"{tacticalActor.DisplayName} stepped out to shoot with {weapon.DisplayName}.";
                        stepOutTracker = new KeyValuePair<bool, string>(true, msg);
                    }
                    else
                    {
                        stepOutTracker = new KeyValuePair<bool, string>(false, "");
                    }
                }
            }
            catch (Exception e)
            {
            }
        }
    }



    [HarmonyPatch(typeof(UIStateShoot), "CalculateReturnFirePredictions")]
    public static class UIStateShoot_CalculateReturnFirePredictions_Patch
    {
        internal static KeyValuePair<bool, string> stepOutTracker = new KeyValuePair<bool, string>(false, "");
        public static bool Prepare()
        {
            SuperCheatsModPlusConfig Config = (SuperCheatsModPlusConfig)SuperCheatsModPlusMain.Main.Config;
            return Config.NoReturnFireWhenSteppingOut;
        }

        public static void Prefix(UIStateShoot __instance)
        {
            try
            {
                ShootAbility ___shootAbility = (ShootAbility)AccessTools.Property(typeof(UIStateShoot), "_shootAbility").GetValue(__instance, null);

                if (__instance.AbilityTarget == null || ___shootAbility.Weapon == null)
                {
                    return;
                }

                TacticalActor tacticalActor = ___shootAbility.TacticalActor;
                TacticalAbilityTarget abilityTarget = __instance.AbilityTarget;

                if (abilityTarget.AttackType == AttackType.Regular)
                {
                    bool shooterWillStepOut = Vector3.SqrMagnitude(tacticalActor.Pos - abilityTarget.ShootFromPos) > 0.01f;
                    if (shooterWillStepOut)
                    {
                        string msg = $"{tacticalActor.DisplayName} will step out to shoot with {___shootAbility.Weapon.DisplayName}.";
                        stepOutTracker = new KeyValuePair<bool, string>(true, msg);
                    }
                    else
                    {
                        stepOutTracker = new KeyValuePair<bool, string>(false, "");
                    }
                }
            }
            catch (Exception e)
            {
            }
        }
    }



    [HarmonyPatch(typeof(UIStateAbilitySelected), "CalculateReturnFirePredictions")]
    public static class UIStateAbilitySelected_CalculateReturnFirePredictions_Patch
    {
        internal static KeyValuePair<bool, string> stepOutTracker = new KeyValuePair<bool, string>(false, "");
        public static bool Prepare()
        {
            SuperCheatsModPlusConfig Config = (SuperCheatsModPlusConfig)SuperCheatsModPlusMain.Main.Config;
            return Config.NoReturnFireWhenSteppingOut;
        }

        public static void Prefix(UIStateAbilitySelected __instance, List<TacticalActorBase> ____targetActors, TacticalAbility ____selectedAbility, bool ____groundTargeting)
        {
            SuperCheatsModPlusConfig Config = (SuperCheatsModPlusConfig)SuperCheatsModPlusMain.Main.Config;
            try
            {
                if (!____targetActors.Any<TacticalActorBase>() || __instance.SelectedAbilityTarget == null || !(____selectedAbility is IAttackAbility))
                {
                    return;
                }

                if (____groundTargeting)
                {
                    //return;
                }

                TacticalActor tacticalActor = ____selectedAbility.TacticalActor;
                TacticalAbilityTarget abilityTarget = __instance.SelectedAbilityTarget;

                if (abilityTarget.AttackType == AttackType.Regular)
                {
                    bool performerWillStepOut = Vector3.SqrMagnitude(tacticalActor.Pos - abilityTarget.ShootFromPos) > 0.01f;
                    if (performerWillStepOut)
                    {
                        string msg = $"{tacticalActor.DisplayName} will step out to use {____selectedAbility.TacticalAbilityDef?.ViewElementDef?.DisplayName1.Localize()} ({____selectedAbility.TargetEquipmentName}).";
                        stepOutTracker = new KeyValuePair<bool, string>(true, msg);
                    }
                    else
                    {
                        stepOutTracker = new KeyValuePair<bool, string>(false, "");
                    }
                }
            }
            catch (Exception e)
            {
            }
        }
    }



        [HarmonyPatch(typeof(TacticalLevelController), "GetReturnFireAbilities")]
        public static class TacticalLevelController_GetReturnFireAbilities_Patch
        {
            internal static Dictionary<TacticalActor, int> returnFireCounter = new Dictionary<TacticalActor, int>();
            internal static KeyValuePair<bool, string> stepOutTracker = new KeyValuePair<bool, string>(false, "");
            public static void Postfix(TacticalLevelController __instance, ref List<ReturnFireAbility> __result, TacticalActor shooter, Weapon weapon, TacticalAbilityTarget target)
            {
                SuperCheatsModPlusConfig Config = (SuperCheatsModPlusConfig)SuperCheatsModPlusMain.Main.Config;
                float reactionAngleCos = (float)Math.Cos(Config.ReturnFireAngle * Math.PI / 180d / 2d);
                try
                {
                    // No potential return fire from original method
                    if (__result == null || __result.Count == 0)
                    {
                        return;
                    }

                    // Double check
                    WeaponDef weaponDef = weapon?.WeaponDef;
                    if (target.AttackType == AttackType.ReturnFire || target.AttackType == AttackType.Overwatch || target.AttackType == AttackType.Synced || target.AttackType == AttackType.ZoneControl || (weaponDef != null && weaponDef.NoReturnFireFromTargets))
                    {
                        return;
                    }

                    List<ReturnFireAbility> returnFireAbilities = __result;
                    for (int i = returnFireAbilities.Count - 1; i >= 0; i--) // Reverse iteration to be able to modify list directly
                    {
                        TacticalActor tacticalActor = returnFireAbilities[i].TacticalActor;
                        // Check for step out
                        if (Config.NoReturnFireWhenSteppingOut && stepOutTracker.Key == true)
                        {
                            returnFireAbilities.RemoveAt(i);
                            continue;
                        }

                        // Check flanked
                        if (!CanReturnFireFromAngle(shooter, tacticalActor, reactionAngleCos))
                        {
                            returnFireAbilities.RemoveAt(i);
                            continue;
                        }

                        // Check shot limit
                        if (HasReachedReturnFireLimit(tacticalActor))
                        {
                            returnFireAbilities.RemoveAt(i);
                            continue;
                        }
                    }
                }

                catch (Exception e)
                {
                }
            }
        }



    [HarmonyPatch(typeof(SpottedTargetsElement), "ShowReturnFireIcon")]
    public static class SpottedTargetsElement_ShowReturnFireIcon_Patch
    {
        private static List<SpottedTargetsElement> AlreadyAdjustedElements = new List<SpottedTargetsElement>();

        private static Color lerpedColor = Color.white;
        private static float t = 0f;
        private static bool up = true;
        private static readonly float step = 0.03f;

        public static bool Prepare()
        {
                SuperCheatsModPlusConfig Config = (SuperCheatsModPlusConfig)SuperCheatsModPlusMain.Main.Config;
                return Config.EmphasizeReturnFireHint;
        }

        private static IEnumerator Pulse(Image i, Color c1, Color c2)
        {
            while (true)
            {
                if (up && t < 1f)
                {
                    t += step;
                }
                else if (up && t >= 1f)
                {
                    t -= step;
                    up = false;
                }
                else if (t > 0)
                {
                    t -= step;
                }
                else if (t <= 0)
                {
                    t += step;
                    up = true;
                }

                lerpedColor = Color.Lerp(c1, c2, t);
                i.color = lerpedColor;

                yield return new WaitForSeconds(1 / 30);
            }
        }

        public static void Prefix(SpottedTargetsElement __instance)
        {
            try
            {
                if (!AlreadyAdjustedElements.Contains(__instance))
                {
                    __instance.ReturnFire.transform.localScale = new Vector3(1.7f, 1.7f, 1.7f);
                    __instance.ReturnFire.transform.Translate(new Vector3(-2f, 3f, 1f));
                    AlreadyAdjustedElements.Add(__instance);
                }

                //__instance.StopCoroutine("Pulse");
                __instance.StopAllCoroutines();
                __instance.StartCoroutine(Pulse(__instance.ReturnFire, Color.white, Color.red));
            }
            catch (Exception e)
            {
            }
        }
    
        }
        //[HarmonyPatch(typeof(GeoRosterContainterItem), "Init")]
        //public static class GeoRosterContainterItem_Init_Patch
        //{
        //    public static bool Prepare()
        //    {
        //        SuperCheatsModPlusConfig Config = (SuperCheatsModPlusConfig)SuperCheatsModPlusMain.Main.Config;
        //        return Config.EnableScrapAircraft;
        //    }
        //
        //    public static void Prefix(GeoRosterContainterItem __instance)
        //    {
        //        try
        //        {
        //            // Store empty slot color and text to reset to on mouse exit/refresh list
        //            Text emptySlotText = __instance.EmptySlot.GetComponentInChildren<Text>(true);
        //            emptySlotDefaultColor = emptySlotText.color;
        //            emptySlotDefaultText = emptySlotText.text;
        //        }
        //        catch (Exception e)
        //        {
        //        }
        //    }
        //}
        //
        //
        //
        //[HarmonyPatch(typeof(GeoRosterContainterItem), "Refresh")]
        //public static class GeoRosterContainterItem_Refresh_Patch
        //{
        //    public static bool Prepare()
        //    {
        //        SuperCheatsModPlusConfig Config = (SuperCheatsModPlusConfig)SuperCheatsModPlusMain.Main.Config;
        //        return Config.EnableScrapAircraft;
        //    }
        //
        //    public static void Postfix(GeoRosterContainterItem __instance)
        //    {
        //        try
        //        {
        //            // Empty
        //            if (__instance.Container.MaxCharacterSpace > 0 && __instance.Container.CurrentOccupiedSpace == 0)
        //            {
        //                Text emptySlotText = __instance.EmptySlot.GetComponentInChildren<Text>(true);
        //                //Logger.Info($"[GeoRosterContainterItem_Refresh_POSTFIX] emptySlotText: {emptySlotText.text}");
        //
        //                // Aircraft 
        //                if (__instance.Container.MaxCharacterSpace != 2147483647)
        //                {
        //                    emptySlotText.text = emptySlotScrapText;
        //                }
        //                // Base
        //                else
        //                {
        //                    emptySlotText.text = emptySlotDefaultText;
        //                }
        //                //Logger.Info($"[GeoRosterContainterItem_Refresh_POSTFIX] emptySlotText: {emptySlotText.text}");
        //            }
        //        }
        //        catch (Exception e)
        //        {
        //        }
        //    }
        //}
        //
        //
        //
        //[HarmonyPatch(typeof(UIStateGeoRoster), "EnterState")]
        //public static class UIStateGeoRoster_EnterState_Patch
        //{
        //    public static bool Prepare()
        //    {
        //        SuperCheatsModPlusConfig Config = (SuperCheatsModPlusConfig)SuperCheatsModPlusMain.Main.Config;
        //        return Config.EnableScrapAircraft;
        //    }
        //
        //    public static void Postfix(UIStateGeoRoster __instance, List<IGeoCharacterContainer> ____characterContainers, GeoRosterFilterMode ____preferableFilterMode)
        //    {
        //        try
        //        {
        //            GeoscapeViewContext ___Context = (GeoscapeViewContext)AccessTools.Property(typeof(GeoscapeViewState), "Context").GetValue(__instance, null);
        //            UIModuleGeneralPersonelRoster ____geoRosterModule = (UIModuleGeneralPersonelRoster)AccessTools.Property(typeof(UIStateGeoRoster), "_geoRosterModule").GetValue(__instance, null);
        //
        //            RefreshScrapTriggers();
        //
        //
        //
        //            // Scoped functions
        //            void RefreshScrapTriggers()
        //            {
        //                for (int i = 0; i < ____geoRosterModule.Groups.Count; i++)
        //                {
        //                    GeoRosterContainterItem c = ____geoRosterModule.Groups[i];
        //
        //                    if (!c.EmptySlot.GetComponent<EventTrigger>())
        //                    {
        //                        c.EmptySlot.AddComponent<EventTrigger>();
        //                    }
        //
        //                    EventTrigger eventTrigger = c.EmptySlot.GetComponent<EventTrigger>();
        //                    eventTrigger.triggers.Clear();
        //
        //                    c.Refresh();
        //
        //                    if (c.Container.MaxCharacterSpace != 2147483647) // !Bases
        //                    {
        //                        Text emptySlotText = c.EmptySlot.GetComponentInChildren<Text>(true);
        //                        ContainerInfo containerInfo = new ContainerInfo(c.Container.Name, i);
        //
        //                        EventTrigger.Entry mouseenter = new EventTrigger.Entry();
        //                        mouseenter.eventID = EventTriggerType.PointerEnter;
        //                        mouseenter.callback.AddListener((eventData) => { OnScrapAircraftMouseEnter(emptySlotText); });
        //                        eventTrigger.triggers.Add(mouseenter);
        //
        //                        EventTrigger.Entry mouseexit = new EventTrigger.Entry();
        //                        mouseexit.eventID = EventTriggerType.PointerExit;
        //                        mouseexit.callback.AddListener((eventData) => { OnScrapAircraftMouseExit(emptySlotText); });
        //                        eventTrigger.triggers.Add(mouseexit);
        //
        //                        EventTrigger.Entry click = new EventTrigger.Entry();
        //                        click.eventID = EventTriggerType.PointerClick;
        //                        click.callback.AddListener((eventData) => { OnScrapAircraftClick(containerInfo); });
        //                        eventTrigger.triggers.Add(click);
        //                    }
        //                }
        //            }
        //
        //            void OnScrapAircraftMouseEnter(Text t)
        //            {
        //                t.color = Color.red;
        //            }
        //
        //            void OnScrapAircraftMouseExit(Text t)
        //            {
        //                t.color = emptySlotDefaultColor;
        //            }
        //
        //            void OnScrapAircraftClick(ContainerInfo containerInfo)
        //            {
        //                string aircraftIdentifier = containerInfo.Name;
        //                int groupIndex = containerInfo.Index;
        //                GeoFaction owningFaction = ___Context.ViewerFaction;
        //                GeoVehicle aircraftToScrap = owningFaction.Vehicles.Where(v => v.Name == aircraftIdentifier).FirstOrDefault();
        //                GeoscapeModulesData ____geoscapeModules = (GeoscapeModulesData)AccessTools.Property(typeof(GeoscapeViewState), "_geoscapeModules").GetValue(__instance, null);
        //                UIModuleGeoscapeScreenUtils ____utilsModule = ____geoscapeModules.GeoscapeScreenUtilsModule;
        //                string messageBoxText = ____utilsModule.DismissVehiclePrompt.Localize(null);
        //                VehicleItemDef aircraftItemDef = GameUtl.GameComponent<DefRepository>().GetAllDefs<VehicleItemDef>().Where(viDef => viDef.ComponentSetDef.Components.Contains(aircraftToScrap.VehicleDef)).FirstOrDefault();
        //
        //                if (aircraftItemDef != null && !aircraftItemDef.ScrapPrice.IsEmpty)
        //                {
        //                    messageBoxText = messageBoxText + "\n" + ____utilsModule.ScrapResourcesBack.Localize(null) + "\n \n";
        //                    foreach (ResourceUnit resourceUnit in ((IEnumerable<ResourceUnit>)aircraftItemDef.ScrapPrice))
        //                    {
        //                        if (resourceUnit.RoundedValue > 0)
        //                        {
        //                            string resourcesInfo = "";
        //                            ResourceType type = resourceUnit.Type;
        //                            switch (type)
        //                            {
        //                                case ResourceType.Supplies:
        //                                    resourcesInfo = ____utilsModule.ScrapSuppliesResources.Localize(null);
        //                                    break;
        //                                case ResourceType.Materials:
        //                                    resourcesInfo = ____utilsModule.ScrapMaterialsResources.Localize(null);
        //                                    break;
        //                                case (ResourceType)3:
        //                                    break;
        //                                case ResourceType.Tech:
        //                                    resourcesInfo = ____utilsModule.ScrapTechResources.Localize(null);
        //                                    break;
        //                                default:
        //                                    if (type == ResourceType.Mutagen)
        //                                    {
        //                                        resourcesInfo = ____utilsModule.ScrapMutagenResources.Localize(null);
        //                                    }
        //                                    break;
        //                            }
        //                            resourcesInfo = resourcesInfo.Replace("{0}", resourceUnit.RoundedValue.ToString());
        //                            messageBoxText += resourcesInfo;
        //                        }
        //                    }
        //                }
        //
        //                // Safety check as the game's UI fails hard if there's NO GeoVehicle left at all
        //                if (owningFaction.Vehicles.Count() <= 1)
        //                {
        //                    GameUtl.GetMessageBox().ShowSimplePrompt("This is Phoenix Point's last aircraft available", MessageBoxIcon.Error, MessageBoxButtons.OK, new MessageBox.MessageBoxCallback(OnScrapAircraftImpossibleCallback), null, null);
        //                }
        //                else
        //                {
        //                    GameUtl.GetMessageBox().ShowSimplePrompt(string.Format(messageBoxText, aircraftIdentifier), MessageBoxIcon.Warning, MessageBoxButtons.YesNo, new MessageBox.MessageBoxCallback(OnScrapAircraftCallback), null, containerInfo);
        //                }
        //            }
        //
        //            void OnScrapAircraftImpossibleCallback(MessageBoxCallbackResult msgResult)
        //            {
        //                // Nothing
        //            }
        //
        //            void OnScrapAircraftCallback(MessageBoxCallbackResult msgResult)
        //            {
        //                if (msgResult.DialogResult == MessageBoxResult.Yes)
        //                {
        //                    ContainerInfo containerInfo = msgResult.UserData as ContainerInfo;
        //
        //                    string aircraftIdentifier = containerInfo.Name;
        //                    int groupIndex = containerInfo.Index;
        //                    GeoFaction owningFaction = ___Context.ViewerFaction;
        //                    GeoVehicle aircraftToScrap = owningFaction.Vehicles.Where(v => v.Name == aircraftIdentifier).FirstOrDefault();
        //
        //                    if (aircraftToScrap != null)
        //                    {
        //                        // Unset vehicle.CurrentSite and trigger site.VehicleLeft
        //                        aircraftToScrap.Travelling = true;
        //
        //                        // Away with it!
        //                        aircraftToScrap.Destroy();
        //
        //                        // Add resources
        //                        VehicleItemDef aircraftItemDef = GameUtl.GameComponent<DefRepository>().GetAllDefs<VehicleItemDef>().Where(viDef => viDef.ComponentSetDef.Components.Contains(aircraftToScrap.VehicleDef)).FirstOrDefault();
        //                        if (aircraftItemDef != null && !aircraftItemDef.ScrapPrice.IsEmpty)
        //                        {
        //                            ___Context.Level.PhoenixFaction.Wallet.Give(aircraftItemDef.ScrapPrice, OperationReason.Scrap);
        //
        //                            GeoscapeModulesData ____geoscapeModules = (GeoscapeModulesData)AccessTools.Property(typeof(GeoscapeViewState), "_geoscapeModules").GetValue(__instance, null);
        //
        //                            //MethodInfo ___UpdateResourceInfo = typeof(UIModuleInfoBar).GetMethod("UpdateResourceInfo", BindingFlags.NonPublic | BindingFlags.Instance);
        //                            ___UpdateResourceInfo.Invoke(____geoscapeModules.ResourcesModule, new object[] { owningFaction, true });
        //                        }
        //
        //                        // Clean roster from aircraft container
        //                        ____characterContainers.RemoveAt(groupIndex);
        //
        //                        // Reset roster list
        //                        ____geoRosterModule.Init(___Context, ____characterContainers, null, ____preferableFilterMode, RosterSelectionMode.SingleSelect);
        //
        //                        // Reapply events to the correct slots
        //                        RefreshScrapTriggers();
        //                    }
        //                    else
        //                    {
        //                    }
        //                }
        //            }
        //        }
        //        catch (Exception e)
        //        {
        //        }
        //    }
        //}
    }
}

