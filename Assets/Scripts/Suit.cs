using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MHW
{
	public class Skill
	{
		public string title;
		public int level;
	}

	public class Equip
	{
		public EquipType type;
		public string title;

		public Equip(EquipType type, string title)
		{
			this.type = type;
			this.title = title;
		}
	}

	public class Decoration : Equip
	{
		public Decoration(string title) : base(EquipType.Decoration, title) { }
	}

	public class Stone : Equip
	{
		public Stone(string title) : base(EquipType.Stone, title) { }
	}

	public class Suit
	{
		public Dictionary<EquipType, Equip> equips;

		public Suit(List<Equip> equips)
		{
			this.equips = new Dictionary<EquipType, Equip>();
			foreach (var equip in equips)
			{
				if (equip != null)
				{
					this.equips.Add(equip.type, equip);
				}
			}
		}

		public override string ToString()
		{
			string suitName = "";
			foreach (var equipPair in equips)
			{
				suitName += "[" + equipPair.Key + "]" + equipPair.Value.title + "+";
			}
			return suitName.Substring(0, suitName.Length - 1);
		}

		public static List<Suit> FindSuits(Dictionary<string, int> requriedSkills, List<Decoration> existDecorations, Stone existStone)
		{
			Dictionary<EquipType, List<Equip>> filteredEquips = EquipDatas.GetFilteredEquips(requriedSkills);
			if (existStone != null)
			{
				filteredEquips[EquipType.Stone].Clear();
				filteredEquips[EquipType.Stone].Add(null);
				filteredEquips[EquipType.Stone].Add(existStone);
			}

			List<Suit> suits = new List<Suit>();

			foreach (var head in filteredEquips[EquipType.Head])
			{
				foreach (var body in filteredEquips[EquipType.Body])
				{
					foreach (var arm in filteredEquips[EquipType.Arm])
					{
						foreach (var waist in filteredEquips[EquipType.Waist])
						{
							foreach (var leg in filteredEquips[EquipType.Leg])
							{
								foreach (var stone in filteredEquips[EquipType.Stone])
								{
									var equips = new List<Equip> { head, body, arm, waist, leg, stone };
									if (TestEquips(requriedSkills, equips, existDecorations))
									{
										suits.Add(new Suit(equips));
									}
								}
							}
						}
					}
				}
			}

			return suits;
		}

		private static bool TestEquips(Dictionary<string, int> requriedSkills, List<Equip> equips, List<Decoration> decorations)
		{
			var skills = new Dictionary<string, int>();
			var holeDatas = new List<HoleData>();
			foreach (var equip in equips)
			{
				if (equip != null)
				{
					var equipData = EquipDatas.GetEquipDataByEquip(equip);
					foreach (var skill in equipData.skills)
					{
						var skillTitle = skill.title;
						var skillLevel = skill.level;
						if (!skills.ContainsKey(skillTitle))
						{
							skills.Add(skillTitle, 0);
						}
						skills[skillTitle] += skillLevel;
					}
					holeDatas.AddRange(equipData.holes);
				}
			}

			var tempRequriedSkills = new Dictionary<string, int>(requriedSkills);
			foreach (var skill in skills)
			{
				var skillTitle = skill.Key;
				var skillLevel = skill.Value;
				if (tempRequriedSkills.ContainsKey(skillTitle))
				{
					tempRequriedSkills[skillTitle] -= skillLevel;
				}
			}
			foreach (var skill in tempRequriedSkills)
			{
				if (skill.Value > 0)
				{
					return TryDecoration(tempRequriedSkills, holeDatas, decorations);
				}
			}
			return true;
		}

		private static bool TryDecoration(Dictionary<string, int> requriedSkills, List<HoleData> holes, List<Decoration> decorations)
		{
			bool success = true;
			foreach (var skillPair in requriedSkills)
			{
				if (skillPair.Value > 0)
				{
					success = false;
					break;
				}
			}
			if (!success)
			{
				if (holes.Count == 0 || decorations.Count == 0)
				{
					success = false;
				}
				else
				{
					var lastHole = holes[holes.Count - 1];
					holes.Remove(lastHole);

					if (TryDecoration(requriedSkills, holes, decorations))
					{
						success = true;
					}
					else
					{
						for (int i = 0; i < decorations.Count; i++)
						{
							var decoration = decorations[i];
							var decorationData = EquipDatas.GetEquipDataByEquip(decorations[i]);
							if (lastHole.maxLevel >= decorationData.level)
							{
								foreach (var skill in decorationData.skills)
								{
									if (requriedSkills.ContainsKey(skill.title))
									{
										requriedSkills[skill.title] -= skill.level;
									}
								}
								decorations.RemoveAt(i);

								if (TryDecoration(requriedSkills, holes, decorations))
								{
									success = true;
								}

								foreach (var skill in decorationData.skills)
								{
									if (requriedSkills.ContainsKey(skill.title))
									{
										requriedSkills[skill.title] += skill.level;
									}
								}
								decorations.Insert(i, decoration);

								if (success)
								{
									break;
								}
							}
						}
					}

					holes.Add(lastHole);
				}
			}
			return success;
		}
	}
}
