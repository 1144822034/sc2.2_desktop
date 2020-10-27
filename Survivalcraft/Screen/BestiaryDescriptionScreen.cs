using Engine;
using Engine.Media;
using System.Collections.Generic;
using System.Xml.Linq;

namespace Game
{
	public class BestiaryDescriptionScreen : Screen
	{
		public ModelWidget m_modelWidget;

		public LabelWidget m_nameWidget;

		public ButtonWidget m_leftButtonWidget;

		public ButtonWidget m_rightButtonWidget;

		public LabelWidget m_descriptionWidget;

		public LabelWidget m_propertyNames1Widget;

		public LabelWidget m_propertyValues1Widget;

		public LabelWidget m_propertyNames2Widget;

		public LabelWidget m_propertyValues2Widget;

		public ContainerWidget m_dropsPanel;

		public int m_index;

		public IList<BestiaryCreatureInfo> m_infoList;

		public BestiaryDescriptionScreen()
		{
			XElement node = ContentManager.Get<XElement>("Screens/BestiaryDescriptionScreen");
			LoadContents(this, node);
			m_modelWidget = Children.Find<ModelWidget>("Model");
			m_nameWidget = Children.Find<LabelWidget>("Name");
			m_leftButtonWidget = Children.Find<ButtonWidget>("Left");
			m_rightButtonWidget = Children.Find<ButtonWidget>("Right");
			m_descriptionWidget = Children.Find<LabelWidget>("Description");
			m_propertyNames1Widget = Children.Find<LabelWidget>("PropertyNames1");
			m_propertyValues1Widget = Children.Find<LabelWidget>("PropertyValues1");
			m_propertyNames2Widget = Children.Find<LabelWidget>("PropertyNames2");
			m_propertyValues2Widget = Children.Find<LabelWidget>("PropertyValues2");
			m_dropsPanel = Children.Find<ContainerWidget>("Drops");
		}

		public override void Enter(object[] parameters)
		{
			BestiaryCreatureInfo item = (BestiaryCreatureInfo)parameters[0];
			m_infoList = (IList<BestiaryCreatureInfo>)parameters[1];
			m_index = m_infoList.IndexOf(item);
			UpdateCreatureProperties();
		}

		public override void Update()
		{
			m_leftButtonWidget.IsEnabled = (m_index > 0);
			m_rightButtonWidget.IsEnabled = (m_index < m_infoList.Count - 1);
			if (m_leftButtonWidget.IsClicked || base.Input.Left)
			{
				m_index = MathUtils.Max(m_index - 1, 0);
				UpdateCreatureProperties();
			}
			if (m_rightButtonWidget.IsClicked || base.Input.Right)
			{
				m_index = MathUtils.Min(m_index + 1, m_infoList.Count - 1);
				UpdateCreatureProperties();
			}
			if (base.Input.Back || base.Input.Cancel || Children.Find<ButtonWidget>("TopBar.Back").IsClicked)
			{
				ScreensManager.SwitchScreen(ScreensManager.PreviousScreen);
			}
		}

		public void UpdateCreatureProperties()
		{
			if (m_index >= 0 && m_index < m_infoList.Count)
			{
				BestiaryCreatureInfo bestiaryCreatureInfo = m_infoList[m_index];
				m_modelWidget.AutoRotationVector = new Vector3(0f, 1f, 0f);
				BestiaryScreen.SetupBestiaryModelWidget(bestiaryCreatureInfo, m_modelWidget, new Vector3(-1f, 0f, -1f), autoRotate: true, autoAspect: true);
				m_nameWidget.Text = bestiaryCreatureInfo.DisplayName;
				m_descriptionWidget.Text = bestiaryCreatureInfo.Description;
				m_propertyNames1Widget.Text = string.Empty;
				m_propertyValues1Widget.Text = string.Empty;
				m_propertyNames1Widget.Text += LanguageControl.getTranslate("bestiary.resilience");
				LabelWidget propertyValues1Widget = m_propertyValues1Widget;
				propertyValues1Widget.Text = propertyValues1Widget.Text + bestiaryCreatureInfo.AttackResilience.ToString() + "\n";
				m_propertyNames1Widget.Text += LanguageControl.getTranslate("bestiary.attack");
				LabelWidget propertyValues1Widget2 = m_propertyValues1Widget;
				propertyValues1Widget2.Text = propertyValues1Widget2.Text + ((bestiaryCreatureInfo.AttackPower > 0f) ? bestiaryCreatureInfo.AttackPower.ToString("0.0") : "None") + "\n";
				m_propertyNames1Widget.Text += LanguageControl.getTranslate("bestiary.herding");
				LabelWidget propertyValues1Widget3 = m_propertyValues1Widget;
				propertyValues1Widget3.Text = propertyValues1Widget3.Text + (bestiaryCreatureInfo.IsHerding ? LanguageControl.getTranslate("system.yes") : LanguageControl.getTranslate("system.no")) + "\n";
				m_propertyNames1Widget.Text += LanguageControl.getTranslate("bestiary.can_be");
				LabelWidget propertyValues1Widget4 = m_propertyValues1Widget;
				propertyValues1Widget4.Text = propertyValues1Widget4.Text + (bestiaryCreatureInfo.CanBeRidden ? LanguageControl.getTranslate("system.yes") : LanguageControl.getTranslate("system.no")) + "\n";
				m_propertyNames1Widget.Text = m_propertyNames1Widget.Text.TrimEnd();
				m_propertyValues1Widget.Text = m_propertyValues1Widget.Text.TrimEnd();
				m_propertyNames2Widget.Text = string.Empty;
				m_propertyValues2Widget.Text = string.Empty;
				m_propertyNames2Widget.Text += LanguageControl.getTranslate("bestiary.speed");
				LabelWidget propertyValues2Widget = m_propertyValues2Widget;
				propertyValues2Widget.Text = propertyValues2Widget.Text + ((double)bestiaryCreatureInfo.MovementSpeed * 3.6).ToString("0") + LanguageControl.getTranslate("bestiary.speed_unit");
				m_propertyNames2Widget.Text += LanguageControl.getTranslate("bestiary.jump_height");
				LabelWidget propertyValues2Widget2 = m_propertyValues2Widget;
				propertyValues2Widget2.Text = propertyValues2Widget2.Text + bestiaryCreatureInfo.JumpHeight.ToString("0.0") + LanguageControl.getTranslate("bestiary.length_unit");
				m_propertyNames2Widget.Text += LanguageControl.getTranslate("bestiary.weight");
				LabelWidget propertyValues2Widget3 = m_propertyValues2Widget;
				propertyValues2Widget3.Text = propertyValues2Widget3.Text + bestiaryCreatureInfo.Mass.ToString() + LanguageControl.getTranslate("bestiary.weight_unit");
				m_propertyNames2Widget.Text += LanguageControl.getTranslate("bestiary.spawner_eggs");
				LabelWidget propertyValues2Widget4 = m_propertyValues2Widget;
				propertyValues2Widget4.Text = propertyValues2Widget4.Text + (bestiaryCreatureInfo.HasSpawnerEgg ? LanguageControl.getTranslate("system.yes") : LanguageControl.getTranslate("system.no")) + "\n";
				m_propertyNames2Widget.Text = m_propertyNames2Widget.Text.TrimEnd();
				m_propertyValues2Widget.Text = m_propertyValues2Widget.Text.TrimEnd();
				m_dropsPanel.Children.Clear();
				if (bestiaryCreatureInfo.Loot.Count > 0)
				{
					foreach (ComponentLoot.Loot item in bestiaryCreatureInfo.Loot)
					{
						string text = (item.MinCount >= item.MaxCount) ? $"{item.MinCount}" : $"{item.MinCount} "+LanguageControl.getTranslate("system.range_tip") +" {item.MaxCount}";
						if (item.Probability < 1f)
						{
							text +=string.Format(LanguageControl.getTranslate("bestiary.of_time"),$"{item.Probability * 100f:0}");
						}
						m_dropsPanel.Children.Add(new StackPanelWidget
						{
							Margin = new Vector2(20f, 0f),
							Children = 
							{
								(Widget)new BlockIconWidget
								{
									Size = new Vector2(32f),
									Scale = 1.2f,
									VerticalAlignment = WidgetAlignment.Center,
									Value = item.Value
								},
								(Widget)new CanvasWidget
								{
									Size = new Vector2(10f, 0f)
								},
								(Widget)new LabelWidget
								{
									Font = ContentManager.Get<BitmapFont>("Fonts/Pericles"),
									VerticalAlignment = WidgetAlignment.Center,
									Text = text
								}
							}
						});
					}
				}
				else
				{
					m_dropsPanel.Children.Add(new LabelWidget
					{
						Margin = new Vector2(20f, 0f),
						Font = ContentManager.Get<BitmapFont>("Fonts/Pericles"),
						Text = LanguageControl.getTranslate("system.nothing")
					});
				}
			}
		}
	}
}
