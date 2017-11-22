using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using Verse.AI;

namespace KeenKomponents
{
	public class WorkGiver_Upkeep : WorkGiver_Scanner
	{
		public JobDef JobDefOf_Upkeep = new KeenKomp_Upkeep();

		public override bool HasJobOnThing(Pawn pawn, Thing thing, bool forced = false)
		{
			if (thing.Faction != pawn.Faction)
			{
				return false;
			}
			if (pawn.Faction == Faction.OfPlayer && !pawn.Map.areaManager.Home[thing.Position] && !forced)
			{
				return false;
			}
			if (FireUtility.IsBurning(thing))
			{
				return false;
			}
			if (thing.Map.designationManager.DesignationOn(thing, DesignationDefOf.Deconstruct) != null)
			{
				return false;
			}
			ThingWithComps thingWithComps = thing as ThingWithComps;
			if (thingWithComps == null)
			{
				return false;
			}
			CompKeenKomponentBreakdownable compBreakdownable = ThingCompUtility.TryGetComp<CompKeenKomponentBreakdownable>(thingWithComps);

			return compBreakdownable != null && thing.TryGetComp<CompKeenKomponentBreakdownable>().FloDurability < KeenKomp_Settings.floMaxDurability && 
				ReservationUtility.CanReserveAndReach(pawn, thing, PathEndMode.Touch, DangerUtility.NormalMaxDanger(pawn), 1, -1, null, forced);
		}

		public override Job JobOnThing(Pawn pawn, Thing thing, bool forced)
		{
			return new Job(JobDefOf_Upkeep, thing);
		}

		public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
		{
			return pawn.Map.GetComponent<MapComponent_KeenKomponentStats>().ThingsThatNeedUpkeep;
		}

		public override bool ShouldSkip(Pawn pawn)
		{
			return !pawn.Map.GetComponent<MapComponent_KeenKomponentStats>().ThingsThatNeedUpkeep.Any<Thing>();
		}
	}
}
