using System.Xml.Linq;

namespace Game
{
	public class CancellableBusyDialog : Dialog
	{
		private LabelWidget m_largeLabelWidget;

		private LabelWidget m_smallLabelWidget;

		private ButtonWidget m_cancelButtonWidget;

		private bool m_autoHideOnCancel;

		public CancellableProgress Progress
		{
			get;
			private set;
		}

		public string LargeMessage
		{
			get
			{
				return m_largeLabelWidget.Text;
			}
			set
			{
				m_largeLabelWidget.Text = (value ?? string.Empty);
				m_largeLabelWidget.IsVisible = !string.IsNullOrEmpty(value);
			}
		}

		private string SmallMessage
		{
			get
			{
				return m_smallLabelWidget.Text;
			}
			set
			{
				m_smallLabelWidget.Text = (value ?? string.Empty);
			}
		}

		private bool IsCancelButtonEnabled
		{
			get
			{
				return m_cancelButtonWidget.IsEnabled;
			}
			set
			{
				m_cancelButtonWidget.IsEnabled = value;
			}
		}

		public CancellableBusyDialog(string largeMessage, bool autoHideOnCancel)
		{
			XElement node = ContentManager.Get<XElement>("Dialogs/CancellableBusyDialog");
			LoadContents(this, node);
			m_largeLabelWidget = Children.Find<LabelWidget>("CancellableBusyDialog.LargeLabel");
			m_smallLabelWidget = Children.Find<LabelWidget>("CancellableBusyDialog.SmallLabel");
			m_cancelButtonWidget = Children.Find<ButtonWidget>("CancellableBusyDialog.CancelButton");
			Progress = new CancellableProgress();
			m_autoHideOnCancel = autoHideOnCancel;
			LargeMessage = largeMessage;
		}

		public override void Update()
		{
			if (Progress.Completed > 0f && Progress.Total > 0f)
			{
				SmallMessage = $"{Progress.Completed / Progress.Total * 100f:0}%";
			}
			else
			{
				SmallMessage = string.Empty;
			}
			if (m_cancelButtonWidget.IsClicked)
			{
				Progress.Cancel();
				if (m_autoHideOnCancel)
				{
					DialogsManager.HideDialog(this);
				}
			}
			if (base.Input.Cancel)
			{
				base.Input.Clear();
			}
		}
	}
}
