using System;
using System.Windows.Forms;

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
            this.Text = Translate.GetTranslate("frmStats", "TITLE");
            labelSuccess.Text = Translate.GetTranslate("frmStats", "LABEL_SUCCESS");
            labelNotFound.Text = Translate.GetTranslate("frmStats", "LABEL_NOT_FOUND");
            labelNotEared.Text = Translate.GetTranslate("frmStats", "LABEL_NOT_EARED");
            labelTotal.Text = Translate.GetTranslate("frmStats", "LABEL_TOTAL");
            buttonReset.Text = Translate.GetTranslate("frmStats", "BUTTON_RESET");
            buttonClose.Text = Translate.GetTranslate("frmStats", "BUTTON_CLOSE");
        }

        private void buttonReset_Click(object sender, EventArgs e)
        {
            labelSuccessCount.Text = "0";
            labelNotFoundCount.Text = "0";
            labelNotEaredCount.Text = "0";
            labelTotalCount.Text = "0";

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
            FishingStats stats = m_manager.GetFishingStats();
            labelSuccessCount.Text = stats.TotalSuccessFishing.ToString();
            labelNotFoundCount.Text = stats.TotalNotFoundFish.ToString();
            labelNotEaredCount.Text = stats.TotalNotEaredFish.ToString();
            labelTotalCount.Text = stats.Total().ToString();
        }

        private Manager m_manager;
    }
}