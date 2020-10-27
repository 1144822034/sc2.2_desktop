using System.Xml.Linq;

namespace Game
{
	public class SettingsUiScreen : Screen
	{
		public ContainerWidget m_windowModeContainer;

		public ButtonWidget m_windowModeButton;

		public ButtonWidget m_languageButton;

		public ButtonWidget m_uiSizeButton;

		public ButtonWidget m_upsideDownButton;

		public ButtonWidget m_hideMoveLookPadsButton;

		public ButtonWidget m_showGuiInScreenshotsButton;

		public ButtonWidget m_showLogoInScreenshotsButton;

		public ButtonWidget m_screenshotSizeButton;

		public ButtonWidget m_communityContentModeButton;

		public SettingsUiScreen()
		{
			XElement node = ContentManager.Get<XElement>("Screens/SettingsUiScreen");
			LoadContents(this, node);
			m_windowModeContainer = Children.Find<ContainerWidget>("WindowModeContainer");
			m_languageButton = Children.Find<ButtonWidget>("LanguageButton");
			m_windowModeButton = Children.Find<ButtonWidget>("WindowModeButton");
			m_uiSizeButton = Children.Find<ButtonWidget>("UiSizeButton");
			m_upsideDownButton = Children.Find<ButtonWidget>("UpsideDownButton");
			m_hideMoveLookPadsButton = Children.Find<ButtonWidget>("HideMoveLookPads");
			m_showGuiInScreenshotsButton = Children.Find<ButtonWidget>("ShowGuiInScreenshotsButton");
			m_showLogoInScreenshotsButton = Children.Find<ButtonWidget>("ShowLogoInScreenshotsButton");
			m_screenshotSizeButton = Children.Find<ButtonWidget>("ScreenshotSizeButton");
			m_communityContentModeButton = Children.Find<ButtonWidget>("CommunityContentModeButton");
		}

		public override void Enter(object[] parameters)
		{
			m_windowModeContainer.IsVisible = true;
		}

		public override void Update()
		{
			if (m_windowModeButton.IsClicked)
			{
				SettingsManager.WindowMode = (WindowMode)((int)(SettingsManager.WindowMode + 1) % EnumUtils.GetEnumValues(typeof(WindowMode)).Count);
			}
			if (m_languageButton.IsClicked)
			{
				DialogsManager.ShowDialog(null, new MessageDialog(LanguageControl.getTranslate("modify.are.you.sure"), LanguageControl.getTranslate("language.tip"), LanguageControl.getTranslate("system.yes"), LanguageControl.getTranslate("system.no"), delegate (MessageDialogButton button)
				{
					if (button == MessageDialogButton.Button1)
					{
						ModsManager.modSettings.languageType = (LanguageControl.LanguageType)((int)(ModsManager.modSettings.languageType + 1) % EnumUtils.GetEnumValues(typeof(LanguageControl.LanguageType)).Count);
						LanguageControl.init(ModsManager.modSettings.languageType);
						ModsManager.SaveSettings();
					}
				}));
			}
			if (m_uiSizeButton.IsClicked)
			{
				SettingsManager.GuiSize = (GuiSize)((int)(SettingsManager.GuiSize + 1) % EnumUtils.GetEnumValues(typeof(GuiSize)).Count);
			}
			if (m_upsideDownButton.IsClicked)
			{
				SettingsManager.UpsideDownLayout = !SettingsManager.UpsideDownLayout;
			}
			if (m_hideMoveLookPadsButton.IsClicked)
			{
				SettingsManager.HideMoveLookPads = !SettingsManager.HideMoveLookPads;
			}
			if (m_showGuiInScreenshotsButton.IsClicked)
			{
				SettingsManager.ShowGuiInScreenshots = !SettingsManager.ShowGuiInScreenshots;
			}
			if (m_showLogoInScreenshotsButton.IsClicked)
			{
				SettingsManager.ShowLogoInScreenshots = !SettingsManager.ShowLogoInScreenshots;
			}
			if (m_screenshotSizeButton.IsClicked)
			{
				SettingsManager.ScreenshotSize = (ScreenshotSize)((int)(SettingsManager.ScreenshotSize + 1) % EnumUtils.GetEnumValues(typeof(ScreenshotSize)).Count);
			}
			if (m_communityContentModeButton.IsClicked)
			{
				SettingsManager.CommunityContentMode = (CommunityContentMode)((int)(SettingsManager.CommunityContentMode + 1) % EnumUtils.GetEnumValues(typeof(CommunityContentMode)).Count);
			}
			m_windowModeButton.Text =LanguageControl.getTranslate("WindowMode." + SettingsManager.WindowMode.ToString());
			m_uiSizeButton.Text =LanguageControl.getTranslate("GuiSize." + SettingsManager.GuiSize.ToString());
			m_languageButton.Text = LanguageControl.getShow(ModsManager.modSettings.languageType);
			m_upsideDownButton.Text = (SettingsManager.UpsideDownLayout ? LanguageControl.getTranslate("system.yes") : LanguageControl.getTranslate("system.no"));
			m_hideMoveLookPadsButton.Text = (SettingsManager.HideMoveLookPads ? LanguageControl.getTranslate("system.yes") : LanguageControl.getTranslate("system.no"));
			m_showGuiInScreenshotsButton.Text = (SettingsManager.ShowGuiInScreenshots ? LanguageControl.getTranslate("system.yes") : LanguageControl.getTranslate("system.no"));
			m_showLogoInScreenshotsButton.Text = (SettingsManager.ShowLogoInScreenshots ? LanguageControl.getTranslate("system.yes") : LanguageControl.getTranslate("system.no"));
			m_screenshotSizeButton.Text =LanguageControl.getTranslate("ScreenshotSize." + SettingsManager.ScreenshotSize.ToString());
			m_communityContentModeButton.Text =LanguageControl.getTranslate("CommunityContentMode." + SettingsManager.CommunityContentMode.ToString());
			if (base.Input.Back || base.Input.Cancel || Children.Find<ButtonWidget>("TopBar.Back").IsClicked)
			{
				ScreensManager.SwitchScreen(ScreensManager.PreviousScreen);
			}
		}
	}
}
