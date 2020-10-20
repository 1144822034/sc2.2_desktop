using Engine;
using System;
using System.IO;
using System.Xml.Linq;

namespace Game
{
	public class DownloadContentFromLinkDialog : Dialog
	{
		private TextBoxWidget m_linkTextBoxWidget;

		private TextBoxWidget m_nameTextBoxWidget;

		private RectangleWidget m_typeIconWidget;

		private LabelWidget m_typeLabelWidget;

		private ButtonWidget m_changeTypeButtonWidget;

		private ButtonWidget m_downloadButtonWidget;

		private ButtonWidget m_cancelButtonWidget;

		private bool m_updateContentName;

		private bool m_updateContentType;

		private ExternalContentType m_type;

		public DownloadContentFromLinkDialog()
		{
			XElement node = ContentManager.Get<XElement>("Dialogs/DownloadContentFromLinkDialog");
			LoadContents(this, node);
			m_linkTextBoxWidget = Children.Find<TextBoxWidget>("DownloadContentFromLinkDialog.Link");
			m_nameTextBoxWidget = Children.Find<TextBoxWidget>("DownloadContentFromLinkDialog.Name");
			m_typeIconWidget = Children.Find<RectangleWidget>("DownloadContentFromLinkDialog.TypeIcon");
			m_typeLabelWidget = Children.Find<LabelWidget>("DownloadContentFromLinkDialog.Type");
			m_changeTypeButtonWidget = Children.Find<ButtonWidget>("DownloadContentFromLinkDialog.ChangeType");
			m_downloadButtonWidget = Children.Find<ButtonWidget>("DownloadContentFromLinkDialog.Download");
			m_cancelButtonWidget = Children.Find<ButtonWidget>("DownloadContentFromLinkDialog.Cancel");
			m_linkTextBoxWidget.TextChanged += delegate
			{
				m_updateContentName = true;
				m_updateContentType = true;
			};
		}

		public override void Update()
		{
			string text = m_linkTextBoxWidget.Text.Trim();
			string name = m_nameTextBoxWidget.Text.Trim();
			m_typeLabelWidget.Text = ExternalContentManager.GetEntryTypeDescription(m_type);
			m_typeIconWidget.Subtexture = ExternalContentManager.GetEntryTypeIcon(m_type);
			if (ExternalContentManager.DoesEntryTypeRequireName(m_type))
			{
				m_nameTextBoxWidget.IsEnabled = true;
				m_downloadButtonWidget.IsEnabled = (text.Length > 0 && name.Length > 0 && m_type != ExternalContentType.Unknown);
				if (m_updateContentName)
				{
					m_nameTextBoxWidget.Text = GetNameFromLink(m_linkTextBoxWidget.Text);
					m_updateContentName = false;
				}
			}
			else
			{
				m_nameTextBoxWidget.IsEnabled = false;
				m_nameTextBoxWidget.Text = string.Empty;
				m_downloadButtonWidget.IsEnabled = (text.Length > 0 && m_type != ExternalContentType.Unknown);
			}
			if (m_updateContentType)
			{
				m_type = GetTypeFromLink(m_linkTextBoxWidget.Text);
				m_updateContentType = false;
			}
			if (m_changeTypeButtonWidget.IsClicked)
			{
				DialogsManager.ShowDialog(base.ParentWidget, new SelectExternalContentTypeDialog("Select Content Type", delegate(ExternalContentType item)
				{
					m_type = item;
					m_updateContentName = true;
				}));
			}
			else if (base.Input.Cancel || m_cancelButtonWidget.IsClicked)
			{
				DialogsManager.HideDialog(this);
			}
			else if (m_downloadButtonWidget.IsClicked)
			{
				CancellableBusyDialog busyDialog = new CancellableBusyDialog("Downloading", autoHideOnCancel: false);
				DialogsManager.ShowDialog(base.ParentWidget, busyDialog);
				WebManager.Get(text, null, null, busyDialog.Progress, delegate(byte[] data)
				{
					ExternalContentManager.ImportExternalContent(new MemoryStream(data), m_type, name, delegate
					{
						DialogsManager.HideDialog(busyDialog);
						DialogsManager.HideDialog(this);
					}, delegate(Exception error)
					{
						DialogsManager.HideDialog(busyDialog);
						DialogsManager.ShowDialog(base.ParentWidget, new MessageDialog("Error", error.Message, "OK", null, null));
					});
				}, delegate(Exception error)
				{
					DialogsManager.HideDialog(busyDialog);
					DialogsManager.ShowDialog(base.ParentWidget, new MessageDialog("Error", error.Message, "OK", null, null));
				});
			}
		}

		private static string UnclutterLink(string address)
		{
			try
			{
				string text = address;
				int num = text.IndexOf('&');
				if (num > 0)
				{
					text = text.Remove(num);
				}
				int num2 = text.IndexOf('?');
				if (num2 > 0)
				{
					text = text.Remove(num2);
				}
				return Uri.UnescapeDataString(text);
			}
			catch (Exception)
			{
				return string.Empty;
			}
		}

		private static string GetNameFromLink(string address)
		{
			try
			{
				return Storage.GetFileNameWithoutExtension(UnclutterLink(address));
			}
			catch (Exception)
			{
				return string.Empty;
			}
		}

		private static ExternalContentType GetTypeFromLink(string address)
		{
			try
			{
				return ExternalContentManager.ExtensionToType(Storage.GetExtension(UnclutterLink(address)));
			}
			catch (Exception)
			{
				return ExternalContentType.Unknown;
			}
		}
	}
}