namespace UltimateFishBot
{
    using System;
    using System.Drawing;
    using System.Net;
    using System.Reflection;
    using System.Threading.Tasks;
    using System.Windows.Forms;

    using UltimateFishBot.Classes;
    using UltimateFishBot.Classes.Helpers;
    using UltimateFishBot.Forms;
    using UltimateFishBot.Properties;

    public partial class frmMain : Form
    {
        private static readonly int WM_HOTKEY = 0x0312;

        private readonly Manager m_manager;

        public frmMain()
        {
            this.InitializeComponent();

            this.m_manager = new Manager(this);
        }

        public enum HotKey
        {
            StartStop = 0
        }

        public enum KeyModifier
        {
            None = 0, 

            Alt = 1, 

            Control = 2, 

            Shift = 4
        }

        public void ReloadHotkeys()
        {
            this.UnregisterHotKeys();

            foreach (var hotKey in (HotKey[])Enum.GetValues(typeof(HotKey)))
            {
                var key = Keys.None;

                switch (hotKey)
                {
                    case HotKey.StartStop:
                        key = Settings.Default.StartStopHotKey;
                        break;
                    default:
                        continue;
                }

                var modifiers = this.RemoveAndReturnModifiers(ref key);
                Win32.RegisterHotKey(this.Handle, (int)hotKey, (int)modifiers, (int)key);
            }
        }

        public void StopFishing()
        {
            this.btnStop_Click(null, null);
        }

        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);

            if (m.Msg == WM_HOTKEY)
            {
                var key = (Keys)(((int)m.LParam >> 16) & 0xFFFF); // The key of the hotkey that was pressed.
                var modifier = (KeyModifier)((int)m.LParam & 0xFFFF); // The modifier of the hotkey that was pressed.
                var id = m.WParam.ToInt32(); // The id of the hotkey that was pressed.

                if (id == (int)HotKey.StartStop)
                {
                    if (this.m_manager.IsStoppedOrPaused()) this.btnStart_Click(null, null);
                    else this.btnStop_Click(null, null);
                }
            }
        }

        private void btnAbout_Click(object sender, EventArgs e)
        {
            about.GetForm.Show();
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnHowTo_Click(object sender, EventArgs e)
        {
            frmDirections.GetForm.Show();
        }

        private void btnSettings_Click(object sender, EventArgs e)
        {
            frmSettings.GetForm(this).Show();
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            this.btnSettings.Enabled = false;
            this.btnStop.Enabled = true;

            if (this.m_manager.GetActualState() == Manager.FishingState.Stopped)
            {
                this.m_manager.Start();
                this.btnStart.Text = Translate.GetTranslate("frmMain", "BUTTON_PAUSE");
                this.lblStatus.Text = Translate.GetTranslate("frmMain", "LABEL_STARTED");
                this.lblStatus.Image = Resources.online;
            }
            else if (this.m_manager.GetActualState() == Manager.FishingState.Paused)
            {
                this.m_manager.Resume();
                this.btnStart.Text = Translate.GetTranslate("frmMain", "BUTTON_PAUSE");
                this.lblStatus.Text = Translate.GetTranslate("frmMain", "LABEL_RESUMED");
                this.lblStatus.Image = Resources.online;
            }
            else
            {
                this.btnSettings.Enabled = true;
                this.m_manager.Pause();
                this.btnStart.Text = Translate.GetTranslate("frmMain", "BUTTON_RESUME");
                this.lblStatus.Text = Translate.GetTranslate("frmMain", "LABEL_PAUSED");
                this.lblStatus.Image = Resources.online;
            }
        }

        private void btnStatistics_Click(object sender, EventArgs e)
        {
            frmStats.GetForm(this.m_manager).Show();
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            this.btnSettings.Enabled = true;
            this.btnStop.Enabled = false;
            this.m_manager.Stop();
            this.btnStart.Text = Translate.GetTranslate("frmMain", "BUTTON_START");
            this.lblStatus.Text = Translate.GetTranslate("frmMain", "LABEL_STOPPED");
            this.lblStatus.Image = Resources.offline;
        }

        private void CheckStatus()
        {
            this.lblWarn.Text = Translate.GetTranslate("frmMain", "LABEL_CHECKING_STATUS");
            this.lblWarn.Parent = this.PictureBox1;

            try
            {
                Task.Factory.StartNew(() => new WebClient().DownloadString("http://www.fishbot.net/status.txt"), TaskCreationOptions.LongRunning)
                    .ContinueWith(
                        x =>
                            {
                                if (x.Result.ToLower().Trim() != "safe")
                                {
                                    this.lblWarn.Text = Translate.GetTranslate("frmMain", "LABEL_NO_LONGER_SAFE");
                                    this.lblWarn.ForeColor = Color.Red;
                                    this.lblWarn.BackColor = Color.Black;
                                }
                                else this.lblWarn.Visible = false;
                            }, 
                        TaskScheduler.FromCurrentSynchronizationContext());
            }
            catch (Exception ex)
            {
                this.lblWarn.Text = Translate.GetTranslate("frmMain", "LABEL_COULD_NOT_CHECK_STATUS") + ex;
            }
        }

        private void frmMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            this.UnregisterHotKeys();
        }

        private void frmMain_Load(object sender, EventArgs e)
        {
            this.btnStart.Text = Translate.GetTranslate("frmMain", "BUTTON_START");
            this.btnStop.Text = Translate.GetTranslate("frmMain", "BUTTON_STOP");
            this.btnSettings.Text = Translate.GetTranslate("frmMain", "BUTTON_SETTINGS");
            this.btnStatistics.Text = Translate.GetTranslate("frmMain", "BUTTON_STATISTICS");
            this.btnHowTo.Text = Translate.GetTranslate("frmMain", "BUTTON_HTU");
            this.btnClose.Text = Translate.GetTranslate("frmMain", "BUTTON_EXIT");
            this.btnAbout.Text = Translate.GetTranslate("frmMain", "BUTTON_ABOUT");
            this.lblStatus.Text = Translate.GetTranslate("frmMain", "LABEL_STOPPED");
            this.Text = "UltimateFishBot - v " + Assembly.GetExecutingAssembly().GetName().Version;
            this.ReloadHotkeys();
            this.CheckStatus();
        }

        private KeyModifier RemoveAndReturnModifier(ref Keys key, Keys keyModifier, KeyModifier modifier)
        {
            if ((key & keyModifier) != 0)
            {
                key &= ~keyModifier;
                return modifier;
            }

            return KeyModifier.None;
        }

        private KeyModifier RemoveAndReturnModifiers(ref Keys key)
        {
            var modifiers = KeyModifier.None;

            modifiers |= this.RemoveAndReturnModifier(ref key, Keys.Shift, KeyModifier.Shift);
            modifiers |= this.RemoveAndReturnModifier(ref key, Keys.Control, KeyModifier.Control);
            modifiers |= this.RemoveAndReturnModifier(ref key, Keys.Alt, KeyModifier.Alt);

            return modifiers;
        }

        private void UnregisterHotKeys()
        {
            // Unregister all hotkeys before closing the form.
            foreach (var hotKey in (HotKey[])Enum.GetValues(typeof(HotKey))) Win32.UnregisterHotKey(this.Handle, (int)hotKey);
        }
    }
}