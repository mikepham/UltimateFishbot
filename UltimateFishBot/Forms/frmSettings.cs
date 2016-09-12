namespace UltimateFishBot.Forms
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading;
    using System.Windows.Forms;

    using CoreAudioApi;

    using UltimateFishBot.Classes;
    using UltimateFishBot.Properties;

    public partial class frmSettings : Form
    {
        private static frmSettings inst;

        private Keys m_hotkey;

        private readonly frmMain m_mainForm;

        private MMDevice m_SndDevice;

        public frmSettings(frmMain mainForm)
        {
            this.InitializeComponent();
            this.m_mainForm = mainForm;
            this.m_SndDevice = null;

            this.tmeAudio.Tick += this.tmeAudio_Tick;
        }

        private enum TabulationIndex
        {
            GeneralFishing = 0, 

            FindCursor = 1, 

            HearingFishing = 2, 

            Premium = 3, 

            AntiAfk = 4, 

            Language = 5
        }

        public static frmSettings GetForm(frmMain main)
        {
            if (inst == null || inst.IsDisposed) inst = new frmSettings(main);
            return inst;
        }

        private void btnReset_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show(
                Translate.GetTranslate("frmSettings", "RESET_MESSAGE"), 
                Translate.GetTranslate("frmSettings", "RESET_TITLE"), 
                MessageBoxButtons.OKCancel) == DialogResult.OK)
            {
                Settings.Default.Reset();
                Application.Restart();
            }
        }

        private void btnSetScanArea_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show(
                Translate.GetTranslate("frmSettings", "SCAN_MESSAGE"), 
                Translate.GetTranslate("frmSettings", "SCAN_TITLE"), 
                MessageBoxButtons.OKCancel) == DialogResult.OK)
            {
                frmOverlay.GetForm(this).Show();
            }
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void buttonSave_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Some changes may start working only after application restart.");

            /// General
            Settings.Default.CastingDelay = int.Parse(this.txtCastDelay.Text);
            Settings.Default.LootingDelay = int.Parse(this.txtLootingDelay.Text);
            Settings.Default.FishWait = int.Parse(this.txtFishWait.Text);

            /// Finding the Cursor
            Settings.Default.ScanningDelay = int.Parse(this.txtDelay.Text);
            Settings.Default.ScanningRetries = int.Parse(this.txtRetries.Text);
            Settings.Default.ScanningSteps = int.Parse(this.txtScanSteps.Text);
            Settings.Default.CheckCursor = this.cmbCompareIcon.Checked;
            Settings.Default.AlternativeRoute = this.cmbAlternativeRoute.Checked;
            Settings.Default.customScanArea = this.customAreaCheckbox.Checked;

            /// Hearing the Fish
            Settings.Default.SplashLimit = int.Parse(this.txtSplash.Text);
            Settings.Default.AudioDevice = (string)this.cmbAudio.SelectedValue;
            Settings.Default.AverageSound = this.cbSoundAvg.Checked;

            /// Premium Settings

            Settings.Default.ProcName = this.txtProcName.Text;
            Settings.Default.AutoLure = this.cbAutoLure.Checked;
            Settings.Default.SwapGear = this.cbHearth.Checked;
            Settings.Default.UseAltKey = this.cbAlt.Checked;

            Settings.Default.FishKey = this.txtFishKey.Text;
            Settings.Default.LureKey = this.txtLureKey.Text;
            Settings.Default.HearthKey = this.txtHearthKey.Text;
            Settings.Default.AutoHearth = this.cbHearth.Checked;

            // MoP Premium (Angler's Raft & Ancient Pandaren Fishing Charm)
            Settings.Default.CharmKey = this.txtCharmKey.Text;
            Settings.Default.RaftKey = this.txtRaftKey.Text;
            Settings.Default.AutoRaft = this.cbApplyRaft.Checked;
            Settings.Default.AutoCharm = this.cbApplyCharm.Checked;
            Settings.Default.ShiftLoot = this.cbShiftLoot.Checked;

            // WoD Premium (Bait)
            Settings.Default.BaitKey1 = this.txtBaitKey1.Text;
            Settings.Default.BaitKey2 = this.txtBaitKey2.Text;
            Settings.Default.BaitKey3 = this.txtBaitKey3.Text;
            Settings.Default.BaitKey4 = this.txtBaitKey4.Text;
            Settings.Default.BaitKey5 = this.txtBaitKey5.Text;
            Settings.Default.BaitKey6 = this.txtBaitKey6.Text;
            Settings.Default.BaitKey7 = this.txtBaitKey7.Text;
            Settings.Default.AutoBait = this.cbAutoBait.Checked;
            Settings.Default.CycleThroughBaitList = this.cbCycleThroughBaitList.Checked;

            this.SaveHotKeys();

            // Times
            Settings.Default.LureTime = int.Parse(this.txtLureTime.Text);
            Settings.Default.HearthTime = int.Parse(this.txtHearthTime.Text);
            Settings.Default.RaftTime = int.Parse(this.txtRaftTime.Text);
            Settings.Default.CharmTime = int.Parse(this.txtCharmTime.Text);
            Settings.Default.BaitTime = int.Parse(this.txtBaitTime.Text);

            /// Anti Afk
            Settings.Default.AntiAfk = this.cbAntiAfk.Checked;
            Settings.Default.AntiAfkTime = int.Parse(this.txtAntiAfkTimer.Text);
            Settings.Default.AntiAfkMoves = this.cmbMovements.SelectedIndex;

            /// Language
            Settings.Default.Txt2speech = this.chkTxt2speech.Checked;
            if ((string)this.cmbLanguage.SelectedItem != Settings.Default.Language)
            {
                Settings.Default.Language = (string)this.cmbLanguage.SelectedItem;
                Settings.Default.Save();

                MessageBox.Show(Translate.GetTranslate("frmSettings", "LABEL_LANGUAGE_CHANGE"), Translate.GetTranslate("frmSettings", "TITLE_LANGUAGE_CHANGE"));

                Thread.Sleep(1000);
                Application.Restart();
            }
            else
            {
                Settings.Default.Save();
                this.Close();
            }
        }

        private void cmbAudio_SelectedIndexChanged(object sender, EventArgs e)
        {
            var sndDevEnum = new MMDeviceEnumerator();

            if (!string.IsNullOrEmpty((string)this.cmbAudio.SelectedValue)) this.m_SndDevice = sndDevEnum.GetDevice((string)this.cmbAudio.SelectedValue);
            else this.m_SndDevice = sndDevEnum.GetDefaultAudioEndpoint(EDataFlow.eRender, ERole.eMultimedia);
        }

        private void customAreaCheckbox_CheckedChanged(object sender, EventArgs e)
        {
            if (this.customAreaCheckbox.Checked)
            {
                this.btnSetScanArea.Enabled = true;
                this.txtMinXY.Enabled = true;
                this.txtMaxXY.Enabled = true;
            }
            else
            {
                this.btnSetScanArea.Enabled = false;
                this.txtMinXY.Enabled = false;
                this.txtMaxXY.Enabled = false;
            }
        }

        private void frmSettings_Load(object sender, EventArgs e)
        {
            /*
             * Set Text from translate file
             */
            this.Text = Translate.GetTranslate("frmSettings", "TITLE");

            this.tabSettings.TabPages[(int)TabulationIndex.GeneralFishing].Text = Translate.GetTranslate("frmSettings", "TAB_TITLE_GENERAL_FISHING");
            this.tabSettings.TabPages[(int)TabulationIndex.FindCursor].Text = Translate.GetTranslate("frmSettings", "TAB_TITLE_FIND_CURSOR");
            this.tabSettings.TabPages[(int)TabulationIndex.HearingFishing].Text = Translate.GetTranslate("frmSettings", "TAB_TITLE_HEARING_FISH");
            this.tabSettings.TabPages[(int)TabulationIndex.Premium].Text = Translate.GetTranslate("frmSettings", "TAB_TITLE_PREMIUM");
            this.tabSettings.TabPages[(int)TabulationIndex.AntiAfk].Text = Translate.GetTranslate("frmSettings", "TAB_TITLE_ANTI_AFK");
            this.tabSettings.TabPages[(int)TabulationIndex.Language].Text = Translate.GetTranslate("frmSettings", "TAB_TITLE_LANGUAGE");

            /// General

            this.LabelDelayCast.Text = Translate.GetTranslate("frmSettings", "LABEL_DELAY_AFTER_CAST");
            this.LabelDelayCastDesc.Text = Translate.GetTranslate("frmSettings", "LABEL_DELAY_AFTER_CAST_DESC");

            this.LabelFishWait.Text = Translate.GetTranslate("frmSettings", "LABEL_FISH_WAIT_LIMIT");
            this.LabelFishWaitDesc.Text = Translate.GetTranslate("frmSettings", "LABEL_FISH_WAIT_LIMIT_DESC");

            this.LabelDelayLooting.Text = Translate.GetTranslate("frmSettings", "LABEL_DELAY_AFTER_LOOTING");
            this.LabelDelayLootingDesc.Text = Translate.GetTranslate("frmSettings", "LABEL_DELAY_AFTER_LOOTING_DESC");

            /// Finding the Cursor

            this.LabelScanningSteps.Text = Translate.GetTranslate("frmSettings", "LABEL_SCANNING_STEPS");
            this.LabelScanningStepsDesc.Text = Translate.GetTranslate("frmSettings", "LABEL_SCANNING_STEPS_DESC");

            this.LabelScanningDelay.Text = Translate.GetTranslate("frmSettings", "LABEL_SCANNING_DELAY");
            this.LabelScanningDelayDesc.Text = Translate.GetTranslate("frmSettings", "LABEL_SCANNING_DELAY_DESC");

            this.LabelScanningRetries.Text = Translate.GetTranslate("frmSettings", "LABEL_SCANNING_RETRIES");
            this.LabelScanningRetriesDesc.Text = Translate.GetTranslate("frmSettings", "LABEL_SCANNING_RETRIES_DESC");

            this.cmbCompareIcon.Text = Translate.GetTranslate("frmSettings", "LABEL_CHECK_ICON");
            this.LabelCheckCursorIcon.Text = Translate.GetTranslate("frmSettings", "LABEL_CHECK_ICON_DESC");

            this.cmbAlternativeRoute.Text = Translate.GetTranslate("frmSettings", "LABEL_ALTERNATIVE_ROUTE");
            this.LabelAlternativeRoute.Text = Translate.GetTranslate("frmSettings", "LABEL_ALTERNATIVE_ROUTE_DESC");

            this.LabelScanArea.Text = Translate.GetTranslate("frmSettings", "LABEL_SCAN_AREA");
            this.btnSetScanArea.Text = Translate.GetTranslate("frmSettings", "SET_SCANNING_AREA");
            this.LabelMinXY.Text = Translate.GetTranslate("frmSettings", "START_XY");
            this.LabelMaxXY.Text = Translate.GetTranslate("frmSettings", "END_XY");

            /// Hearing the Fish

            this.LabelSplashThreshold.Text = Translate.GetTranslate("frmSettings", "LABEL_SPLASH_THRESHOLD");
            this.LabelSplashThresholdDesc.Text = Translate.GetTranslate("frmSettings", "LABEL_SPLASH_THRESHOLD_DESC");

            this.LabelAudioDevice.Text = Translate.GetTranslate("frmSettings", "LABEL_AUDIO_DEVICE");
            this.LabelAudioDeviceDesc.Text = Translate.GetTranslate("frmSettings", "LABEL_AUDIO_DEVICE_DESC");

            this.cbSoundAvg.Text = Translate.GetTranslate("frmSettings", "CB_AVG_SND");

            /// Premium Settings

            this.LabelCastKey.Text = Translate.GetTranslate("frmSettings", "LABEL_CAST_KEY");
            this.LabelLureKey.Text = Translate.GetTranslate("frmSettings", "LABEL_LURE_KEY");
            this.LabelHearthKey.Text = Translate.GetTranslate("frmSettings", "LABEL_HEARTHSTONE_KEY");
            this.LabelRaftKey.Text = Translate.GetTranslate("frmSettings", "LABEL_RAFT_KEY");
            this.LabelCharmKey.Text = Translate.GetTranslate("frmSettings", "LABEL_CHARM_KEY");
            this.LabelBaitKey1.Text = Translate.GetTranslate("frmSettings", "LABEL_BAIT_KEY_1");
            this.LabelBaitKey2.Text = Translate.GetTranslate("frmSettings", "LABEL_BAIT_KEY_2");
            this.LabelBaitKey3.Text = Translate.GetTranslate("frmSettings", "LABEL_BAIT_KEY_3");
            this.LabelBaitKey4.Text = Translate.GetTranslate("frmSettings", "LABEL_BAIT_KEY_4");
            this.LabelBaitKey5.Text = Translate.GetTranslate("frmSettings", "LABEL_BAIT_KEY_5");
            this.LabelBaitKey6.Text = Translate.GetTranslate("frmSettings", "LABEL_BAIT_KEY_6");
            this.LabelBaitKey7.Text = Translate.GetTranslate("frmSettings", "LABEL_BAIT_KEY_7");

            this.LoadHotKeys();

            this.LabelCustomizeDesc.Text = Translate.GetTranslate("frmSettings", "LABEL_CUSTOMIZE_DESC");

            this.cbAlt.Text = Translate.GetTranslate("frmSettings", "CB_ALT_KEY");
            this.cbAutoLure.Text = Translate.GetTranslate("frmSettings", "CB_AUTO_LURE");
            this.cbHearth.Text = Translate.GetTranslate("frmSettings", "CB_AUTO_HEARTHSTONE");
            this.cbApplyRaft.Text = Translate.GetTranslate("frmSettings", "CB_AUTO_RAFT");
            this.cbApplyCharm.Text = Translate.GetTranslate("frmSettings", "CB_AUTO_CHARM");
            this.cbAutoBait.Text = Translate.GetTranslate("frmSettings", "CB_AUTO_BAIT");
            this.cbCycleThroughBaitList.Text = Translate.GetTranslate("frmSettings", "CB_CYCLE_THROUGH_BAIT_LIST");
            this.cbShiftLoot.Text = Translate.GetTranslate("frmSettings", "CB_SHIFT_LOOT");

            this.LabelProcessName.Text = Translate.GetTranslate("frmSettings", "LABEL_PROCESS_NAME");
            this.LabelProcessNameDesc.Text = Translate.GetTranslate("frmSettings", "LABEL_PROCESS_NAME_DESC");

            /// Anti Afk

            this.LoadAntiAfkMovements();
            this.cbAntiAfk.Text = Translate.GetTranslate("frmSettings", "CB_ANTI_AFK");

            /// Language Settings

            this.labelLanguage.Text = Translate.GetTranslate("frmSettings", "LABEL_LANGUAGE");
            this.labelLanguageDesc.Text = Translate.GetTranslate("frmSettings", "LABEL_LANGUAGE_DESC");

            /// Buttons

            this.buttonSave.Text = Translate.GetTranslate("frmSettings", "BUTTON_SAVE");
            this.buttonCancel.Text = Translate.GetTranslate("frmSettings", "BUTTON_CANCEL");

            /*
             * Set Settings from save
             */

            /// General
            this.txtCastDelay.Text = Settings.Default.CastingDelay.ToString();
            this.txtLootingDelay.Text = Settings.Default.LootingDelay.ToString();
            this.txtFishWait.Text = Settings.Default.FishWait.ToString();

            /// Finding the Cursor
            this.txtDelay.Text = Settings.Default.ScanningDelay.ToString();
            this.txtRetries.Text = Settings.Default.ScanningRetries.ToString();
            this.txtScanSteps.Text = Settings.Default.ScanningSteps.ToString();
            this.cmbCompareIcon.Checked = Settings.Default.CheckCursor;
            this.cmbAlternativeRoute.Checked = Settings.Default.AlternativeRoute;
            this.customAreaCheckbox.Checked = Settings.Default.customScanArea;
            this.txtMinXY.Text = Settings.Default.minScanXY.ToString();
            this.txtMaxXY.Text = Settings.Default.maxScanXY.ToString();

            /// Hearing the Fish
            this.txtSplash.Text = Settings.Default.SplashLimit.ToString();
            this.LoadAudioDevices();
            this.cbSoundAvg.Checked = Settings.Default.AverageSound;

            /// Premium Settings
            this.txtProcName.Text = Settings.Default.ProcName;
            this.cbAutoLure.Checked = Settings.Default.AutoLure;
            this.cbHearth.Checked = Settings.Default.SwapGear;
            this.cbAlt.Checked = Settings.Default.UseAltKey;

            this.txtFishKey.Text = Settings.Default.FishKey;
            this.txtLureKey.Text = Settings.Default.LureKey;
            this.txtHearthKey.Text = Settings.Default.HearthKey;
            this.cbHearth.Checked = Settings.Default.AutoHearth;
            this.txtHearthTime.Text = Settings.Default.HearthTime.ToString();

            // MoP Premium (Angler's Raft & Ancient Pandaren Fishing Charm)
            this.txtCharmKey.Text = Settings.Default.CharmKey;
            this.txtRaftKey.Text = Settings.Default.RaftKey;
            this.cbApplyRaft.Checked = Settings.Default.AutoRaft;
            this.cbApplyCharm.Checked = Settings.Default.AutoCharm;
            this.cbShiftLoot.Checked = Settings.Default.ShiftLoot;

            // WoD Premium (Bait)
            this.txtBaitKey1.Text = Settings.Default.BaitKey1;
            this.txtBaitKey2.Text = Settings.Default.BaitKey2;
            this.txtBaitKey3.Text = Settings.Default.BaitKey3;
            this.txtBaitKey4.Text = Settings.Default.BaitKey4;
            this.txtBaitKey5.Text = Settings.Default.BaitKey5;
            this.txtBaitKey6.Text = Settings.Default.BaitKey6;
            this.txtBaitKey7.Text = Settings.Default.BaitKey7;
            this.cbAutoBait.Checked = Settings.Default.AutoBait;
            this.cbCycleThroughBaitList.Checked = Settings.Default.CycleThroughBaitList;

            // Times
            this.txtLureTime.Text = Settings.Default.LureTime.ToString();
            this.txtHearthTime.Text = Settings.Default.HearthTime.ToString();
            this.txtRaftTime.Text = Settings.Default.RaftTime.ToString();
            this.txtCharmTime.Text = Settings.Default.CharmTime.ToString();
            this.txtBaitTime.Text = Settings.Default.BaitTime.ToString();

            /// Anti Afk
            this.cbAntiAfk.Checked = Settings.Default.AntiAfk;
            this.txtAntiAfkTimer.Text = Settings.Default.AntiAfkTime.ToString();
            this.cmbMovements.SelectedIndex = Settings.Default.AntiAfkMoves;

            /// Languages
            this.chkTxt2speech.Checked = Settings.Default.Txt2speech;
            this.LoadLanguages();
        }

        private void LoadAntiAfkMovements()
        {
            this.cmbMovements.Items.Clear();

            foreach (var movements in Translate.GetTranslates("frmSettings", "CMB_ANTIAFK_MOVE")) this.cmbMovements.Items.Add(movements);
        }

        private void LoadAudioDevices()
        {
            var audioDevices = new List<Tuple<string, string>>();
            audioDevices.Add(new Tuple<string, string>("Default", string.Empty));

            try
            {
                var sndDevEnum = new MMDeviceEnumerator();
                var audioCollection = sndDevEnum.EnumerateAudioEndPoints(EDataFlow.eRender, EDeviceState.DEVICE_STATEMASK_ALL);

                // Try to add each audio endpoint to our collection
                for (var i = 0; i < audioCollection.Count; ++i)
                {
                    var device = audioCollection[i];
                    audioDevices.Add(new Tuple<string, string>(device.FriendlyName, device.ID));
                }
            }
            catch (Exception)
            {
            }

            // Setup the display
            this.cmbAudio.Items.Clear();
            this.cmbAudio.DisplayMember = "Item1";
            this.cmbAudio.ValueMember = "Item2";
            this.cmbAudio.DataSource = audioDevices;
            this.cmbAudio.SelectedValue = Settings.Default.AudioDevice;
        }

        private void LoadHotKeys()
        {
            this.m_hotkey = Settings.Default.StartStopHotKey;
            this.txtHotKey.Text = new KeysConverter().ConvertToString(this.m_hotkey);
        }

        private void LoadLanguages()
        {
            var languageFiles = Directory.GetFiles("./Resources/", "*.xml");
            this.cmbLanguage.Items.Clear();

            foreach (var file in languageFiles)
            {
                var tmpFile = file.Substring(12); // Remove the "./Resources/" part
                tmpFile = tmpFile.Substring(0, tmpFile.Length - 4); // Remove the  ".xml" part
                this.cmbLanguage.Items.Add(tmpFile);
            }

            this.cmbLanguage.SelectedItem = Settings.Default.Language;
        }

        private void SaveHotKeys()
        {
            Settings.Default.StartStopHotKey = this.m_hotkey;
            this.m_mainForm.ReloadHotkeys();
        }

        private void tabSettings_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.tmeAudio.Enabled = this.tabSettings.SelectedIndex == 2;
        }

        private void tmeAudio_Tick(object sender, EventArgs e)
        {
            if (this.m_SndDevice != null)
            {
                try
                {
                    var currentVolumnLevel = (int)(this.m_SndDevice.AudioMeterInformation.MasterPeakValue * 100);
                    this.pgbSoundLevel.Value = currentVolumnLevel;
                    this.lblAudioLevel.Text = currentVolumnLevel.ToString();
                }
                catch (Exception)
                {
                    this.pgbSoundLevel.Value = 0;
                    this.lblAudioLevel.Text = "0";
                }
            }
            else
            {
                this.pgbSoundLevel.Value = 0;
                this.lblAudioLevel.Text = "0";
            }
        }

        private void txtHotKey_KeyDown(object sender, KeyEventArgs e)
        {
            this.m_hotkey = e.KeyData;
            this.txtHotKey.Text = new KeysConverter().ConvertToString(this.m_hotkey);
        }
    }
}