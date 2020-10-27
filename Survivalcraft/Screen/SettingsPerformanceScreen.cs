using Engine;
using System.Collections.Generic;
using System.Xml.Linq;

namespace Game
{
	public class SettingsPerformanceScreen : Screen
	{
		public static List<int> m_presentationIntervals = new List<int>
		{
			2,
			1,
			0
		};

		public static List<int> m_visibilityRanges = new List<int>
		{
			32,
			48,
			64,
			80,
			96,
			112,
			128,
			160,
			192,
			224,
			256,
			320
		};

		public ButtonWidget m_resolutionButton;

		public SliderWidget m_visibilityRangeSlider;

		public LabelWidget m_visibilityRangeWarningLabel;

		public ButtonWidget m_viewAnglesButton;

		public ButtonWidget m_terrainMipmapsButton;

		public ButtonWidget m_skyRenderingModeButton;

		public ButtonWidget m_objectShadowsButton;

		public SliderWidget m_framerateLimitSlider;

		public ButtonWidget m_displayFpsCounterButton;

		public ButtonWidget m_displayFpsRibbonButton;

		public int m_enterVisibilityRange;

		public SettingsPerformanceScreen()
		{
			XElement node = ContentManager.Get<XElement>("Screens/SettingsPerformanceScreen");
			LoadContents(this, node);
			m_resolutionButton = Children.Find<ButtonWidget>("ResolutionButton");
			m_visibilityRangeSlider = Children.Find<SliderWidget>("VisibilityRangeSlider");
			m_visibilityRangeWarningLabel = Children.Find<LabelWidget>("VisibilityRangeWarningLabel");
			m_viewAnglesButton = Children.Find<ButtonWidget>("ViewAnglesButton");
			m_terrainMipmapsButton = Children.Find<ButtonWidget>("TerrainMipmapsButton");
			m_skyRenderingModeButton = Children.Find<ButtonWidget>("SkyRenderingModeButton");
			m_objectShadowsButton = Children.Find<ButtonWidget>("ObjectShadowsButton");
			m_framerateLimitSlider = Children.Find<SliderWidget>("FramerateLimitSlider");
			m_displayFpsCounterButton = Children.Find<ButtonWidget>("DisplayFpsCounterButton");
			m_displayFpsRibbonButton = Children.Find<ButtonWidget>("DisplayFpsRibbonButton");
			m_visibilityRangeSlider.MinValue = 0f;
			m_visibilityRangeSlider.MaxValue = m_visibilityRanges.Count - 1;
		}

		public override void Enter(object[] parameters)
		{
			m_enterVisibilityRange = SettingsManager.VisibilityRange;
		}

		public override void Update()
		{
			if (m_resolutionButton.IsClicked)
			{
				IList<int> enumValues = EnumUtils.GetEnumValues(typeof(ResolutionMode));
				SettingsManager.ResolutionMode = (ResolutionMode)((enumValues.IndexOf((int)SettingsManager.ResolutionMode) + 1) % enumValues.Count);
			}
			if (m_visibilityRangeSlider.IsSliding)
			{
				SettingsManager.VisibilityRange = m_visibilityRanges[MathUtils.Clamp((int)m_visibilityRangeSlider.Value, 0, m_visibilityRanges.Count - 1)];
			}
			if (m_viewAnglesButton.IsClicked)
			{
				IList<int> enumValues2 = EnumUtils.GetEnumValues(typeof(ViewAngleMode));
				SettingsManager.ViewAngleMode = (ViewAngleMode)((enumValues2.IndexOf((int)SettingsManager.ViewAngleMode) + 1) % enumValues2.Count);
			}
			if (m_terrainMipmapsButton.IsClicked)
			{
				SettingsManager.TerrainMipmapsEnabled = !SettingsManager.TerrainMipmapsEnabled;
			}
			if (m_skyRenderingModeButton.IsClicked)
			{
				IList<int> enumValues3 = EnumUtils.GetEnumValues(typeof(SkyRenderingMode));
				SettingsManager.SkyRenderingMode = (SkyRenderingMode)((enumValues3.IndexOf((int)SettingsManager.SkyRenderingMode) + 1) % enumValues3.Count);
			}
			if (m_objectShadowsButton.IsClicked)
			{
				SettingsManager.ObjectsShadowsEnabled = !SettingsManager.ObjectsShadowsEnabled;
			}
			if (m_framerateLimitSlider.IsSliding)
			{
				SettingsManager.PresentationInterval = m_presentationIntervals[MathUtils.Clamp((int)m_framerateLimitSlider.Value, 0, m_presentationIntervals.Count - 1)];
			}
			if (m_displayFpsCounterButton.IsClicked)
			{
				SettingsManager.DisplayFpsCounter = !SettingsManager.DisplayFpsCounter;
			}
			if (m_displayFpsRibbonButton.IsClicked)
			{
				SettingsManager.DisplayFpsRibbon = !SettingsManager.DisplayFpsRibbon;
			}
			m_resolutionButton.Text =LanguageControl.getTranslate("ResolutionMode." + SettingsManager.ResolutionMode.ToString());
			m_visibilityRangeSlider.Value = ((m_visibilityRanges.IndexOf(SettingsManager.VisibilityRange) >= 0) ? m_visibilityRanges.IndexOf(SettingsManager.VisibilityRange) : 64);
			m_visibilityRangeSlider.Text = string.Format(LanguageControl.getTranslate("settingper.blocks"), SettingsManager.VisibilityRange);
			if (SettingsManager.VisibilityRange <= 48)
			{
				m_visibilityRangeWarningLabel.IsVisible = true;
				m_visibilityRangeWarningLabel.Text = "[settingper.good_for_lower]";
			}
			else if (SettingsManager.VisibilityRange <= 64)
			{
				m_visibilityRangeWarningLabel.IsVisible = false;
			}
			else if (SettingsManager.VisibilityRange <= 112)
			{
				m_visibilityRangeWarningLabel.IsVisible = true;
				m_visibilityRangeWarningLabel.Text = "[settingper.1gb_com]";
			}
			else if (SettingsManager.VisibilityRange <= 224)
			{
				m_visibilityRangeWarningLabel.IsVisible = true;
				m_visibilityRangeWarningLabel.Text = "[settingper.2gb_com]";
			}
			else if (SettingsManager.VisibilityRange <= 384)
			{
				m_visibilityRangeWarningLabel.IsVisible = true;
				m_visibilityRangeWarningLabel.Text = "[settingper.4gb_com]";
			}
			else if (SettingsManager.VisibilityRange <= 512)
			{
				m_visibilityRangeWarningLabel.IsVisible = true;
				m_visibilityRangeWarningLabel.Text = "[settingper.8gb_com]";
			}
			else
			{
				m_visibilityRangeWarningLabel.IsVisible = true;
				m_visibilityRangeWarningLabel.Text = "[settingper.16gb_com]";
			}
			m_viewAnglesButton.Text =LanguageControl.getTranslate("ViewAngleMode." + SettingsManager.ViewAngleMode.ToString());
			if (SettingsManager.TerrainMipmapsEnabled) {
				m_terrainMipmapsButton.Text = LanguageControl.getTranslate("system.enable");
			}
			else {
				m_terrainMipmapsButton.Text = LanguageControl.getTranslate("system.disable");
			}
			
			m_skyRenderingModeButton.Text =LanguageControl.getTranslate("SkyRenderingMode." + SettingsManager.SkyRenderingMode.ToString());
			m_objectShadowsButton.Text = SettingsManager.ObjectsShadowsEnabled ? LanguageControl.getTranslate("system.enable") : LanguageControl.getTranslate("system.disable");
			m_framerateLimitSlider.Value = (m_presentationIntervals.IndexOf(SettingsManager.PresentationInterval) >= 0) ? m_presentationIntervals.IndexOf(SettingsManager.PresentationInterval) : (m_presentationIntervals.Count - 1);
			m_framerateLimitSlider.Text = (SettingsManager.PresentationInterval != 0) ? string.Format(LanguageControl.getTranslate("settingper.vsync"), SettingsManager.PresentationInterval) : LanguageControl.getTranslate("settingper.unlimited");
			m_displayFpsCounterButton.Text = (SettingsManager.DisplayFpsCounter ? LanguageControl.getTranslate("system.yes") : LanguageControl.getTranslate("system.no"));
			m_displayFpsRibbonButton.Text = (SettingsManager.DisplayFpsRibbon ? LanguageControl.getTranslate("system.yes") : LanguageControl.getTranslate("system.no"));
			if (base.Input.Back || base.Input.Cancel || Children.Find<ButtonWidget>("TopBar.Back").IsClicked)
			{
				bool flag = SettingsManager.VisibilityRange > 128;
				if (SettingsManager.VisibilityRange > m_enterVisibilityRange && flag)
				{
					DialogsManager.ShowDialog(null, new MessageDialog(LanguageControl.getTranslate("settingper.large_tip"), LanguageControl.getTranslate("settingper.large_content"), LanguageControl.getTranslate("system.ok"), LanguageControl.getTranslate("system.back"), delegate(MessageDialogButton button)
					{
						if (button == MessageDialogButton.Button1)
						{
							ScreensManager.SwitchScreen(ScreensManager.PreviousScreen);
						}
					}));
				}
				else
				{
					ScreensManager.SwitchScreen(ScreensManager.PreviousScreen);
				}
			}
		}
	}
}
