using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using Verse;
using Verse.AI;
using RimWorld;

namespace KeenKomponents
{
	[StaticConstructorOnStartup]
	public static class Initialize
	{
		static Initialize()
		{
			WorkGiverDef wgd = new WorkGiverDef();
			wgd.defName = "Upkeep";
			wgd.label = "Upkeep";
			wgd.giverClass = typeof(WorkGiver_Upkeep);
			wgd.workType = WorkTypeDefOf.Construction;
			wgd.emergency = false;
			wgd.verb = "upkeep";
			wgd.gerund = "upkeeping";

			DefDatabase<WorkGiverDef>.Add(wgd);
			DefDatabase<WorkTypeDef>.GetNamed("Construction").ResolveReferences();
		}
	}
}
