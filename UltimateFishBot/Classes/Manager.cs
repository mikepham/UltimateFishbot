namespace UltimateFishBot.Classes
{
    using System;
    using System.Windows.Forms;

    using UltimateFishBot.Classes.BodyParts;
    using UltimateFishBot.Properties;

    public class Manager
    {
        private const int ACTION_TIMER_LENGTH = 500;

        private const int MINUTE = 60 * SECOND;

        private const int SECOND = 1000;

        private FishingState m_actualState;

        private readonly Timer m_AntiAfkTimer;

        private readonly Timer m_BaitTimer;

        private readonly Timer m_CharmTimer;

        private Ears m_ears;

        private readonly Eyes m_eyes;

        private FishingStats m_fishingStats;

        private int m_fishWaitTime;

        private readonly Hands m_hands;

        private readonly Timer m_HearthStoneTimer;

        private readonly Legs m_legs;

        private readonly Timer m_LureTimer;

        private readonly frmMain m_mainForm;

        private readonly Mouth m_mouth;

        private NeededAction m_neededActions;

        private readonly Timer m_nextActionTimer;

        private readonly Timer m_RaftTimer;

        private T2S t2s;

        public Manager(frmMain mainForm)
        {
            this.m_mainForm = mainForm;

            this.m_eyes = new Eyes(this);
            this.m_hands = new Hands();
            this.m_ears = new Ears(this);
            this.m_mouth = new Mouth(this.m_mainForm);
            this.m_legs = new Legs();

            this.m_actualState = FishingState.Stopped;
            this.m_neededActions = NeededAction.None;

            this.m_fishingStats.Reset();

            // InitializeTimer(Timer,                Handler);
            this.InitializeTimer(ref this.m_nextActionTimer, this.TakeNextAction);
            this.InitializeTimer(ref this.m_LureTimer, this.LureTimerTick);
            this.InitializeTimer(ref this.m_CharmTimer, this.CharmTimerTick);
            this.InitializeTimer(ref this.m_RaftTimer, this.RaftTimerTick);
            this.InitializeTimer(ref this.m_BaitTimer, this.BaitTimerTick);
            this.InitializeTimer(ref this.m_HearthStoneTimer, this.HearthStoneTimerTick);
            this.InitializeTimer(ref this.m_AntiAfkTimer, this.AntiAfkTimerTick);

            this.ResetTimers();
        }

        public enum FishingState
        {
            Idle = 0, 

            Start = 1, 

            Casting = 2, 

            SearchingForBobber = 3, 

            WaitingForFish = 4, 

            Looting = 5, 

            Paused = 6, 

            Stopped = 7
        }

        public enum NeededAction
        {
            None = 0x00, 

            HearthStone = 0x01, 

            Lure = 0x02, 

            Charm = 0x04, 

            Raft = 0x08, 

            Bait = 0x10, 

            AntiAfkMove = 0x20
        }

        public FishingState GetActualState()
        {
            return this.m_actualState;
        }

        public FishingStats GetFishingStats()
        {
            return this.m_fishingStats;
        }

        public int GetFishWaitTime()
        {
            return this.m_fishWaitTime;
        }

        public void HearFish()
        {
            if (this.GetActualState() != FishingState.WaitingForFish) return;

            this.m_mouth.Say(Translate.GetTranslate("manager", "LABEL_HEAR_FISH"));

            this.SetActualState(FishingState.Looting);
            this.m_hands.Loot();
            this.m_fishWaitTime = 0;
            this.SetActualState(FishingState.Idle);
        }

        public bool IsStoppedOrPaused()
        {
            return this.GetActualState() == FishingState.Stopped || this.GetActualState() == FishingState.Paused;
        }

        public void Pause()
        {
            this.SetActualState(FishingState.Paused);
        }

        public void ResetFishingStats()
        {
            this.m_fishingStats.Reset();
        }

        public void ReStart()
        {
            this.ResetTimers();
            this.SwitchTimerState(true);

            if (Settings.Default.AutoLure) this.AddNeededAction(NeededAction.Lure);

            if (Settings.Default.AutoCharm) this.AddNeededAction(NeededAction.Charm);

            if (Settings.Default.AutoRaft) this.AddNeededAction(NeededAction.Raft);

            if (Settings.Default.AutoBait) this.AddNeededAction(NeededAction.Bait);

            this.SetActualState(FishingState.Start);
        }

        public void Resume()
        {
            this.SetActualState(FishingState.Start);
        }

        public void SetActualState(FishingState newState)
        {
            if (this.IsStoppedOrPaused()) if (newState != FishingState.Start) return;

            this.UpdateStats(newState);

            this.m_actualState = newState;
        }

        public void Start()
        {
            if (this.GetActualState() == FishingState.Stopped) this.ReStart();
            else if (this.GetActualState() == FishingState.Paused) this.Resume();
        }

        public void Stop()
        {
            this.SwitchTimerState(false);
            this.SetActualState(FishingState.Stopped);
        }

        private void AddNeededAction(NeededAction action)
        {
            this.m_neededActions |= action;
        }

        private void AntiAfkTimerTick(object myObject, EventArgs myEventArgs)
        {
            this.AddNeededAction(NeededAction.AntiAfkMove);
        }

        private void BaitTimerTick(object myObject, EventArgs myEventArgs)
        {
            this.AddNeededAction(NeededAction.Bait);
        }

        private void CharmTimerTick(object myObject, EventArgs myEventArgs)
        {
            this.AddNeededAction(NeededAction.Charm);
        }

        private void HandleNeededAction(NeededAction action)
        {
            switch (action)
            {
                case NeededAction.HearthStone:
                    this.m_mainForm.StopFishing();
                    goto case NeededAction.Lure; // We continue, Hearthstone need m_hands.DoAction
                case NeededAction.Lure:
                case NeededAction.Charm:
                case NeededAction.Raft:
                case NeededAction.Bait:
                    this.m_hands.DoAction(action, this.m_mouth);
                    break;
                case NeededAction.AntiAfkMove:
                    this.m_legs.DoMovement(this.t2s);
                    break;
            }

            this.RemoveNeededAction(action);
        }

        private bool HasNeededAction(NeededAction action)
        {
            return (this.m_neededActions & action) != NeededAction.None;
        }

        private void HearthStoneTimerTick(object myObject, EventArgs myEventArgs)
        {
            this.AddNeededAction(NeededAction.HearthStone);
        }

        private void InitializeTimer(ref Timer timer, EventHandler handler)
        {
            timer = new Timer();
            timer.Enabled = false;
            timer.Tick += handler;
        }

        private void LureTimerTick(object myObject, EventArgs myEventArgs)
        {
            this.AddNeededAction(NeededAction.Lure);
        }

        private void RaftTimerTick(object myObject, EventArgs myEventArgs)
        {
            this.AddNeededAction(NeededAction.Raft);
        }

        private void RemoveNeededAction(NeededAction action)
        {
            this.m_neededActions &= ~action;
        }

        private void ResetTimers()
        {
            this.m_nextActionTimer.Interval = ACTION_TIMER_LENGTH;
            this.m_LureTimer.Interval = Settings.Default.LureTime * MINUTE + 22 * SECOND;
            this.m_RaftTimer.Interval = Settings.Default.RaftTime * MINUTE;
            this.m_CharmTimer.Interval = Settings.Default.CharmTime * MINUTE;
            this.m_BaitTimer.Interval = Settings.Default.BaitTime * MINUTE;
            this.m_HearthStoneTimer.Interval = Settings.Default.HearthTime * MINUTE;
            this.m_AntiAfkTimer.Interval = Settings.Default.AntiAfkTime * MINUTE;

            this.m_fishWaitTime = 0;
        }

        private void SwitchTimerState(bool enabled)
        {
            // For activation, we check that the corresponding settings are sets
            if (enabled)
            {
                this.m_nextActionTimer.Enabled = true;

                if (Settings.Default.AutoLure) this.m_LureTimer.Enabled = true;

                if (Settings.Default.AutoRaft) this.m_RaftTimer.Enabled = true;

                if (Settings.Default.AutoCharm) this.m_CharmTimer.Enabled = true;

                if (Settings.Default.AutoBait) this.m_BaitTimer.Enabled = true;

                if (Settings.Default.AutoHearth) this.m_HearthStoneTimer.Enabled = true;

                if (Settings.Default.AntiAfk) this.m_AntiAfkTimer.Enabled = true;
            }

            // On deactivation, we don't care
            else
            {
                this.m_nextActionTimer.Enabled = false;
                this.m_LureTimer.Enabled = false;
                this.m_RaftTimer.Enabled = false;
                this.m_CharmTimer.Enabled = false;
                this.m_BaitTimer.Enabled = false;
                this.m_HearthStoneTimer.Enabled = false;
            }
        }

        private void TakeNextAction(object myObject, EventArgs myEventArgs)
        {
            switch (this.GetActualState())
            {
                case FishingState.Start:
                    {
                        // We just start, going to Idle to begin bot loop
                        this.SetActualState(FishingState.Idle);
                        break;
                    }

                case FishingState.Idle:
                    {
                        // We first check if another action is needed, foreach on all NeededAction enum values
                        foreach (var neededAction in (NeededAction[])Enum.GetValues(typeof(NeededAction)))
                        {
                            if (this.HasNeededAction(neededAction))
                            {
                                this.HandleNeededAction(neededAction);
                                return;
                            }
                        }

                        // If no other action required, we can cast !
                        this.m_mouth.Say(Translate.GetTranslate("manager", "LABEL_CASTING"));
                        this.SetActualState(FishingState.Casting);
                        this.m_hands.Cast();
                        break;
                    }

                case FishingState.Casting:
                    {
                        this.m_mouth.Say(Translate.GetTranslate("manager", "LABEL_START_FINDING"));
                        this.SetActualState(FishingState.SearchingForBobber);
                        this.m_eyes.StartLooking(); // <= The new state will be set in the Eyes
                        break;
                    }

                case FishingState.SearchingForBobber:
                    {
                        // We are just waiting for the Eyes
                        this.m_mouth.Say(Translate.GetTranslate("manager", "LABEL_FINDING"));
                        break;
                    }

                case FishingState.WaitingForFish:
                    {
                        // We are waiting a detection from the Ears
                        this.m_mouth.Say(Translate.GetTranslate("manager", "LABEL_WAITING", this.GetFishWaitTime() / 1000, Settings.Default.FishWait / 1000));

                        if ((this.m_fishWaitTime += ACTION_TIMER_LENGTH) >= Settings.Default.FishWait)
                        {
                            this.SetActualState(FishingState.Idle);
                            this.m_fishWaitTime = 0;
                        }

                        break;
                    }
            }
        }

        private void UpdateStats(FishingState newState)
        {
            if (newState == FishingState.Idle)
            {
                // If we start a new loop, check why and increase stats according
                switch (this.m_actualState)
                {
                    case FishingState.Looting:
                        {
                            ++this.m_fishingStats.totalSuccessFishing;
                            break;
                        }

                    case FishingState.Casting:
                    case FishingState.SearchingForBobber:
                        {
                            ++this.m_fishingStats.totalNotFoundFish;
                            break;
                        }

                    case FishingState.WaitingForFish:
                        {
                            ++this.m_fishingStats.totalNotEaredFish;
                            break;
                        }
                }
            }
        }

        public struct FishingStats
        {
            public int totalSuccessFishing { get; set; }

            public int totalNotFoundFish { get; set; }

            public int totalNotEaredFish { get; set; }

            public void Reset()
            {
                this.totalSuccessFishing = 0;
                this.totalNotFoundFish = 0;
                this.totalNotEaredFish = 0;
            }

            public int Total()
            {
                return this.totalSuccessFishing + this.totalNotFoundFish + this.totalNotEaredFish;
            }
        }
    }
}