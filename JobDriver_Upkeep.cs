using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using RimWorld;
using Verse;
using Verse.AI;

namespace KeenKomponents
{
	public class JobDriver_Upkeep : JobDriver
	{
		protected float ticksToNextUpkeep;

		private float WarmupTicks = 80f;

		private float TicksBetweenUpkeeps = 20f;

		protected override IEnumerable<Toil> MakeNewToils()
		{
			this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
			yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);
			Toil upkeep = new Toil();
			upkeep.initAction = delegate
			{
				ticksToNextUpkeep = 80f;
			};
			upkeep.tickAction = delegate
			{
				Pawn actor = upkeep.actor;

				actor.skills.Learn(SkillDefOf.Construction, 0.1f, false);
				actor.skills.Learn(SkillDefOf.Crafting, 0.1f, false);
				actor.skills.Learn(SkillDefOf.Intellectual, 0.1f, false);

				float statValue = (actor.GetStatValue(StatDefOf.ConstructionSpeed, true) + actor.GetStatValue(StatDefOf.ResearchSpeed, true)) / 3f;

				Log.Message(actor.LabelCap + " Construction Speed: " + actor.GetStatValue(StatDefOf.ConstructionSpeed, true).ToString() +
					", Research Speed: " + actor.GetStatValue(StatDefOf.ResearchSpeed, true).ToString());

				ticksToNextUpkeep -= statValue;
				if (ticksToNextUpkeep <= 0f)
				{
					ticksToNextUpkeep += 20f;
					TargetThingA.TryGetComp<CompKeenKomponentBreakdownable>().FloDurability++;
					if (TargetThingA.TryGetComp<CompKeenKomponentBreakdownable>().FloDurability == KeenKompUtilities.floMaxDurability)
					{
						// TODO: add upkeep RecordDef
						//actor.records.Increment(RecordDefOf.ThingsRepaired);
						actor.jobs.EndCurrentJob(JobCondition.Succeeded, true);
					}
				}
			};
			upkeep.FailOnCannotTouch(TargetIndex.A, PathEndMode.Touch);
			upkeep.WithEffect(base.TargetThingA.def.repairEffect, TargetIndex.A);
			upkeep.defaultCompleteMode = ToilCompleteMode.Never;
			yield return upkeep;
		}

		public override bool TryMakePreToilReservations()
		{
			return pawn.Reserve(job.targetA, job, 1, -1, null);
		}
	}
}
