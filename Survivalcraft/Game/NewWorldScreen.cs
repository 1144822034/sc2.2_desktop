using System.Collections.Generic;
using System.Xml.Linq;

namespace Game
{
	public class NewWorldScreen : Screen
	{
		private TextBoxWidget m_nameTextBox;

		private TextBoxWidget m_seedTextBox;

		private ButtonWidget m_gameModeButton;

		private ButtonWidget m_startingPositionButton;

		private ButtonWidget m_worldOptionsButton;

		private LabelWidget m_blankSeedLabel;

		private LabelWidget m_descriptionLabel;

		private LabelWidget m_errorLabel;

		private ButtonWidget m_playButton;

		private Random m_random = new Random();

		private WorldSettings m_worldSettings;

		public NewWorldScreen()
		{
			XElement node = ContentManager.Get<XElement>("Screens/NewWorldScreen");
			LoadContents(this, node);
			m_nameTextBox = Children.Find<TextBoxWidget>("Name");
			m_seedTextBox = Children.Find<TextBoxWidget>("Seed");
			m_gameModeButton = Children.Find<ButtonWidget>("GameMode");
			m_startingPositionButton = Children.Find<ButtonWidget>("StartingPosition");
			m_worldOptionsButton = Children.Find<ButtonWidget>("WorldOptions");
			m_blankSeedLabel = Children.Find<LabelWidget>("BlankSeed");
			m_descriptionLabel = Children.Find<LabelWidget>("Description");
			m_errorLabel = Children.Find<LabelWidget>("Error");
			m_playButton = Children.Find<ButtonWidget>("Play");
			m_nameTextBox.TextChanged += delegate
			{
				m_worldSettings.Name = m_nameTextBox.Text;
			};
			m_seedTextBox.TextChanged += delegate
			{
				m_worldSettings.Seed = m_seedTextBox.Text;
			};
		}

		public override void Enter(object[] parameters)
		{
			if (ScreensManager.PreviousScreen.GetType() != typeof(WorldOptionsScreen))
			{
				m_worldSettings = new WorldSettings
				{
					Name = WorldsManager.NewWorldNames[m_random.Int(0, WorldsManager.NewWorldNames.Count - 1)],
					OriginalSerializationVersion = VersionsManager.SerializationVersion
				};
			}
		}

		public override void Update()
		{
			if (m_gameModeButton.IsClicked)
			{
				IList<int> enumValues = EnumUtils.GetEnumValues(typeof(GameMode));
				m_worldSettings.GameMode = (GameMode)((enumValues.IndexOf((int)m_worldSettings.GameMode) + 1) % enumValues.Count);
				while (m_worldSettings.GameMode == GameMode.Adventure)
				{
					m_worldSettings.GameMode = (GameMode)((enumValues.IndexOf((int)m_worldSettings.GameMode) + 1) % enumValues.Count);
				}
			}
			if (m_startingPositionButton.IsClicked)
			{
				IList<int> enumValues2 = EnumUtils.GetEnumValues(typeof(StartingPositionMode));
				m_worldSettings.StartingPositionMode = (StartingPositionMode)((enumValues2.IndexOf((int)m_worldSettings.StartingPositionMode) + 1) % enumValues2.Count);
			}
			bool flag = WorldsManager.ValidateWorldName(m_worldSettings.Name);
			m_nameTextBox.Text = m_worldSettings.Name;
			m_seedTextBox.Text = m_worldSettings.Seed;
			m_gameModeButton.Text = m_worldSettings.GameMode.ToString();
			m_startingPositionButton.Text = m_worldSettings.StartingPositionMode.ToString();
			m_playButton.IsVisible = flag;
			m_errorLabel.IsVisible = !flag;
			m_blankSeedLabel.IsVisible = (m_worldSettings.Seed.Length == 0 && !m_seedTextBox.HasFocus);
			m_descriptionLabel.Text = StringsManager.GetString("GameMode." + m_worldSettings.GameMode.ToString() + ".Description");
			if (m_worldOptionsButton.IsClicked)
			{
				ScreensManager.SwitchScreen("WorldOptions", m_worldSettings, false);
			}
			if (m_playButton.IsClicked && WorldsManager.ValidateWorldName(m_nameTextBox.Text))
			{
				if (m_worldSettings.GameMode != 0)
				{
					m_worldSettings.ResetOptionsForNonCreativeMode();
				}
				WorldInfo worldInfo = WorldsManager.CreateWorld(m_worldSettings);
				ScreensManager.SwitchScreen("GameLoading", worldInfo, null);
			}
			if (base.Input.Back || base.Input.Cancel || Children.Find<ButtonWidget>("TopBar.Back").IsClicked)
			{
				ScreensManager.SwitchScreen("Play");
			}
		}
	}
}
