namespace UltimateFishBot.Classes.BodyParts
{
    using System.Speech.Synthesis;

    using UltimateFishBot.Properties;

    internal class Mouth
    {
        private readonly frmMain m_mainForm;

        private readonly T2S t2s = new T2S();

        public Mouth(frmMain mainForm)
        {
            this.m_mainForm = mainForm;
        }

        public void Say(string text)
        {
            this.m_mainForm.lblStatus.Text = text;
            if (text == Translate.GetTranslate("manager", "LABEL_PAUSED") || (text == Translate.GetTranslate("manager", "LABEL_STOPPED")))
            {
                this.m_mainForm.lblStatus.Image = Resources.offline;
            }
            else
            {
                this.m_mainForm.lblStatus.Image = Resources.online;
            }

            this.t2s.Say(text);
        }
    }

    internal class T2S
    {
        private string lastMessage;

        private readonly SpeechSynthesizer synthesizer = new SpeechSynthesizer();

        private readonly bool uset2s;

        public T2S()
        {
            this.uset2s = Settings.Default.Txt2speech;
            this.synthesizer.Volume = 60; // 0...100
            this.synthesizer.Rate = 1; // -10...10
        }

        public void Say(string text)
        {
            // Debug code
            // System.Console.WriteLine("Use T2S: " + uset2s);
            // System.Console.WriteLine("Previous message: " + lastMessage);
            // System.Console.WriteLine("Current message: " + text);
            // System.Console.WriteLine("Synthesizer ready: " + (synthesizer.State == SynthesizerState.Ready));

            // Say asynchronous text through Text 2 Speech synthesizer
            if (this.uset2s && (this.lastMessage != text) && (this.synthesizer.State == SynthesizerState.Ready))
            {
                this.synthesizer.SpeakAsync(text);
                this.lastMessage = text;
            }
        }
    }
}