using System;
using System.Collections.Generic;
using UnityEngine;

namespace MHW
{
	[Serializable]
	public class SkillData
	{
		[SerializeField]
		public string title;

		[SerializeField]
		public int maxLevel;
	}

	public class SkillDatas
	{
		private static Dictionary<string, SkillData> _skillDatas;

		public static void Init()
		{
			_skillDatas = new Dictionary<string, SkillData>();

			TextAsset skillData = Resources.Load<TextAsset>("SkillData");
			string[] lines = skillData.text.Split('\n');
			foreach (var line in lines)
			{
				if (!string.IsNullOrEmpty(line))
				{
					var skill = JsonUtility.FromJson<SkillData>(line);
					_skillDatas.Add(skill.title, skill);
				}
			}
		}

		public static Dictionary<string, SkillData> GetSkillDatas()
		{
			return _skillDatas;
		}

		public static SkillData GetSkillDataBySkill(Skill skill)
		{
			var title = skill.title;
			if (_skillDatas.ContainsKey(title))
			{
				return _skillDatas[title];
			}
			return null;
		}
	}
}

