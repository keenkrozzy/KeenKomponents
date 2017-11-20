using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using RimWorld;

namespace KeenKomponents
{
	public class CompKeenKomponentBreakdownable : ThingComp
	{
		private bool isBroke;
		private float floDurability;
		private int intTotalMaterials = 0;
		private float floMaterialWearPerTick = 0;
		private Dictionary<string, int> materials = new Dictionary<string, int>();
		private CompPowerTrader powerComp;
		private float floDurabilityPercent = 0;

		public const string BreakdownSignal = "Breakdown";

		public bool IsBroke
		{
			get
			{
				return isBroke;
			}
		}

		public float FloDurability
		{
			get
			{
				return floDurability;
			}
			set
			{
				floDurability += value;
				if (floDurability > KeenKompUtilities.floMaxDurability)
				{
					floDurability = KeenKompUtilities.floMaxDurability;
				}
				floDurabilityPercent = floDurability / KeenKompUtilities.floMaxDurability;
			}
		}

		public float FloDurabilityPercent
		{
			get
			{
				return floDurabilityPercent;
			}
		}

		public float FloMaterialWearPerTick
		{
			get
			{
				return floMaterialWearPerTick;
			}
		}

		public CompKeenKomponentBreakdownable()
		{
			floDurability = KeenKompUtilities.floMaxDurability;
		}

		public override void PostExposeData()
		{
			base.PostExposeData();
			Scribe_Values.Look<bool>(ref isBroke, "isBroke", false, false);
			Scribe_Values.Look<float>(ref floDurability, "floDurability", 0, false);
		}

		public override void PostDraw()
		{
			if (IsBroke)
			{
				parent.Map.overlayDrawer.DrawOverlay(parent, OverlayTypes.BrokenDown);
			}
		}

		public override void PostSpawnSetup(bool respawningAfterLoad)
		{
			base.PostSpawnSetup(respawningAfterLoad);
			powerComp = parent.GetComp<CompPowerTrader>();
			parent.Map.GetComponent<MapComponent_KeenKomponentStats>().Register(this);
			PopulateStuff(parent.def.costList);
			AverageWearAndTear();
			floDurabilityPercent = floDurability / KeenKompUtilities.floMaxDurability;
		}

		public override void PostDeSpawn(Map map)
		{
			base.PostDeSpawn(map);
			map.GetComponent<MapComponent_KeenKomponentStats>().Deregister(this);
		}

		public void CheckForBreakdown()
		{
			floDurabilityPercent = floDurability / KeenKompUtilities.floMaxDurability;

			//Log.Message("CompKeenKomponentBreakdownable.CheckForBreakdown: " + parent.def.label + " durability at " + floDurabilityPercent.ToStringPercent() + ", MaterialWearPerTick at "
			//	+ floMaterialWearPerTick.ToString());

			if (floDurability > 0)
			{
				floDurability -= floMaterialWearPerTick;
			}

			if (floDurability < 0)
			{
				floDurability = 0;
			}

			/***************************************************
			* CheckForBreakdown is called 57.637 times per day *
			***************************************************/
			if (CanBreakdownNow())
			{
				if (floDurabilityPercent >= .75f && (Rand.Range(1, 28012) > 28010)) // well maintained should average 8.1 years without breakdown
				{
					Log.Message(parent.Label + " has broke at " + floDurabilityPercent.ToStringPercent() + " durability with a loss rate of " + 
						floMaterialWearPerTick.ToString() + ". (if >= .75f)");
					DoBreakdown();
				}
				else if (floDurabilityPercent >= .50f && floDurabilityPercent < .75f && (Rand.Range(1, 17291) > 17289)) // maintained should average 5 years without breakdown
				{
					Log.Message(parent.Label + " has broke at " + floDurabilityPercent.ToStringPercent() + " durability with a loss rate of " +
						floMaterialWearPerTick.ToString() + ". (if >= .50f && < .75f)");
					DoBreakdown();
				}
				else if (floDurabilityPercent >= .25f && floDurabilityPercent < .50f && (Rand.Range(1, 3458) > 3456)) // barely maintained should average 1 year without breakdown
				{
					Log.Message(parent.Label + " has broke at " + floDurabilityPercent.ToStringPercent() + " durability with a loss rate of " +
						floMaterialWearPerTick.ToString() + ". (if >= .25f && < .50f)");
					DoBreakdown();
				}
				else if (floDurabilityPercent >= .01f && floDurabilityPercent < .25f && Rand.Range(1, 865) > 863) // not maintained should average a season without breakdown
				{
					Log.Message(parent.Label + " has broke at " + floDurabilityPercent.ToStringPercent() + " durability with a loss rate of " +
						floMaterialWearPerTick.ToString() + ". (if >= .01f && < .25f)");
					DoBreakdown();
				}
				else if (floDurabilityPercent < .01f && Rand.Range(1, 58) > 56) // ready to break should average a day
				{
					Log.Message(parent.Label + " has broke at " + floDurabilityPercent.ToStringPercent() + " durability with a loss rate of " +
						floMaterialWearPerTick.ToString() + ". (if < .01f)");
					DoBreakdown();
				}

			}
		}

		protected bool CanBreakdownNow()
		{
			return !IsBroke && (powerComp == null || powerComp.PowerOn);
		}

		public void Notify_Repaired()
		{
			isBroke = false;
			floDurability = KeenKompUtilities.floMaxDurability;
			parent.Map.GetComponent<MapComponent_KeenKomponentStats>().Notify_Repaired(parent);
			if (parent is Building_PowerSwitch)
			{
				parent.Map.powerNetManager.Notfiy_TransmitterTransmitsPowerNowChanged(parent.GetComp<CompPower>());
			}
		}

		public void DoBreakdown()
		{
			isBroke = true;
			parent.BroadcastCompSignal("Breakdown");
			parent.Map.GetComponent<MapComponent_KeenKomponentStats>().Notify_BrokenDown(parent);
			if (parent.Faction == Faction.OfPlayer)
			{
				Find.LetterStack.ReceiveLetter("LetterLabelBuildingBrokenDown".Translate(new object[]
				{
					parent.LabelShort
				}), "LetterBuildingBrokenDown".Translate(new object[]
				{
					parent.LabelShort
				}), LetterDefOf.NegativeEvent, parent, null);
			}
		}

		public override string CompInspectStringExtra()
		{
			if (IsBroke)
			{
				return "BrokenDown".Translate();
			}
			return null;
		}

		private void PopulateStuff(List<ThingCountClass> lstTCC)
		{
			lstTCC.OrderByDescending(x => x.count);
			materials = lstTCC.ToDictionary(x => x.thingDef.label, v => v.count);

			string str = "";

			foreach (KeyValuePair<string, int> pair in materials)
			{

				str = string.Format("{0}{1} {2}\n", str, pair.Key, pair.Value);

				if (pair.Key != "component")
				{
					intTotalMaterials += pair.Value;
				}
			}

			Log.Message(str + "Total Materials: " + intTotalMaterials.ToString());
		}

		private void AverageWearAndTear()
		{
			foreach (KeyValuePair<string, int> pair in materials)
			{
				switch (pair.Key)
				{
					case "wood":
						floMaterialWearPerTick += pair.Value * KeenKompUtilities.floMaintWoodRate;
						break;
					case "steel":
						floMaterialWearPerTick += pair.Value * KeenKompUtilities.floMaintSteelRate;
						break;
					case "plasteel":
						floMaterialWearPerTick += pair.Value * KeenKompUtilities.floMaintPlasteelRate;
						break;
					case "silver":
						floMaterialWearPerTick += pair.Value * KeenKompUtilities.floMaintSilverRate;
						break;
					case "gold":
						floMaterialWearPerTick += pair.Value * KeenKompUtilities.floMaintGoldRate;
						break;
					case "uranium":
						floMaterialWearPerTick += pair.Value * KeenKompUtilities.floMaintUraniumRate;
						break;
				}
			}

			floMaterialWearPerTick = floMaterialWearPerTick / intTotalMaterials;

			if (materials.TryGetValue("component", out int v1))
			{
				floMaterialWearPerTick += v1 * KeenKompUtilities.floMaintComponentRate;
			}

			if (materials.TryGetValue("advanced component", out int v2))
			{
				floMaterialWearPerTick += v2 * KeenKompUtilities.floMaintAdvancedComponentRate;
			}
		}

		public string Materials()
		{
			string strReturn = "";

			foreach (KeyValuePair<string, int> pair in materials)
			{
				if (pair.Key == "component" && pair.Value > 1)
				{
					strReturn = string.Format("{0}{1}, ", strReturn, "components");
				}
				else if (pair.Key == "advanced component" && pair.Value > 1)
				{
					strReturn = string.Format("{0}{1}, ", strReturn, "advanced components");
				}
				else
				{
					strReturn = string.Format("{0}{1}, ", strReturn, pair.Key);
				}
			}

			return strReturn.Substring(0, strReturn.Length - 2);
		}
	}
}
