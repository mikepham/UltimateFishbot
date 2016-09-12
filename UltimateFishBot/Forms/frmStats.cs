namespace UltimateFishBot.Forms
{
    using System;
    using System.Windows.Forms;

    using UltimateFishBot.Classes;

    public partial class frmStats : Form
    {
        private static frmStats inst;

        private readonly Manager m_manager;

        public frmStats(Manager manager)
        {
            this.InitializeComponent();

            this.m_manager = manager;
            this.UpdateStats();
        }

        public static frmStats GetForm(Manager manag)
        {
            if (inst == null || inst.IsDisposed) inst = new frmStats(manag);
            return inst;
        }

        private void buttonClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void buttonReset_Click(object sender, EventArgs e)
        {
            this.labelSuccessCount.Text = "0";
            this.labelNotFoundCount.Text = "0";
            this.labelNotEaredCount.Text = "0";
            this.labelTotalCount.Text = "0";

            this.m_manager.ResetFishingStats();
        }

        private void frmStats_Load(object sender, EventArgs e)
        {
            this.Text = Translate.GetTranslate("frmStats", "TITLE");
            this.labelSuccess.Text = Translate.GetTranslate("frmStats", "LABEL_SUCCESS");
            this.labelNotFound.Text = Translate.GetTranslate("frmStats", "LABEL_NOT_FOUND");
            this.labelNotEared.Text = Translate.GetTranslate("frmStats", "LABEL_NOT_EARED");
            this.labelTotal.Text = Translate.GetTranslate("frmStats", "LABEL_TOTAL");
            this.buttonReset.Text = Translate.GetTranslate("frmStats", "BUTTON_RESET");
            this.buttonClose.Text = Translate.GetTranslate("frmStats", "BUTTON_CLOSE");
        }

        private void timerUpdateStats_Tick(object sender, EventArgs e)
        {
            this.UpdateStats();
        }

        private void UpdateStats()
        {
            var stats = this.m_manager.GetFishingStats();
            this.labelSuccessCount.Text = stats.totalSuccessFishing.ToString();
            this.labelNotFoundCount.Text = stats.totalNotFoundFish.ToString();
            this.labelNotEaredCount.Text = stats.totalNotEaredFish.ToString();
            this.labelTotalCount.Text = stats.Total().ToString();
        }
    }
}