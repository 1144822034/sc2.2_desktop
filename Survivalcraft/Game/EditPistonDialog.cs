using System;
using System.Xml.Linq;

namespace Game
{
	public class EditPistonDialog : Dialog
	{
		private LabelWidget m_title;

		private SliderWidget m_slider1;

		private SliderWidget m_slider2;

		private ContainerWidget m_panel2;

		private SliderWidget m_slider3;

		private ButtonWidget m_okButton;

		private ButtonWidget m_cancelButton;

		private Action<int> m_handler;

		private int m_data;

		private PistonMode m_mode;

		private int m_maxExtension;

		private int m_pullCount;

		private int m_speed;

		private static string[] m_speedNames = new string[4]
		{
			"Fast",
			"Medium",
			"Slow",
			"Very Slow"
		};

		public EditPistonDialog(int data, Action<int> handler)
		{
			XElement node = ContentManager.Get<XElement>("Dialogs/EditPistonDialog");
			LoadContents(this, node);
			m_title = Children.Find<LabelWidget>("EditPistonDialog.Title");
			m_slider1 = Children.Find<SliderWidget>("EditPistonDialog.Slider1");
			m_panel2 = Children.Find<ContainerWidget>("EditPistonDialog.Panel2");
			m_slider2 = Children.Find<SliderWidget>("EditPistonDialog.Slider2");
			m_slider3 = Children.Find<SliderWidget>("EditPistonDialog.Slider3");
			m_okButton = Children.Find<ButtonWidget>("EditPistonDialog.OK");
			m_cancelButton = Children.Find<ButtonWidget>("EditPistonDialog.Cancel");
			m_handler = handler;
			m_data = data;
			m_mode = PistonBlock.GetMode(data);
			m_maxExtension = PistonBlock.GetMaxExtension(data);
			m_pullCount = PistonBlock.GetPullCount(data);
			m_speed = PistonBlock.GetSpeed(data);
			m_title.Text = "Edit " + BlocksManager.Blocks[237].GetDisplayName(null, Terrain.MakeBlockValue(237, 0, data));
			m_slider1.Granularity = 1f;
			m_slider1.MinValue = 1f;
			m_slider1.MaxValue = 8f;
			m_slider2.Granularity = 1f;
			m_slider2.MinValue = 1f;
			m_slider2.MaxValue = 8f;
			m_slider3.Granularity = 1f;
			m_slider3.MinValue = 0f;
			m_slider3.MaxValue = 3f;
			m_panel2.IsVisible = (m_mode != PistonMode.Pushing);
			UpdateControls();
		}

		public override void Update()
		{
			if (m_slider1.IsSliding)
			{
				m_maxExtension = (int)m_slider1.Value - 1;
			}
			if (m_slider2.IsSliding)
			{
				m_pullCount = (int)m_slider2.Value - 1;
			}
			if (m_slider3.IsSliding)
			{
				m_speed = (int)m_slider3.Value;
			}
			if (m_okButton.IsClicked)
			{
				int value = PistonBlock.SetMaxExtension(PistonBlock.SetPullCount(PistonBlock.SetSpeed(m_data, m_speed), m_pullCount), m_maxExtension);
				Dismiss(value);
			}
			if (base.Input.Cancel || m_cancelButton.IsClicked)
			{
				Dismiss(null);
			}
			UpdateControls();
		}

		private void UpdateControls()
		{
			m_slider1.Value = m_maxExtension + 1;
			m_slider1.Text = $"{m_maxExtension + 1} blocks";
			m_slider2.Value = m_pullCount + 1;
			m_slider2.Text = $"{m_pullCount + 1} blocks";
			m_slider3.Value = m_speed;
			m_slider3.Text = m_speedNames[m_speed];
		}

		private void Dismiss(int? result)
		{
			DialogsManager.HideDialog(this);
			if (m_handler != null && result.HasValue)
			{
				m_handler(result.Value);
			}
		}
	}
}
