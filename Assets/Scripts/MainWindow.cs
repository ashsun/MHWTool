using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MHW
{
	public class MainWindow : MonoBehaviour
	{
		[SerializeField]
		public SkillUI[] skillUIs;

		[SerializeField]
		public StoneUI stoneUI;

		[SerializeField]
		public DecorationUI decorationUI;

		[SerializeField]
		public Button searchUI;

		[SerializeField]
		public Transform resultDialog;

		[SerializeField]
		public SuitUI suitUI;

		public void Start()
		{
			SkillDatas.Init();
			EquipDatas.Init();

			//Skill
			List<Dropdown.OptionData> skillOptions = new List<Dropdown.OptionData>();
			skillOptions.Add(new Dropdown.OptionData(@"空"));
			foreach (var skill in SkillDatas.GetSkillDatas())
			{
				skillOptions.Add(new Dropdown.OptionData(skill.Key));
			}
			foreach (var skillUI in skillUIs)
			{
				skillUI.dropdown.options = skillOptions;
				skillUI.dropdown.onValueChanged.AddListener(skillUI.OnSelectSkill);
				skillUI.add.onClick.AddListener(skillUI.OnAddLevel);
				skillUI.minus.onClick.AddListener(skillUI.OnMinusLevel);
				skillUI.skill = null;
				skillUI.RefreshUI();
			}

			//Stone
			List<Dropdown.OptionData> stoneOptions = new List<Dropdown.OptionData>();
			stoneOptions.Add(new Dropdown.OptionData(@"空"));
			foreach (var equip in EquipDatas.GetEquipDatas()[EquipType.Stone])
			{
				stoneOptions.Add(new Dropdown.OptionData(equip.Key));
			}
			stoneUI.dropdown.options = stoneOptions;
			stoneUI.dropdown.onValueChanged.AddListener(stoneUI.OnSelectStone);
			stoneUI.stone = null;

			//Decoration
			List<Dropdown.OptionData> decorationOptions = new List<Dropdown.OptionData>();
			decorationOptions.Add(new Dropdown.OptionData(@"空"));
			foreach (var equip in EquipDatas.GetEquipDatas()[EquipType.Decoration])
			{
				decorationOptions.Add(new Dropdown.OptionData(equip.Key));
			}
			decorationUI.dropdown.options = decorationOptions;
			decorationUI.dropdown.onValueChanged.AddListener(decorationUI.OnSelectDecoration);
			decorationUI.add.onClick.AddListener(decorationUI.OnAdd);
			decorationUI.decoration = null;
			decorationUI.decorationItems = new List<DecorationUI.DecorationItem>();
			decorationUI.RefreshUI();

			//Search
			searchUI.onClick.AddListener(OnSearch);

			//Dialog
			resultDialog.GetComponentInChildren<Button>().onClick.AddListener(OnDialogConfirm);
			resultDialog.gameObject.SetActive(false);

			//Suit
			suitUI.skillUIs = skillUIs;
			suitUI.suitsSelection.onValueChanged.AddListener(suitUI.OnSelectSuit);
			suitUI.selectedSuit = null;
			suitUI.suits = new List<Suit>();
			suitUI.RefreshSelection();
		}

		private void OnSearch()
		{
			Dictionary<string, int> requriedSkills = new Dictionary<string, int>();
			foreach (var skillUI in skillUIs)
			{
				if (skillUI.skill != null)
				{
					if (!requriedSkills.ContainsKey(skillUI.skill.title))
					{
						requriedSkills.Add(skillUI.skill.title, 0);
					}
					requriedSkills[skillUI.skill.title] = Mathf.Max(requriedSkills[skillUI.skill.title], skillUI.skill.level);
				}
			}
			List<Decoration> existDecorations = new List<Decoration>();
			foreach (var decorationItem in decorationUI.decorationItems)
			{
				if (decorationItem.decoration != null)
				{
					existDecorations.Add(decorationItem.decoration);
				}
			}
			Stone existStone = stoneUI.stone;

			string rst = "";

			if (requriedSkills.Count > 0)
			{
				suitUI.suits = Suit.FindSuits(requriedSkills, existDecorations, existStone);
				suitUI.RefreshSelection();
				const string rstFormat = @"搜索到{0}个符合要求的组合";
				rst = string.Format(rstFormat, suitUI.suits.Count);
			}
			else
			{
				rst = @"请选择至少一个技能进行搜索";
			}
			resultDialog.gameObject.SetActive(true);
			resultDialog.Find("Text").GetComponent<Text>().text = rst;
		}

		private void OnDialogConfirm()
		{
			resultDialog.gameObject.SetActive(false);
		}

		[Serializable]
		public class SkillUI
		{
			public Dropdown dropdown;
			public Button add;
			public Transform levelUI;
			public Button minus;

			public Skill skill = null;
			public int maxLevel = 0;

			public void OnSelectSkill(int selectID)
			{
				if (selectID == 0)
				{
					skill = null;
					maxLevel = 0;
				}
				else
				{
					skill = new Skill();
					skill.title = dropdown.options[selectID].text;
					skill.level = 1;
					maxLevel = SkillDatas.GetSkillDataBySkill(skill).maxLevel;
				}

				RefreshUI();
			}

			public void OnAddLevel()
			{
				if (skill == null)
				{
					return;
				}

				skill.level = Mathf.Clamp(skill.level + 1, 1, maxLevel);

				RefreshUI();
			}

			public void OnMinusLevel()
			{
				if (skill == null)
				{
					return;
				}

				skill.level = Mathf.Clamp(skill.level - 1, 1, maxLevel);

				RefreshUI();
			}

			public void RefreshUI()
			{
				add.gameObject.SetActive(skill != null);
				minus.gameObject.SetActive(skill != null);

				int level = skill != null ? skill.level : 0;
				Image[] levelImages = levelUI.GetComponentsInChildren<Image>();
				for (int i = 0; i < levelImages.Length; i++)
				{
					if (i + 1 > maxLevel)
					{
						levelImages[i].color = new Color(1, 1, 1, 0);
					}
					else if (i + 1 > level)
					{
						levelImages[i].color = new Color(1, 1, 1, 1);
					}
					else
					{
						levelImages[i].color = new Color(140f / 255f, 140f / 255f, 140f / 255f, 1);
					}
				}
			}
		}

		[Serializable]
		public class StoneUI
		{
			public Dropdown dropdown;

			public Stone stone = null;

			public void OnSelectStone(int selectID)
			{
				if (selectID == 0)
				{
					stone = null;
				}
				else
				{
					stone = new Stone(dropdown.options[selectID].text);
				}
			}
		}

		[Serializable]
		public class DecorationUI
		{
			public Dropdown dropdown;
			public Button add;
			public ScrollRect scroller;
			public Transform scrollItem;

			public Decoration decoration = null;
			public List<DecorationItem> decorationItems = new List<DecorationItem>();

			public void OnSelectDecoration(int selectID)
			{
				if (selectID == 0)
				{
					decoration = null;
				}
				else
				{
					decoration = new Decoration(dropdown.options[selectID].text);
				}

				RefreshUI();
			}

			public void RefreshUI()
			{
				add.gameObject.SetActive(decoration != null);
			}

			public void OnAdd()
			{
				if (decoration == null)
				{
					return;
				}

				decorationItems.Add(new DecorationItem(this, decoration));

				RefreshList();
			}

			public void OnMinus(DecorationItem item)
			{
				decorationItems.Remove(item);

				RefreshList();
			}

			private void RefreshList()
			{
				scroller.content.sizeDelta = new Vector2(scroller.content.sizeDelta.x, decorationItems.Count * scrollItem.GetComponent<RectTransform>().sizeDelta.y);

				for (int i = 0; i < decorationItems.Count; i++)
				{
					decorationItems[i].item.GetComponent<RectTransform>().sizeDelta = scrollItem.GetComponent<RectTransform>().sizeDelta;
					decorationItems[i].item.GetComponent<RectTransform>().offsetMin = scrollItem.GetComponent<RectTransform>().offsetMin;
					decorationItems[i].item.GetComponent<RectTransform>().offsetMax = scrollItem.GetComponent<RectTransform>().offsetMax;
					decorationItems[i].item.transform.position -= new Vector3(0, i * decorationItems[i].item.GetComponent<RectTransform>().sizeDelta.y, 0);
				}
			}

			public class DecorationItem
			{
				public Decoration decoration;
				public GameObject item;

				public DecorationItem(DecorationUI ui, Decoration decoration)
				{
					this.decoration = decoration;

					item = GameObject.Instantiate(ui.scrollItem.gameObject);
					item.transform.SetParent(ui.scrollItem.parent);
					item.gameObject.SetActive(true);
					item.transform.Find("Text").GetComponent<Text>().text = decoration.title;
					item.transform.Find("Minus").GetComponent<Button>().onClick.AddListener(
						delegate()
						{
							ui.OnMinus(this);
							GameObject.Destroy(item);
						}
					);
				}
			}
		}

		[Serializable]
		public class SuitUI
		{
			public SkillUI[] skillUIs;

			public Dropdown suitsSelection;
			public ScrollRect scroller;
			public Text equips;
			public Transform scrollItem;

			public Suit selectedSuit = null;
			public List<Suit> suits = new List<Suit>();

			public void OnSelectSuit(int selectID)
			{
				if (suits.Count > selectID)
				{
					selectedSuit = suits[selectID];
				}
				else
				{
					selectedSuit = null;
				}

				RefreshContent();
			}

			public void RefreshSelection()
			{
				List<Dropdown.OptionData> suitOptions = new List<Dropdown.OptionData>();
				foreach (var suit in suits)
				{
					suitOptions.Add(new Dropdown.OptionData(suit.ToString()));
				}
				suitsSelection.options = suitOptions;
				suitsSelection.value = 0;

				OnSelectSuit(0);
			}

			public void RefreshContent()
			{
				const string equipText =
					"头部：{0}\n" +
					"胸部：{1}\n" +
					"手部：{2}\n" +
					"腰部：{3}\n" +
					"腿部：{4}\n" +
					"护石：{5}";

				if (selectedSuit == null)
				{
					equips.text = string.Format(equipText, @"无", @"无", @"无", @"无", @"无", @"无");
				}
				else
				{
					Equip headEquip = selectedSuit.equips.ContainsKey(EquipType.Head) ? selectedSuit.equips[EquipType.Head] : null;
					Equip bodyEquip = selectedSuit.equips.ContainsKey(EquipType.Body) ? selectedSuit.equips[EquipType.Body] : null;
					Equip armEquip = selectedSuit.equips.ContainsKey(EquipType.Arm) ? selectedSuit.equips[EquipType.Arm] : null;
					Equip waistEquip = selectedSuit.equips.ContainsKey(EquipType.Waist) ? selectedSuit.equips[EquipType.Waist] : null;
					Equip legEquip = selectedSuit.equips.ContainsKey(EquipType.Leg) ? selectedSuit.equips[EquipType.Leg] : null;
					Equip stoneEquip = selectedSuit.equips.ContainsKey(EquipType.Stone) ? selectedSuit.equips[EquipType.Stone] : null;
					equips.text = string.Format(equipText,
						headEquip != null ? headEquip.title : @"无",
						bodyEquip != null ? bodyEquip.title : @"无",
						armEquip != null ? armEquip.title : @"无",
						waistEquip != null ? waistEquip.title : @"无",
						legEquip != null ? legEquip.title : @"无",
						stoneEquip != null ? stoneEquip.title : @"无"
					);
				}

				for (int i = scrollItem.parent.childCount - 1; i >= 0; i--)
				{
					var content = scrollItem.parent.GetChild(i);
					if (content != scrollItem)
					{
						GameObject.Destroy(content.gameObject);
					}
				}
				if (selectedSuit != null)
				{
					HashSet<string> skillNames = new HashSet<string>();
					foreach (var skillUI in skillUIs)
					{
						if (skillUI.skill != null)
						{
							skillNames.Add(skillUI.skill.title);
						}
					}
					foreach (var type in Enum.GetValues(typeof(EquipType)))
					{
						if (selectedSuit.equips.ContainsKey((EquipType)type))
						{
							var equip = selectedSuit.equips[(EquipType)type];
							if (equip != null)
							{
								foreach (var skill in EquipDatas.GetEquipDataByEquip(equip).skills)
								{
									skillNames.Add(skill.title);
								}
							}
						}
					}

					HashSet<int> holeLevels = new HashSet<int>() { 1, 2, 3 };

					scroller.content.sizeDelta = new Vector2(scroller.content.sizeDelta.x, (skillNames.Count + holeLevels.Count) * scrollItem.GetComponent<RectTransform>().sizeDelta.y);
					int index = 0;
					foreach (var skillName in skillNames)
					{
						var item = GameObject.Instantiate(scrollItem);
						item.transform.SetParent(scrollItem.parent);
						item.gameObject.SetActive(true);
						item.transform.Find("Name").GetComponent<Text>().text = skillName;
						int total = 0;
						foreach (var type in Enum.GetValues(typeof(EquipType)))
						{
							var text = item.transform.Find(type.ToString());
							if (text == null)
							{
								continue;
							}
							int level = 0;
							if (selectedSuit.equips.ContainsKey((EquipType)type))
							{
								var equip = selectedSuit.equips[(EquipType)type];
								if (equip != null)
								{
									var skills = EquipDatas.GetEquipDataByEquip(equip).skills;
									foreach (var skill in skills)
									{
										if (skill.title == skillName)
										{
											level = skill.level;
										}
									}
								}
							}
							text.GetComponent<Text>().text = level.ToString();
							total += level;
						}
						item.transform.Find("Total").GetComponent<Text>().text = total.ToString();

						item.GetComponent<RectTransform>().sizeDelta = scrollItem.GetComponent<RectTransform>().sizeDelta;
						item.GetComponent<RectTransform>().offsetMin = scrollItem.GetComponent<RectTransform>().offsetMin;
						item.GetComponent<RectTransform>().offsetMax = scrollItem.GetComponent<RectTransform>().offsetMax;
						item.transform.position -= new Vector3(0, index++ * scrollItem.GetComponent<RectTransform>().sizeDelta.y, 0);
					}
					foreach (var holeLevel in holeLevels)
					{
						var item = GameObject.Instantiate(scrollItem);
						item.transform.SetParent(scrollItem.parent);
						item.gameObject.SetActive(true);
						item.transform.Find("Name").GetComponent<Text>().text = holeLevel + "级孔";
						int total = 0;
						foreach (var type in Enum.GetValues(typeof(EquipType)))
						{
							var text = item.transform.Find(type.ToString());
							if (text == null)
							{
								continue;
							}
							int count = 0;
							if (selectedSuit.equips.ContainsKey((EquipType)type))
							{
								var equip = selectedSuit.equips[(EquipType)type];
								if (equip != null)
								{
									var holes = EquipDatas.GetEquipDataByEquip(equip).holes;
									foreach (var hole in holes)
									{
										if (hole.maxLevel == holeLevel)
										{
											count++;
										}
									}
								}
							}
							text.GetComponent<Text>().text = count.ToString();
							total += count;
						}
						item.transform.Find("Total").GetComponent<Text>().text = total.ToString();

						item.GetComponent<RectTransform>().sizeDelta = scrollItem.GetComponent<RectTransform>().sizeDelta;
						item.GetComponent<RectTransform>().offsetMin = scrollItem.GetComponent<RectTransform>().offsetMin;
						item.GetComponent<RectTransform>().offsetMax = scrollItem.GetComponent<RectTransform>().offsetMax;
						item.transform.position -= new Vector3(0, index++ * scrollItem.GetComponent<RectTransform>().sizeDelta.y, 0);
					}
				}
			}
		}
	}
}
