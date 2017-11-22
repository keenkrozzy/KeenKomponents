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
	public class ITab_KeenKomp_Details : ITab
	{
		private float floBorderPadding = 25f;
		private float floMaxDurability = KeenKomp_Settings.floMaxDurability;
		private static Texture2D texFullBar = SolidColorMaterials.NewSolidColorTexture(new Color(0.2f, 0.2f, 0.24f));
		private static Texture2D EmptyBarTex = SolidColorMaterials.NewSolidColorTexture(Color.clear);

		private MainTabWindow_Inspect InspectPane
		{
			get
			{
				return (MainTabWindow_Inspect)MainButtonDefOf.Inspect.TabWindow;
			}
		}

		static ITab_KeenKomp_Details()
		{
		}

		public ITab_KeenKomp_Details()
		{
			labelKey = "Details";
		}

		private Thing GetThing()
		{
			return SelThing;
		}

		private float GetDurabilityPercent()
		{
			return GetThing().TryGetComp<CompKeenKomponentBreakdownable>().FloDurabilityPercent;
		}

		private string GetMaterials()
		{
			return "Materials: " + GetThing().TryGetComp<CompKeenKomponentBreakdownable>().Materials();
		}

		private float GetDLPD()
		{
			float x = GetThing().TryGetComp<CompKeenKomponentBreakdownable>().FloMaterialWearPerTick;
			return (x * 57.637f) / KeenKomp_Settings.floMaxDurability;
		}

		private string GetBreakdownChanceString()
		{
			float x = GetDurabilityPercent();

			if (x >= .75f) // well maintained should average 8.1 years without breakdown
			{
				return "Breakdown Chance: Extremely Unlikely";
			}
			else if (x >= .50f && x < .75f) // maintained should average 5 years without breakdown
			{
				return "Breakdown Chance: Highly Unlikely";
			}
			else if (x >= .25f && x < .50f) // barely maintained should average 1 year without breakdown
			{
				return "Breakdown Chance: Possible";
			}
			else if (x >= .01f && x < .25f) // not maintained should average a season without breakdown
			{
				return "Breakdown Chance: Likely";
			}
			else if (x < .01f) // ready to break should average a day
			{
				return "Breakdown Chance: Very Soon";
			}
			else
			{
				return "There is a bug at ITab_KeenKomp_Details.GetBreakdownChanceString()";
			}
		}

		protected override void FillTab()
		{
			string strName = GetThing().LabelCap;
			string strMaterial = GetMaterials();
			string strDLPD = "Durability loss per day: " + GetDLPD().ToStringPercent();
			string strDurabilityPercent = GetDurabilityPercent().ToStringPercent();
			string strBreakdownChance = GetBreakdownChanceString();

			Text.Font = GameFont.Medium;
			Vector2 vecName = Text.CalcSize(strName);
			Text.Font = GameFont.Small;
			Vector2 vecMaterial = Text.CalcSize(strMaterial);
			Vector2 vecDLPD = Text.CalcSize(strDLPD);
			Vector2 vecDurability = Text.CalcSize(strDurabilityPercent);
			Vector2 vecBreakdownChance = Text.CalcSize(strBreakdownChance);

			size.x = Mathf.Max(new float[] { vecName.x, vecMaterial.x, vecDLPD.x, vecBreakdownChance.x }) + (floBorderPadding * 2f);
			size.y = vecName.y + vecMaterial.y + vecDLPD.y + vecDurability.y + vecBreakdownChance.y + (floBorderPadding * 2f);

			if (size.x < InspectPane.windowRect.width)
			{
				size.x = InspectPane.windowRect.width;
			}

			Rect rectName = new Rect(floBorderPadding, floBorderPadding, vecName.x, vecName.y);
			Rect rectMaterial = new Rect(floBorderPadding, rectName.y + rectName.height, vecMaterial.x, vecMaterial.y);
			Rect rectDLPD = new Rect(floBorderPadding, rectMaterial.y + rectMaterial.height, vecDLPD.x, vecDLPD.y);
			Rect rectDurability = new Rect(floBorderPadding, rectDLPD.y + rectDLPD.height, size.x - (floBorderPadding * 2f), vecDurability.y);
			Rect rectBreakdownChance = new Rect(floBorderPadding, rectDurability.y + rectDurability.height, vecBreakdownChance.x, vecBreakdownChance.y);

			Text.Font = GameFont.Medium;
			Widgets.Label(rectName.Rounded(), strName);
			Text.Font = GameFont.Small;
			Widgets.Label(rectMaterial.Rounded(), strMaterial);
			Widgets.Label(rectDLPD.Rounded(), strDLPD);
			Widgets.FillableBar(rectDurability.Rounded(), GetDurabilityPercent(), texFullBar);
			Text.Anchor = TextAnchor.MiddleCenter;
			Widgets.Label(rectDurability.Rounded(), strDurabilityPercent);
			Text.Anchor = TextAnchor.UpperLeft;
			Widgets.Label(rectBreakdownChance.Rounded(), strBreakdownChance);

			if (Prefs.DevMode)
			{
				string strMinusDurability = "(DevMode) -5% Durability";
				Vector2 vecMinusDurability = Text.CalcSize(strMinusDurability);
				size.y += vecMinusDurability.y * 1.8f;
				if (Widgets.ButtonText(new Rect(floBorderPadding, rectBreakdownChance.y + rectBreakdownChance.height, vecMinusDurability.x * 1.2f, vecMinusDurability.y * 1.8f), strMinusDurability))
				{
					GetThing().TryGetComp<CompKeenKomponentBreakdownable>().MinusFivePercent();
				}
			}
		}
	}
}
