namespace UltimateFishBot.Classes.BodyParts
{
    using System.Diagnostics;
    using System.Threading;
    using System.Windows.Forms;

    using UltimateFishBot.Classes.Helpers;
    using UltimateFishBot.Properties;

    internal class Hands
    {
        private int baitIndex;

        private string[] baitKeys;

        private Cursor cursor;

        public Hands()
        {
            Debug.Assert(Cursor.Current != null, "Current cursor was null, which is invalid.");

            this.baitIndex = 0;
            this.cursor = new Cursor(Cursor.Current.Handle);
            this.UpdateKeys();
        }

        public void Cast()
        {
            Win32.ActivateWow();
            Thread.Sleep(Settings.Default.CastingDelay);
            Win32.SendKey(Settings.Default.FishKey);
        }

        public void DoAction(Manager.NeededAction action, Mouth mouth)
        {
            string actionKey;

            int sleepTime;

            switch (action)
            {
                case Manager.NeededAction.HearthStone:
                    {
                        actionKey = Settings.Default.HearthKey;
                        mouth.Say(Translate.GetTranslate("manager", "LABEL_HEARTHSTONE"));
                        sleepTime = 0;
                        break;
                    }

                case Manager.NeededAction.Lure:
                    {
                        actionKey = Settings.Default.LureKey;
                        mouth.Say(Translate.GetTranslate("manager", "LABEL_APPLY_LURE"));
                        sleepTime = 3;
                        break;
                    }

                case Manager.NeededAction.Charm:
                    {
                        actionKey = Settings.Default.CharmKey;
                        mouth.Say(Translate.GetTranslate("manager", "LABEL_APPLY_CHARM"));
                        sleepTime = 3;
                        break;
                    }

                case Manager.NeededAction.Raft:
                    {
                        actionKey = Settings.Default.RaftKey;
                        mouth.Say(Translate.GetTranslate("manager", "LABEL_APPLY_RAFT"));
                        sleepTime = 2;
                        break;
                    }

                case Manager.NeededAction.Bait:
                    {
                        var index = 0;

                        if (Settings.Default.CycleThroughBaitList)
                        {
                            if (this.baitIndex >= 6)
                            {
                                this.baitIndex = 0;
                            }

                            index = this.baitIndex++;
                        }

                        actionKey = this.baitKeys[index];
                        mouth.Say(Translate.GetTranslate("manager", "LABEL_APPLY_BAIT", index));
                        sleepTime = 3;
                        break;
                    }

                default:
                    return;
            }

            Win32.ActivateWow();
            Win32.SendKey(actionKey);
            Thread.Sleep(sleepTime * 1000);
        }

        public void Loot()
        {
            Win32.SendMouseClick();
            Thread.Sleep(Settings.Default.LootingDelay);
        }

        public void ResetBaitIndex()
        {
            this.baitIndex = 0;
        }

        public void UpdateKeys()
        {
            this.baitKeys = new[]
                                {
                                    Settings.Default.BaitKey1, Settings.Default.BaitKey2, Settings.Default.BaitKey3, Settings.Default.BaitKey4,
                                    Settings.Default.BaitKey5, Settings.Default.BaitKey6, Settings.Default.BaitKey7
                                };
        }
    }
}