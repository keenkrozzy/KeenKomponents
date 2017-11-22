using Verse;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;

namespace KeenKomponents
{
	[StaticConstructorOnStartup]
	public class KeenKomp_Settings : ModSettings
	{
		public static float floMaxDurability = 27702f;
		public static float floMaintWoodRate = 3f;
		public static float floMaintSteelRate = 1f;
		//public static float floMaintSteelRate = 20f;
		public static float floMaintPlasteelRate = .5f;
		public static float floMaintSilverRate = 1.4f;
		public static float floMaintGoldRate = 1.8f;
		public static float floMaintUraniumRate = .1f;
		public static float floMaintComponentRate = .5f;
		public static float floMaintAdvancedComponentRate = .25f;
		public static float floMaintActiveMultiplier = 1.5f;
		public static float floMaintOperatingMultiplier = 1.5f;
		public static float floUpkeepThreshold = .5f;

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look<float>(ref floUpkeepThreshold, "floUpkeepThreshold", .5f, false);
		}
	}

	public class KeenKomp_Mod : Mod
	{
		public static KeenKomp_Settings settings;

		public KeenKomp_Mod(ModContentPack content) : base(content)
		{
			settings = GetSettings<KeenKomp_Settings>();
		}

		public override string SettingsCategory() => "Keen Komponents";

		public override void DoSettingsWindowContents(Rect inRect)
		{
			float floPadding = inRect.width * .1f;
			string strUpkeepThreshhold = "Upkeep Threshold: " + KeenKomp_Settings.floUpkeepThreshold.ToStringPercent();

			Rect rectUpkeepThreshold = new Rect(floPadding, floPadding, inRect.width - (floPadding * 2f), Text.CalcSize(strUpkeepThreshhold).y);
			Rect rectUpkeepThresholdSlider = new Rect(floPadding, rectUpkeepThreshold.y + rectUpkeepThreshold.height, inRect.width - (floPadding * 2f), 50f);

			Widgets.Label(rectUpkeepThreshold, strUpkeepThreshhold);
			KeenKomp_Settings.floUpkeepThreshold = GUI.HorizontalSlider(rectUpkeepThresholdSlider.Rounded(), KeenKomp_Settings.floUpkeepThreshold, 0f, 1f);
		}


	}
}
