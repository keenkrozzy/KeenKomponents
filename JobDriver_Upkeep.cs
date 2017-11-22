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
		private float floWorkToBeDone = 0f;
		private float floWorkDone = 0f;

		private float ProgressLeft()
		{
			if (floWorkToBeDone != 0)
			{
				return floWorkDone / floWorkToBeDone;
			}
			else
			{
				return 0;
			}
		}

		protected override IEnumerable<Toil> MakeNewToils()
		{
			this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
			yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);

			Toil upkeep = new Toil();

			upkeep.initAction = delegate
			{
				floWorkToBeDone = KeenKomp_Settings.floMaxDurability - TargetThingA.TryGetComp<CompKeenKomponentBreakdownable>().FloDurability;
			};

			upkeep.tickAction = delegate
			{
				Pawn actor = upkeep.actor;

				actor.skills.Learn(SkillDefOf.Construction, 0.1f, false);
				float statValue = actor.GetStatValue(StatDefOf.ConstructionSpeed, true);

				if (!(actor.skills.skills.Find(x => x.def.defName == "Crafting")).TotallyDisabled)
				{
					actor.skills.Learn(SkillDefOf.Crafting, 0.1f, false);
				}

				if (!(actor.skills.skills.Find(x => x.def.defName == "Intellectual")).TotallyDisabled)
				{
					actor.skills.Learn(SkillDefOf.Intellectual, 0.1f, false);
					statValue += actor.GetStatValue(StatDefOf.ResearchSpeed, true);
				}

				statValue = statValue * 10f;

				TargetThingA.TryGetComp<CompKeenKomponentBreakdownable>().FloDurability += statValue;
				floWorkDone += statValue;

				Log.Message(floWorkDone.ToString() + "\t" + TargetThingA.TryGetComp<CompKeenKomponentBreakdownable>().FloDurability.ToString());

					if (TargetThingA.TryGetComp<CompKeenKomponentBreakdownable>().FloDurability == KeenKomp_Settings.floMaxDurability)
					{
						// TODO: add upkeep RecordDef
						//actor.records.Increment(RecordDefOf.ThingsRepaired);
						actor.jobs.EndCurrentJob(JobCondition.Succeeded, true);
						floWorkToBeDone = 0f;
						floWorkDone = 0f;
					}
			};

			upkeep.FailOnCannotTouch(TargetIndex.A, PathEndMode.Touch);
			upkeep.WithEffect(base.TargetThingA.def.repairEffect, TargetIndex.A);
			upkeep.WithProgressBar(TargetIndex.A, ProgressLeft, true);
			upkeep.defaultCompleteMode = ToilCompleteMode.Never;
			yield return upkeep;
		}

		public override bool TryMakePreToilReservations()
		{
			return pawn.Reserve(job.targetA, job, 1, -1, null);
		}
	}
}
