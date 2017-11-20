using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using Verse.AI;

namespace KeenKomponents
{
	public class KeenKomp_Upkeep : JobDef
	{
		public KeenKomp_Upkeep()
		{
			defName = "KeenKomp_Upkeep";
			driverClass = typeof(JobDriver_Upkeep);
			reportString = "Cleaning, inspecting, and fine-tuning TargetA.";
		}
	}
}
