namespace UltimateFishBot.Classes.BodyParts
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Windows.Forms;

    using CoreAudioApi;

    using UltimateFishBot.Properties;

    internal class Ears
    {
        private const int MAX_VOLUME_QUEUE_LENGTH = 5;

        private readonly Timer listenTimer;

        private readonly Manager m_manager;

        private readonly Queue<int> m_volumeQueue;

        private readonly MMDevice SndDevice;

        private readonly int tickrate = 100; // ms pause between sound checks

        public Ears(Manager manager)
        {
            this.m_manager = manager;
            this.m_volumeQueue = new Queue<int>();

            var SndDevEnum = new MMDeviceEnumerator();
            if (Settings.Default.AudioDevice != string.Empty) this.SndDevice = SndDevEnum.GetDevice(Settings.Default.AudioDevice);
            else this.SndDevice = SndDevEnum.GetDefaultAudioEndpoint(EDataFlow.eRender, ERole.eMultimedia);

            this.listenTimer = new Timer();
            this.listenTimer.Interval = this.tickrate;
            if (Settings.Default.AverageSound)
            {
                this.listenTimer.Tick += this.ListenTimerTickAvg;
                Debug.WriteLine("Using average sound comparing");
            }
            else
            {
                this.listenTimer.Tick += this.ListenTimerTick;
                Debug.WriteLine("Using normal sound comparing");
            }

            this.listenTimer.Enabled = true;
        }

        private int GetAverageVolume()
        {
            return (int)this.m_volumeQueue.Average();
        }

        private void ListenTimerTick(object myObject, EventArgs myEventArgs)
        {
            // Get the current level
            var currentVolumnLevel = (int)(this.SndDevice.AudioMeterInformation.MasterPeakValue * 100);

            if (currentVolumnLevel >= Settings.Default.SplashLimit) this.m_manager.HearFish();

            // Debug code
            // if (m_manager.IsStoppedOrPaused() == false)
            // {
            // Debug.WriteLine("Average volume: " + avgVol);
            // Debug.WriteLine("Current volume: " + currentVolumnLevel);
            // Debug.WriteLine("Queue values: ");
            // foreach (int v in m_volumeQueue)
            // {
            // Debug.WriteLine("> " + v);
            // }
            // Debug.WriteLine("Splash limit: " + Properties.Settings.Default.SplashLimit);
            // Debug.WriteLine("______________________");
            // }
        }

        private void ListenTimerTickAvg(object myObject, EventArgs myEventArgs)
        {
            // Get the current level
            var currentVolumnLevel = (int)(this.SndDevice.AudioMeterInformation.MasterPeakValue * 100);
            this.m_volumeQueue.Enqueue(currentVolumnLevel);

            // Keep a running queue of the last X sounds as a reference point
            if (this.m_volumeQueue.Count >= MAX_VOLUME_QUEUE_LENGTH) this.m_volumeQueue.Dequeue();

            // Determine if the current level is high enough to be a fish
            var avgVol = this.GetAverageVolume();
            if (currentVolumnLevel - avgVol >= Settings.Default.SplashLimit) this.m_manager.HearFish();

            // Debug code
            // if (m_manager.IsStoppedOrPaused() == false)
            // {
            // Debug.WriteLine("Average volume: " + avgVol);
            // Debug.WriteLine("Current volume: " + currentVolumnLevel);
            // Debug.WriteLine("Queue values: ");
            // foreach (int v in m_volumeQueue)
            // {
            // Debug.WriteLine("> " + v);
            // }
            // Debug.WriteLine("Splash limit: " + Properties.Settings.Default.SplashLimit);
            // Debug.WriteLine("______________________");
            // }
        }
    }
}