using System;
using System.Xml.Linq;

namespace Game
{
	public class EditBatteryDialog : Dialog
	{
		private Action<int> m_handler;

		private ButtonWidget m_okButton;

		private ButtonWidget m_cancelButton;

		private SliderWidget m_voltageSlider;

		private int m_voltageLevel;

		public EditBatteryDialog(int voltageLevel, Action<int> handler)
		{
			XElement node = ContentManager.Get<XElement>("Dialogs/EditBatteryDialog");
			LoadContents(this, node);
			m_okButton = Children.Find<ButtonWidget>("EditBatteryDialog.OK");
			m_cancelButton = Children.Find<ButtonWidget>("EditBatteryDialog.Cancel");
			m_voltageSlider = Children.Find<SliderWidget>("EditBatteryDialog.VoltageSlider");
			m_handler = handler;
			m_voltageLevel = voltageLevel;
			UpdateControls();
		}

		public override void Update()
		{
			if (m_voltageSlider.IsSliding)
			{
				m_voltageLevel = (int)m_voltageSlider.Value;
			}
			if (m_okButton.IsClicked)
			{
				Dismiss(m_voltageLevel);
			}
			if (base.Input.Cancel || m_cancelButton.IsClicked)
			{
				Dismiss(null);
			}
			UpdateControls();
		}

		private void UpdateControls()
		{
			m_voltageSlider.Text = string.Format("{0:0.0}V ({1})", 1.5f * (float)m_voltageLevel / 15f, (m_voltageLevel < 8) ? "Low" : "High");
			m_voltageSlider.Value = m_voltageLevel;
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
