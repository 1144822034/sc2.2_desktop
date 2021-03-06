using System.Xml.Linq;

namespace Game
{
	public class RateCommunityContentDialog : Dialog
	{
		private string m_address;

		private string m_displayName;

		private string m_userId;

		private LabelWidget m_nameLabel;

		private StarRatingWidget m_starRating;

		private ButtonWidget m_rateButton;

		private LinkWidget m_reportLink;

		private ButtonWidget m_cancelButton;

		public RateCommunityContentDialog(string address, string displayName, string userId)
		{
			m_address = address;
			m_displayName = displayName;
			m_userId = userId;
			XElement node = ContentManager.Get<XElement>("Dialogs/RateCommunityContentDialog");
			LoadContents(this, node);
			m_nameLabel = Children.Find<LabelWidget>("RateCommunityContentDialog.Name");
			m_starRating = Children.Find<StarRatingWidget>("RateCommunityContentDialog.StarRating");
			m_rateButton = Children.Find<ButtonWidget>("RateCommunityContentDialog.Rate");
			m_reportLink = Children.Find<LinkWidget>("RateCommunityContentDialog.Report");
			m_cancelButton = Children.Find<ButtonWidget>("RateCommunityContentDialog.Cancel");
			m_nameLabel.Text = displayName;
			m_rateButton.IsEnabled = false;
		}

		public override void Update()
		{
			m_rateButton.IsEnabled = (m_starRating.Rating != 0f);
			if (m_rateButton.IsClicked)
			{
				DialogsManager.HideDialog(this);
				CancellableBusyDialog busyDialog = new CancellableBusyDialog("Sending Rating", autoHideOnCancel: false);
				DialogsManager.ShowDialog(base.ParentWidget, busyDialog);
				CommunityContentManager.Rate(m_address, m_userId, (int)m_starRating.Rating, busyDialog.Progress, delegate
				{
					DialogsManager.HideDialog(busyDialog);
				}, delegate
				{
					DialogsManager.HideDialog(busyDialog);
				});
			}
			if (m_reportLink.IsClicked && UserManager.ActiveUser != null)
			{
				DialogsManager.HideDialog(this);
				DialogsManager.ShowDialog(base.ParentWidget, new ReportCommunityContentDialog(m_address, m_displayName, UserManager.ActiveUser.UniqueId));
			}
			if (base.Input.Cancel || m_cancelButton.IsClicked)
			{
				DialogsManager.HideDialog(this);
			}
		}
	}
}
