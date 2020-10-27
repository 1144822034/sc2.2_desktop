using Engine;
using System.Collections.Generic;
using System.Xml.Linq;

namespace Game
{
	public class RecipaediaDescriptionScreen : Screen
	{
		public BlockIconWidget m_blockIconWidget;

		public LabelWidget m_nameWidget;

		public ButtonWidget m_leftButtonWidget;

		public ButtonWidget m_rightButtonWidget;

		public LabelWidget m_descriptionWidget;

		public LabelWidget m_propertyNames1Widget;

		public LabelWidget m_propertyValues1Widget;

		public LabelWidget m_propertyNames2Widget;

		public LabelWidget m_propertyValues2Widget;

		public int m_index;

		public IList<int> m_valuesList;

		public RecipaediaDescriptionScreen()
		{
			XElement node = ContentManager.Get<XElement>("Screens/RecipaediaDescriptionScreen");
			LoadContents(this, node);
			m_blockIconWidget = Children.Find<BlockIconWidget>("Icon");
			m_nameWidget = Children.Find<LabelWidget>("Name");
			m_leftButtonWidget = Children.Find<ButtonWidget>("Left");
			m_rightButtonWidget = Children.Find<ButtonWidget>("Right");
			m_descriptionWidget = Children.Find<LabelWidget>("Description");
			m_propertyNames1Widget = Children.Find<LabelWidget>("PropertyNames1");
			m_propertyValues1Widget = Children.Find<LabelWidget>("PropertyValues1");
			m_propertyNames2Widget = Children.Find<LabelWidget>("PropertyNames2");
			m_propertyValues2Widget = Children.Find<LabelWidget>("PropertyValues2");
		}

		public override void Enter(object[] parameters)
		{
			int item = (int)parameters[0];
			m_valuesList = (IList<int>)parameters[1];
			m_index = m_valuesList.IndexOf(item);
			UpdateBlockProperties();
		}

		public override void Update()
		{
			m_leftButtonWidget.IsEnabled = (m_index > 0);
			m_rightButtonWidget.IsEnabled = (m_index < m_valuesList.Count - 1);
			if (m_leftButtonWidget.IsClicked || base.Input.Left)
			{
				m_index = MathUtils.Max(m_index - 1, 0);
				UpdateBlockProperties();
			}
			if (m_rightButtonWidget.IsClicked || base.Input.Right)
			{
				m_index = MathUtils.Min(m_index + 1, m_valuesList.Count - 1);
				UpdateBlockProperties();
			}
			if (base.Input.Back || base.Input.Cancel || Children.Find<ButtonWidget>("TopBar.Back").IsClicked)
			{
				ScreensManager.SwitchScreen(ScreensManager.PreviousScreen);
			}
		}

		public Dictionary<string, string> GetBlockProperties(int value)
		{
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			int num = Terrain.ExtractContents(value);
			Block block = BlocksManager.Blocks[num];
			if (block.DefaultEmittedLightAmount > 0)
			{
				dictionary.Add("亮度", block.DefaultEmittedLightAmount.ToString());
			}
			if (block.FuelFireDuration > 0f)
			{
				dictionary.Add("燃烧值", block.FuelFireDuration.ToString());
			}
			dictionary.Add("可堆叠", (block.MaxStacking > 1) ? ("是 (上限" + block.MaxStacking.ToString() + ")") : LanguageControl.getTranslate("system.no"));
			dictionary.Add("可燃烧", (block.FireDuration > 0f) ? LanguageControl.getTranslate("system.yes") : LanguageControl.getTranslate("system.no"));
			if (block.GetNutritionalValue(value) > 0f)
			{
				dictionary.Add("营养", block.GetNutritionalValue(value).ToString());
			}
			if (block.GetRotPeriod(value) > 0)
			{
				dictionary.Add("最大储藏时间", $"{(float)(2 * block.GetRotPeriod(value)) * 60f / 1200f:0.0} 天");
			}
			if (block.DigMethod != 0)
			{
				dictionary.Add("挖掘方法",LanguageControl.getTranslate( block.DigMethod.ToString()));
				dictionary.Add("挖掘抗性", block.DigResilience.ToString());
			}
			if (block.ExplosionResilience > 0f)
			{
				dictionary.Add("爆炸抗性", block.ExplosionResilience.ToString());
			}
			if (block.GetExplosionPressure(value) > 0f)
			{
				dictionary.Add("爆炸威力", block.GetExplosionPressure(value).ToString());
			}
			bool flag = false;
			if (block.GetMeleePower(value) > 1f)
			{
				dictionary.Add("近战攻击力", block.GetMeleePower(value).ToString());
				flag = true;
			}
			if (block.GetMeleePower(value) > 1f)
			{
				dictionary.Add("近战命中率", $"{100f * block.GetMeleeHitProbability(value):0}%");
				flag = true;
			}
			if (block.GetProjectilePower(value) > 1f)
			{
				dictionary.Add("投掷攻击力", block.GetProjectilePower(value).ToString());
				flag = true;
			}
			if (block.ShovelPower > 1f)
			{
				dictionary.Add("铲", block.ShovelPower.ToString());
				flag = true;
			}
			if (block.HackPower > 1f)
			{
				dictionary.Add("斧", block.HackPower.ToString());
				flag = true;
			}
			if (block.QuarryPower > 1f)
			{
				dictionary.Add("稿", block.QuarryPower.ToString());
				flag = true;
			}
			if (flag && block.Durability > 0)
			{
				dictionary.Add("耐久", block.Durability.ToString());
			}
			if (block.DefaultExperienceCount > 0f)
			{
				dictionary.Add("经验球", block.DefaultExperienceCount.ToString());
			}
			if (block is ClothingBlock)
			{
				ClothingData clothingData = ClothingBlock.GetClothingData(Terrain.ExtractData(value));
				dictionary.Add("可以染色", clothingData.CanBeDyed ? LanguageControl.getTranslate("system.yes") : LanguageControl.getTranslate("system.no"));
				dictionary.Add("护甲防御", $"{(int)(clothingData.ArmorProtection * 100f)}%");
				dictionary.Add("装备耐久", clothingData.Sturdiness.ToString());
				dictionary.Add("绝缘", $"{clothingData.Insulation:0.0} clo");
				dictionary.Add("移速加成", $"{clothingData.MovementSpeedFactor * 100f:0}%");
			}
			return dictionary;
		}

		public void UpdateBlockProperties()
		{
			if (m_index >= 0 && m_index < m_valuesList.Count)
			{
				int value = m_valuesList[m_index];
				int num = Terrain.ExtractContents(value);
				Block block = BlocksManager.Blocks[num];
				m_blockIconWidget.Value = value;
				m_nameWidget.Text = block.GetDisplayName(null, value);
				m_descriptionWidget.Text = block.GetDescription(value);
				m_propertyNames1Widget.Text = string.Empty;
				m_propertyValues1Widget.Text = string.Empty;
				m_propertyNames2Widget.Text = string.Empty;
				m_propertyValues2Widget.Text = string.Empty;
				Dictionary<string, string> blockProperties = GetBlockProperties(value);
				int num2 = 0;
				foreach (KeyValuePair<string, string> item in blockProperties)
				{
					if (num2 < blockProperties.Count - blockProperties.Count / 2)
					{
						LabelWidget propertyNames1Widget = m_propertyNames1Widget;
						propertyNames1Widget.Text = propertyNames1Widget.Text + item.Key + ":\n";
						LabelWidget propertyValues1Widget = m_propertyValues1Widget;
						propertyValues1Widget.Text = propertyValues1Widget.Text + item.Value + "\n";
					}
					else
					{
						LabelWidget propertyNames2Widget = m_propertyNames2Widget;
						propertyNames2Widget.Text = propertyNames2Widget.Text + item.Key + ":\n";
						LabelWidget propertyValues2Widget = m_propertyValues2Widget;
						propertyValues2Widget.Text = propertyValues2Widget.Text + item.Value + "\n";
					}
					num2++;
				}
			}
		}
	}
}
