using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Sound;
using RimWorld;

namespace KeenKomponents
{
	[StaticConstructorOnStartup]
	public static class KeenKompUtilities
	{
		public static List<ThingDef> lstThingsWithPower = DefDatabase<ThingDef>.AllDefs.Where(x => x.ConnectToPower == true || x.EverTransmitsPower == true).ToList();
		public static float floMaxDurability = 27702f;
		public static float floMaintWoodRate = 3f;
		//public static float floMaintSteelRate = 1f;
		public static float floMaintSteelRate = 20f;
		public static float floMaintPlasteelRate = .5f;
		public static float floMaintSilverRate = 1.4f;
		public static float floMaintGoldRate = 1.8f;
		public static float floMaintUraniumRate = .1f;
		public static float floMaintComponentRate = .5f;
		public static float floMaintAdvancedComponentRate = .25f;
		public static float floMaintActiveMultiplier = 1.5f;
		public static float floMaintOperatingMultiplier = 1.5f;
		public static float floUpkeepThreshold = .8f;

		static KeenKompUtilities()
		{
			AttachComp();
			DettachComp();
			AttachITab();
		}

		static void AttachITab()
		{
			foreach (ThingDef td in lstThingsWithPower)
			{
				//Log.Message(td.label);
				try
				{
					if (td.comps.Exists(x => x.GetType() == typeof(CompProperties_KeenKomponentsBreakdownable)))
					{
						if (td.inspectorTabs == null)
						{
							td.inspectorTabs = new List<Type>();
						}
						td.inspectorTabs.Add(typeof(ITab_KeenKomp_Details));
						td.ResolveReferences();
					}
				}
				catch (Exception e)
				{
					Log.Message(string.Format("EXCEPTION! {0}.{1} \n\tMESSAGE: {2} \n\tException occurred calling {3} method", e.TargetSite.ReflectedType.Name,
						e.TargetSite.Name, e.Message));

				}
			}
		}

		static void AttachComp()
		{
			foreach (ThingDef td in lstThingsWithPower)
			{
				if (td.comps.Exists(x => x.GetType() == typeof(CompProperties_Breakdownable)))
				{
					td.comps.Add(new CompProperties_KeenKomponentsBreakdownable());
					//Log.Message("Added CompProperties_KeenKomponentsBreakdownable to " + td.label);
				}
				else
				{
					switch (td.label)
					{
						case "standing lamp":
						case "standing lamp (red)":
						case "standing lamp (green)":
						case "standing lamp (blue)":
						case "sun lamp":
						case "tube television":
						case "flatscreen television":
						case "megascreen television":
						case "moisture pump":
						case "multi-analyzer":
						case "vitals monitor":
						case "electric smithy":
						case "hi-tech research bench":
						case "electric crematorium":
						case "deep drill":
						case "ship cryptosleep casket":
						case "ship computer core":
						case "ship reactor":
						case "ship engine":
						case "sensor cluster":
						case "vanometric power cell":
							{
								td.comps.Add(new CompProperties_KeenKomponentsBreakdownable());
								break;
							}
					}
				}
			}
		}

		static void DettachComp()
		{
			foreach (ThingDef td in lstThingsWithPower)
			{
				//Log.Message("DettachComp: Checking " + td.label);
				foreach (CompProperties cp in td.comps)
				{
					//Log.Message("\t Checking " + cp.ToString());
					if (cp.GetType() == typeof(CompProperties_Breakdownable))
					{
						td.comps.Remove(cp);
						//Log.Message("\t \t Removed CompProperties_Breakdownable from " + td.label);
						break;
					}
				}
			}
		}

	}
}
