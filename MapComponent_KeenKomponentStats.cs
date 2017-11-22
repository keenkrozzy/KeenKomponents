using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using RimWorld;
using UnityEngine;
using Verse;

namespace KeenKomponents
{
	public class MapComponent_KeenKomponentStats : MapComponent
	{
		public MapComponent_KeenKomponentStats(Map map) : base(map)
		{
		}

		private List<CompKeenKomponentBreakdownable> comps = new List<CompKeenKomponentBreakdownable>();

		public HashSet<Thing> brokenDownThings = new HashSet<Thing>();

		public IEnumerable<Thing> ThingsThatNeedUpkeep
		{
			get
			{
				List<CompKeenKomponentBreakdownable> lstNeedUpkeep = comps.Where(x => x.FloDurabilityPercent < KeenKomp_Settings.floUpkeepThreshold).ToList();
				return lstNeedUpkeep.Select(x => x.parent).Cast<Thing>();
			}
		}

		public const int CheckIntervalTicks = 1041;

		public void Register(CompKeenKomponentBreakdownable c)
		{
			comps.Add(c);
			if (c.IsBroke)
			{
				brokenDownThings.Add(c.parent);
				map.GetComponent<BreakdownManager>().Notify_BrokenDown(c.parent);
			}
		}

		public void Deregister(CompKeenKomponentBreakdownable c)
		{
			comps.Remove(c);
			brokenDownThings.Remove(c.parent);
		}

		public override void MapComponentTick()
		{
			if (Find.TickManager.TicksGame % 1041 == 0)
			{
				for (int i = 0; i < comps.Count; i++)
				{
					comps[i].CheckForBreakdown();
				}
			}
		}

		public void Notify_BrokenDown(Thing thing)
		{
			brokenDownThings.Add(thing);
			map.GetComponent<BreakdownManager>().Notify_BrokenDown(thing);
		}

		public void Notify_Repaired(Thing thing)
		{
			brokenDownThings.Remove(thing);
			map.GetComponent<BreakdownManager>().Notify_Repaired(thing);
		}

	}
}
