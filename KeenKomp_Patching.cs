using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Linq;
using System.Text;
using Harmony;
using System.Reflection;
using Verse;
using Verse.AI;
using RimWorld;
using UnityEngine;

namespace KeenKomponents
{
	[StaticConstructorOnStartup]
	internal static class KeenKomp_Patching
	{
		static KeenKomp_Patching()
		{
			HarmonyInstance KeenKompPatch = HarmonyInstance.Create("com.KeenKomp");

			MethodInfo methInfIsBrokenDown = AccessTools.Method(typeof(BreakdownableUtility), "IsBrokenDown", null, null);
			MethodInfo methInfMakeNewToils = AccessTools.Method(typeof(JobDriver_FixBrokenDownBuilding), "MakeNewToils", null, null);

			HarmonyMethod harmonyMethodPreFIsBrokenDown = new HarmonyMethod(typeof(KeenKomp_Patching).GetMethod("PreFIsBrokenDown"));
			HarmonyMethod harmonyMethodPreFMakeNewToils = new HarmonyMethod(typeof(KeenKomp_Patching).GetMethod("PreFMakeNewToils"));

			KeenKompPatch.Patch(methInfIsBrokenDown, harmonyMethodPreFIsBrokenDown, null, null);
			KeenKompPatch.Patch(methInfMakeNewToils, harmonyMethodPreFMakeNewToils, null, null);

			Log.Message("KeenKompPatch initialized.");
		}

		public static bool PreFIsBrokenDown(this Thing t, ref bool __result)
		{
			CompKeenKomponentBreakdownable compBreakdownable = t.TryGetComp<CompKeenKomponentBreakdownable>();
			__result = compBreakdownable != null && compBreakdownable.IsBroke;
			return false;
		}

		public static bool PreFMakeNewToils(ref IEnumerable<Toil> __result, JobDriver_FixBrokenDownBuilding __instance)
		{
			__result = MakeNewToils(__instance);
			return false;
		}

		static IEnumerable<Toil> MakeNewToils(JobDriver_FixBrokenDownBuilding jd)
		{
			jd.FailOnDespawnedNullOrForbidden(TargetIndex.A);
			yield return Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.Touch).FailOnDespawnedNullOrForbidden(TargetIndex.B).FailOnSomeonePhysicallyInteracting(TargetIndex.B);
			yield return Toils_Haul.StartCarryThing(TargetIndex.B, false, false, false);
			yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch).FailOnDespawnedOrNull(TargetIndex.A);
			Toil repair = Toils_General.Wait(1000);
			repair.FailOnDespawnedOrNull(TargetIndex.A);
			repair.FailOnCannotTouch(TargetIndex.A, PathEndMode.Touch);
			repair.WithEffect(Traverse.Create(jd).Property("Building").GetValue<Building>().def.repairEffect, TargetIndex.A);
			repair.WithProgressBarToilDelay(TargetIndex.A, false, -0.5f);
			yield return repair;
			yield return new Toil {
				initAction = delegate
				{
					Traverse.Create(jd).Property("Components").GetValue<Thing>().Destroy(DestroyMode.Vanish);
					if (Rand.Value > jd.pawn.GetStatValue(StatDefOf.FixBrokenDownBuildingSuccessChance, true))
					{
						Vector3 loc = (jd.pawn.DrawPos + Traverse.Create(jd).Property("Building").GetValue<Building>().DrawPos) / 2f;
						MoteMaker.ThrowText(loc, Traverse.Create(jd).Property("Map").GetValue<Map>(), "TextMote_FixBrokenDownBuildingFail".Translate(), 3.65f);
					}
					else
					{
						Traverse.Create(jd).Property("Building").GetValue<Building>().GetComp<CompKeenKomponentBreakdownable>().Notify_Repaired();
					}
				}
			};
		}
	}
}
