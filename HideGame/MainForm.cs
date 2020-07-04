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
using MFGLib;

namespace HideGame
{
	public partial class MainForm : Form
	{
		static string[] ClassBlacklist = new string[] {
			"Progman", // 桌面
			"WorkerW", // 桌面
			"Shell_TrayWnd", // 任务栏		
		};

		const int MaxNameLength = 20;
		IntPtr m_prevTarget = IntPtr.Zero;
		IntPtr m_targetWnd = IntPtr.Zero;
		string m_targetName = null;		

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

			notifyIcon1.ShowBalloonTip(1500, Application.ProductName, "Press Ctrl-Alt-B to hide/restore foreground window.", ToolTipIcon.None);
			UpdateTrayIcon();

			autoStartToolStripMenuItem.Checked = RegistryHelper.CheckAutoStartApp(ProductName) != null;
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
				ToggleGame(false);
			}			
		}

		private void exitToolStripMenuItem_Click(object sender, EventArgs e)
		{
			Close();
		}

		private void ShowGame()
		{
			if (m_targetWnd == IntPtr.Zero)
			{
				return;
			}

			if (Window.IsWindow(m_targetWnd))
			{
				Window.ShowWindow(m_targetWnd, Window.SW_SHOW);
				Window.SetForegroundWindow(m_targetWnd);
			}

			m_prevTarget = m_targetWnd;
			m_targetWnd = IntPtr.Zero;
			m_targetName = null;			
			UpdateTrayIcon();
		}

		private void HideGame(bool foreground)
		{
			IntPtr hwnd;
			if (!foreground && m_prevTarget != IntPtr.Zero && Window.IsWindow(m_prevTarget))
			{
				hwnd = m_prevTarget;				
			}
			else
			{
				hwnd = Window.GetForegroundWindow();
			}
			
			if (hwnd == IntPtr.Zero || hwnd == Window.GetDesktopWindow())
			{
				return;
			}

			string className = Window.GetClassName(hwnd);
			if (Array.IndexOf(ClassBlacklist, className) != -1)
			{
				return;
			}

			m_targetWnd = hwnd;
			m_targetName = Window.GetWindowText(hwnd);
			if (m_targetName != null && m_targetName.Length > MaxNameLength)
			{
				m_targetName = m_targetName.Substring(0, MaxNameLength) + "...";
			}

			Window.ShowWindow(m_targetWnd, Window.SW_HIDE);			
			UpdateTrayIcon();
		}

		private void ToggleGame(bool foreground)
		{
			if (m_targetWnd == IntPtr.Zero)
			{
				HideGame(foreground);
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
			if (m_targetWnd == IntPtr.Zero)
			{
				notifyIcon1.Icon = iconEmpty;
				notifyIcon1.Text = Application.ProductName;
			}
			else
			{
				notifyIcon1.Icon = iconFull;
				notifyIcon1.Text = string.Format("{0} [{1}]", Application.ProductName, m_targetName);
			}			
		}

		protected override void WndProc(ref Message m)
		{
			int id = Hotkey.IsHotkeyEvent(ref m);
			if (id == 0)
			{
				ToggleGame(true);
			}

			base.WndProc(ref m);
		}

		private void autoStartToolStripMenuItem_Click(object sender, EventArgs e)
		{
			bool autoStart = RegistryHelper.CheckAutoStartApp(ProductName) != null;
			autoStart = !autoStart;
			autoStartToolStripMenuItem.Checked = autoStart;
			if (autoStart)
			{
				RegistryHelper.AddAutoStartApp(ProductName, Application.ExecutablePath);
			}
			else
			{
				RegistryHelper.RemoveAutoStartApp(ProductName);
			}
		}
	}
}
