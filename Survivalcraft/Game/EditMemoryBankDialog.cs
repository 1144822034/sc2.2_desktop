using System;
using System.Xml.Linq;

namespace Game
{
	public class EditMemoryBankDialog : Dialog
	{
		private Action m_handler;

		private Widget m_linearPanel;

		private Widget m_gridPanel;

		private ButtonWidget m_okButton;

		private ButtonWidget m_cancelButton;

		private ButtonWidget m_switchViewButton;

		private TextBoxWidget[] m_lineTextBoxes = new TextBoxWidget[16];

		private TextBoxWidget m_linearTextBox;

		private MemoryBankData m_memoryBankData;

		private MemoryBankData m_tmpMemoryBankData;

		private bool m_ignoreTextChanges;

		public EditMemoryBankDialog(MemoryBankData memoryBankData, Action handler)
		{
			XElement node = ContentManager.Get<XElement>("Dialogs/EditMemoryBankDialog");
			LoadContents(this, node);
			m_linearPanel = Children.Find<Widget>("EditMemoryBankDialog.LinearPanel");
			m_gridPanel = Children.Find<Widget>("EditMemoryBankDialog.GridPanel");
			m_okButton = Children.Find<ButtonWidget>("EditMemoryBankDialog.OK");
			m_cancelButton = Children.Find<ButtonWidget>("EditMemoryBankDialog.Cancel");
			m_switchViewButton = Children.Find<ButtonWidget>("EditMemoryBankDialog.SwitchViewButton");
			m_linearTextBox = Children.Find<TextBoxWidget>("EditMemoryBankDialog.LinearText");
			for (int i = 0; i < 16; i++)
			{
				m_lineTextBoxes[i] = Children.Find<TextBoxWidget>("EditMemoryBankDialog.Line" + i.ToString());
			}
			m_handler = handler;
			m_memoryBankData = memoryBankData;
			m_tmpMemoryBankData = (MemoryBankData)m_memoryBankData.Copy();
			m_linearPanel.IsVisible = false;
			for (int j = 0; j < 16; j++)
			{
				m_lineTextBoxes[j].TextChanged += TextBox_TextChanged;
			}
			m_linearTextBox.TextChanged += TextBox_TextChanged;
		}

		private void TextBox_TextChanged(TextBoxWidget textBox)
		{
			if (m_ignoreTextChanges)
			{
				return;
			}
			if (textBox == m_linearTextBox)
			{
				m_tmpMemoryBankData = new MemoryBankData();
				m_tmpMemoryBankData.LoadString(m_linearTextBox.Text);
				return;
			}
			string text = string.Empty;
			for (int i = 0; i < 16; i++)
			{
				text += m_lineTextBoxes[i].Text;
			}
			m_tmpMemoryBankData = new MemoryBankData();
			m_tmpMemoryBankData.LoadString(text);
		}

		public override void Update()
		{
			m_ignoreTextChanges = true;
			try
			{
				string text = m_tmpMemoryBankData.SaveString(saveLastOutput: false);
				if (text.Length < 256)
				{
					text += new string('0', 256 - text.Length);
				}
				for (int i = 0; i < 16; i++)
				{
					m_lineTextBoxes[i].Text = text.Substring(i * 16, 16);
				}
				m_linearTextBox.Text = m_tmpMemoryBankData.SaveString(saveLastOutput: false);
			}
			finally
			{
				m_ignoreTextChanges = false;
			}
			if (m_linearPanel.IsVisible)
			{
				m_switchViewButton.Text = "Grid";
				if (m_switchViewButton.IsClicked)
				{
					m_linearPanel.IsVisible = false;
					m_gridPanel.IsVisible = true;
				}
			}
			else
			{
				m_switchViewButton.Text = "Linear";
				if (m_switchViewButton.IsClicked)
				{
					m_linearPanel.IsVisible = true;
					m_gridPanel.IsVisible = false;
				}
			}
			if (m_okButton.IsClicked)
			{
				m_memoryBankData.Data = m_tmpMemoryBankData.Data;
				Dismiss(result: true);
			}
			if (base.Input.Cancel || m_cancelButton.IsClicked)
			{
				Dismiss(result: false);
			}
		}

		private void Dismiss(bool result)
		{
			DialogsManager.HideDialog(this);
			if (m_handler != null && result)
			{
				m_handler();
			}
		}
	}
}
