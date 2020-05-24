using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Win32API;

namespace HideGame
{
	public partial class MainForm : Form
	{
		const int MaxNameLength = 12;
		IntPtr m_wndHidden = IntPtr.Zero;
		string m_nameHidden = null;

		public MainForm()
		{
			InitializeComponent();
		}

		private void MainForm_Load(object sender, EventArgs e)
		{
			Hide();
			ShowInTaskbar = false;

			if (!Hotkey.RegisterHotKey(Handle, 0, Keys.B, ModKeys.Control | ModKeys.Alt))
			{
				MessageBox.Show(this, "Cannot register hotkey Ctrl-Alt-B", Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
			}

			//notifyIcon1.ShowBalloonTip(1500, Application.ProductName, "Press Ctrl-Alt-B to hide/show games.", ToolTipIcon.None);
			UpdateTrayIcon();
		}

		private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
		{
			ShowGame();
		}

		private void notifyIcon1_Click(object sender, EventArgs e)
		{
			MouseEventArgs me = e as MouseEventArgs;
			if (me.Button == MouseButtons.Left)
			{
				ShowGame();
			}			
		}

		private void exitToolStripMenuItem_Click(object sender, EventArgs e)
		{
			Close();
		}

		private void ShowGame()
		{
			if (m_wndHidden == IntPtr.Zero)
			{
				return;
			}

			if (Window.IsWindow(m_wndHidden))
			{
				Window.ShowWindow(m_wndHidden, Window.SW_SHOW);
				Window.SetForegroundWindow(m_wndHidden);
			}

			m_wndHidden = IntPtr.Zero;
			m_nameHidden = null;
			UpdateTrayIcon();
		}

		private void HideGame()
		{
			IntPtr hwnd = Window.GetForegroundWindow();
			if (hwnd == IntPtr.Zero || hwnd == Window.GetDesktopWindow())
			{
				return;
			}

			m_wndHidden = hwnd;
			m_nameHidden = Window.GetWindowText(hwnd);
			if (m_nameHidden != null && m_nameHidden.Length > MaxNameLength)
			{
				m_nameHidden = m_nameHidden.Substring(0, MaxNameLength) + "...";
			}

			Window.ShowWindow(m_wndHidden, Window.SW_HIDE);			
			UpdateTrayIcon();
		}

		private void ToggleGame()
		{
			if (m_wndHidden == IntPtr.Zero)
			{
				HideGame();
			}
			else
			{
				ShowGame();
			}
		}

		Icon iconEmpty = Properties.Resources.empty;
		Icon iconFull = Properties.Resources.full;

		private void UpdateTrayIcon()
		{
			if (m_wndHidden == IntPtr.Zero)
			{
				notifyIcon1.Icon = iconEmpty;
				notifyIcon1.Text = Application.ProductName + " - Press Ctrl-Alt-B to hide foreground window.";
			}
			else
			{
				notifyIcon1.Icon = iconFull;
				notifyIcon1.Text = string.Format("{0} - Click to restore [{1}].", Application.ProductName, m_nameHidden);
			}			
		}

		protected override void WndProc(ref Message m)
		{
			int id = Hotkey.IsHotkeyEvent(ref m);
			if (id == 0)
			{
				ToggleGame();
			}

			base.WndProc(ref m);
		}		
	}
}
