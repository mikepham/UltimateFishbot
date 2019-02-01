using System;
using System.Windows.Forms;

namespace UltimateFishBot.Forms
{
    using System;
    using System.Windows.Forms;

    using UltimateFishBot.Classes;

    public partial class frmDirections : Form
    {
        private static frmDirections inst;

        public frmDirections()
        {
            this.InitializeComponent();
        }

        public static frmDirections GetForm
        {
            get
            {
                if (inst == null || inst.IsDisposed) inst = new frmDirections();
                return inst;
            }
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void frmDirections_Load(object sender, EventArgs e)
        {
            this.Text = Translate.GetTranslate("frmDirections", "TITLE");

            Label1Title.Text = Translate.GetTranslate("frmDirections", "LABEL_SETTING_WOW_TITLE");
            Label1Desc.Text = Translate.GetTranslate("frmDirections", "LABEL_SETTING_WOW_DESC");

            Label2Title.Text = Translate.GetTranslate("frmDirections", "LABEL_FIND_LOCATION_TITLE");
            Label2Desc.Text = Translate.GetTranslate("frmDirections", "LABEL_FIND_LOCATION_DESC");

            Label3Title.Text = Translate.GetTranslate("frmDirections", "LABEL_FISH_TITLE");
            Label3Desc.Text = Translate.GetTranslate("frmDirections", "LABEL_FISH_DESC");
        }
    }
}