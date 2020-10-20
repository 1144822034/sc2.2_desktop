using Engine;
using System;
using System.Xml.Linq;

namespace Game
{
	public class EditAdjustableDelayGateDialog : Dialog
	{
		private Action<int> m_handler;

		private SliderWidget m_delaySlider;

		private ButtonWidget m_plusButton;

		private ButtonWidget m_minusButton;

		private LabelWidget m_delayLabel;

		private ButtonWidget m_okButton;

		private ButtonWidget m_cancelButton;

		private int m_delay;

		public EditAdjustableDelayGateDialog(int delay, Action<int> handler)
		{
			XElement node = ContentManager.Get<XElement>("Dialogs/EditAdjustableDelayGateDialog");
			LoadContents(this, node);
			m_delaySlider = Children.Find<SliderWidget>("EditAdjustableDelayGateDialog.DelaySlider");
			m_plusButton = Children.Find<ButtonWidget>("EditAdjustableDelayGateDialog.PlusButton");
			m_minusButton = Children.Find<ButtonWidget>("EditAdjustableDelayGateDialog.MinusButton");
			m_delayLabel = Children.Find<LabelWidget>("EditAdjustableDelayGateDialog.Label");
			m_okButton = Children.Find<ButtonWidget>("EditAdjustableDelayGateDialog.OK");
			m_cancelButton = Children.Find<ButtonWidget>("EditAdjustableDelayGateDialog.Cancel");
			m_handler = handler;
			m_delay = delay;
			UpdateControls();
		}

		public override void Update()
		{
			if (m_delaySlider.IsSliding)
			{
				m_delay = (int)m_delaySlider.Value;
			}
			if (m_minusButton.IsClicked)
			{
				m_delay = MathUtils.Max(m_delay - 1, (int)m_delaySlider.MinValue);
			}
			if (m_plusButton.IsClicked)
			{
				m_delay = MathUtils.Min(m_delay + 1, (int)m_delaySlider.MaxValue);
			}
			if (m_okButton.IsClicked)
			{
				Dismiss(m_delay);
			}
			if (base.Input.Cancel || m_cancelButton.IsClicked)
			{
				Dismiss(null);
			}
			UpdateControls();
		}

		private void UpdateControls()
		{
			m_delaySlider.Value = m_delay;
			m_minusButton.IsEnabled = ((float)m_delay > m_delaySlider.MinValue);
			m_plusButton.IsEnabled = ((float)m_delay < m_delaySlider.MaxValue);
			m_delayLabel.Text = $"{(float)(m_delay + 1) * 0.01f:0.00} seconds";
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
