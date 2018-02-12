using System;
using System.Collections.Generic;
using UnityEngine;

namespace MHW
{
	public enum EquipType
	{
		Head,
		Body,
		Arm,
		Waist,
		Leg,
		Decoration,
		Stone
	}

	[Serializable]
	public class EquipData
	{
		public EquipType type;

		public string title;

		[SerializeField]
		public EquipSkillData[] skills;

		[SerializeField]
		public HoleData[] holes;

		[SerializeField]
		public int level;
	}

	[Serializable]
	public class EquipSkillData
	{
		public string title;
		public int level;
	}

	[Serializable]
	public class HoleData
	{
		[SerializeField]
		public int maxLevel;
	}

	public class EquipDatas
	{
		private static Dictionary<EquipType, Dictionary<string, EquipData>> _equipDatas;

		public static void Init()
		{
			_equipDatas = new Dictionary<EquipType, Dictionary<string, EquipData>>()
			{
				{ EquipType.Head, new Dictionary<string, EquipData>() },
				{ EquipType.Body, new Dictionary<string, EquipData>() },
				{ EquipType.Arm, new Dictionary<string, EquipData>() },
				{ EquipType.Waist, new Dictionary<string, EquipData>() },
				{ EquipType.Leg, new Dictionary<string, EquipData>() },
				{ EquipType.Decoration, new Dictionary<string, EquipData>() },
				{ EquipType.Stone, new Dictionary<string, EquipData>() },
			};

			TextAsset equipData = Resources.Load<TextAsset>("EquipData");
			string[] lines = equipData.text.Split('\n');
			foreach (var line in lines)
			{
				if (!string.IsNullOrEmpty(line))
				{
					var equip = JsonUtility.FromJson<EquipData>(line);
					_equipDatas[equip.type].Add(equip.title, equip);
				}
			}
		}

		public static Dictionary<EquipType, List<Equip>> GetFilteredEquips(Dictionary<string, int> skills)
		{
			var filteredEquips = new Dictionary<EquipType, List<Equip>>()
			{
				{ EquipType.Head, new List<Equip>() { null } },
				{ EquipType.Body, new List<Equip>() { null } },
				{ EquipType.Arm, new List<Equip>() { null } },
				{ EquipType.Waist, new List<Equip>() { null } },
				{ EquipType.Leg, new List<Equip>() { null } },
				{ EquipType.Stone, new List<Equip>() { null } },
			};
			foreach (var equipListPair in _equipDatas)
			{
				var equipType = equipListPair.Key;
				if (filteredEquips.ContainsKey(equipType))
				{
					foreach (var equipPair in equipListPair.Value)
					{
						var equipTitle = equipPair.Key;
						var equipData = equipPair.Value;
						foreach (var equipSkill in equipData.skills)
						{
							if (skills.ContainsKey(equipSkill.title))
							{
								filteredEquips[equipType].Add(new Equip(equipType, equipTitle));
								break;
							}
						}
					}
				}
			}
			return filteredEquips;
		}

		public static Dictionary<EquipType, Dictionary<string, EquipData>> GetEquipDatas()
		{
			return _equipDatas;
		}

		public static EquipData GetEquipDataByEquip(Equip equip)
		{
			var type = equip.type;
			var title = equip.title;
			if (_equipDatas.ContainsKey(type))
			{
				if (_equipDatas[type].ContainsKey(title))
				{
					return _equipDatas[type][title];
				}
			}
			return null;
		}
	}
}


