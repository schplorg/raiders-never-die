using Harmony;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Verse;
using Verse.AI;
using Verse.AI.Group;
using UnityEngine;

namespace RaidersNeverDie
{
    [StaticConstructorOnStartup]
    static class HarmonyPatches
    {
        static HarmonyPatches()
        {
            HarmonyInstance harmony = HarmonyInstance.Create("rimworld.schplorg.raidersneverdie");
            // CheckForStateChange Patch
            SuperPatch(harmony, typeof(Verse.Pawn_HealthTracker),"CheckForStateChange","CheckForStateChange_Prefix");
        }

        private static void SuperPatch(HarmonyInstance harmony, Type t, string a, string b){
            MethodInfo targetmethod = AccessTools.Method(t,a);
            HarmonyMethod prefixmethod = new HarmonyMethod(typeof(RaidersNeverDie.HarmonyPatches).GetMethod(b));
            harmony.Patch( targetmethod, prefixmethod, null );
        }

        public static bool CheckForStateChange_Prefix(Verse.Pawn_HealthTracker __instance, DamageInfo? dinfo, Hediff hediff)
        {
            var inst = __instance;
            var pawn = Traverse.Create(__instance).Field("pawn").GetValue<Pawn>();
            var forceIncap= Traverse.Create(__instance).Field("forceIncap").GetValue<bool>();
            var shouldBeDead = Traverse.Create(__instance).Method("ShouldBeDead").GetValue<bool>();
            var shouldBeDowned= Traverse.Create(__instance).Method("ShouldBeDowned").GetValue<bool>();
            if (inst.Dead)
            {
                return false;
            }
            if (shouldBeDead)
            {
                if (!pawn.Destroyed)
                {
                    pawn.Kill(dinfo, hediff);
                }
            }
            else if (!inst.Downed)
            {
                if (shouldBeDowned)
                {
                    if (!forceIncap && dinfo.HasValue && dinfo.Value.Def.ExternalViolenceFor(pawn) && !pawn.IsWildMan() && (pawn.Faction == null || !pawn.Faction.IsPlayer) && (pawn.HostFaction == null || !pawn.HostFaction.IsPlayer))
                    {
                        float chance = pawn.RaceProps.Animal ? 0.5f : ((!pawn.RaceProps.IsMechanoid) ? (HealthTuning.DeathOnDownedChance_NonColonyHumanlikeFromPopulationIntentCurve.Evaluate(StorytellerUtilityPopulation.PopulationIntent) * Find.Storyteller.difficulty.enemyDeathOnDownedChanceFactor) : 1f);
                        if (Rand.Chance(RNDSettings.raiderDeaths*chance))
                        {
                            pawn.Kill(dinfo);
                            return false;
                        }
                    }
                    forceIncap = false;
                    Traverse.Create(__instance).Method("MakeDowned",dinfo, hediff).GetValue();
                }
                else
                {
                    if (inst.capacities.CapableOf(PawnCapacityDefOf.Manipulation))
                    {
                        return false;
                    }
                    if (pawn.carryTracker != null && pawn.carryTracker.CarriedThing != null && pawn.jobs != null && pawn.CurJob != null)
                    {
                        pawn.jobs.EndCurrentJob(JobCondition.InterruptForced);
                    }
                    if (pawn.equipment != null && pawn.equipment.Primary != null)
                    {
                        if (pawn.kindDef.destroyGearOnDrop)
                        {
                            pawn.equipment.DestroyEquipment(pawn.equipment.Primary);
                        }
                        else if (pawn.InContainerEnclosed)
                        {
                            pawn.equipment.TryTransferEquipmentToContainer(pawn.equipment.Primary, pawn.holdingOwner);
                        }
                        else if (pawn.SpawnedOrAnyParentSpawned)
                        {
                            ThingWithComps tmp;
                            pawn.equipment.TryDropEquipment(pawn.equipment.Primary, out tmp, pawn.PositionHeld);
                        }
                        else
                        {
                            pawn.equipment.DestroyEquipment(pawn.equipment.Primary);
                        }
                    }
                }
            }
            else if (!shouldBeDowned)
            {
                Traverse.Create(__instance).Method("MakeUndowned").GetValue();
            }
            return false;
        }
    }
}