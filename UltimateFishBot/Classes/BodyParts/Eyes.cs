namespace UltimateFishBot.Classes.BodyParts
{
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Threading;

    using UltimateFishBot.Classes.Helpers;
    using UltimateFishBot.Properties;

    internal class Eyes
    {
        private readonly BackgroundWorker m_backgroundWorker;

        private readonly Manager m_manager;

        private Win32.CursorInfo m_noFishCursor;

        private Rectangle wowRectangle;

        private int xPosMax;

        private int xPosMin;

        private int yPosMax;

        private int yPosMin;

        public Eyes(Manager manager)
        {
            this.m_manager = manager;

            this.m_backgroundWorker = new BackgroundWorker();
            this.m_backgroundWorker.WorkerSupportsCancellation = true;
            this.m_backgroundWorker.DoWork += this.EyeProcess_DoWork;
            this.m_backgroundWorker.RunWorkerCompleted += this.EyeProcess_RunWorkerCompleted;
        }

        public void StartLooking()
        {
            if (this.m_backgroundWorker.IsBusy) return;

            this.m_manager.SetActualState(Manager.FishingState.SearchingForBobber);
            this.m_backgroundWorker.RunWorkerAsync();
        }

        private void EyeProcess_DoWork(object sender, DoWorkEventArgs e)
        {
            this.m_noFishCursor = Win32.GetNoFishCursor();
            this.wowRectangle = Win32.GetWowRectangle();

            if (!Settings.Default.customScanArea)
            {
                this.xPosMin = this.wowRectangle.Width / 4;
                this.xPosMax = this.xPosMin * 3;
                this.yPosMin = this.wowRectangle.Height / 4;
                this.yPosMax = this.yPosMin * 3;
                Console.Out.WriteLine("Using default area");
            }
            else
            {
                this.xPosMin = Settings.Default.minScanXY.X;
                this.yPosMin = Settings.Default.minScanXY.Y;
                this.xPosMax = Settings.Default.maxScanXY.X;
                this.yPosMax = Settings.Default.maxScanXY.Y;
                Console.Out.WriteLine("Using custom area");
            }

            Console.Out.WriteLine("Scanning area: " + this.xPosMin + " , " + this.yPosMin + " , " + this.xPosMax + " , " + this.yPosMax + " , ");
            if (Settings.Default.AlternativeRoute) this.LookForBobber_Spiral();
            else this.LookForBobber();
        }

        private void EyeProcess_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                // If not found, exception is sent...
                this.m_manager.SetActualState(Manager.FishingState.Idle);
                return;
            }

            // ... if no exception, we found a fish !
            this.m_manager.SetActualState(Manager.FishingState.WaitingForFish);
        }

        private bool ImageCompare(Bitmap firstImage, Bitmap secondImage)
        {
            if (firstImage.Width != secondImage.Width || firstImage.Height != secondImage.Height) return false;

            for (var i = 0; i < firstImage.Width; i++) for (var j = 0; j < firstImage.Height; j++) if (firstImage.GetPixel(i, j).ToString() != secondImage.GetPixel(i, j).ToString()) return false;

            return true;
        }

        private void LookForBobber()
        {
            var XPOSSTEP = (this.xPosMax - this.xPosMin) / Settings.Default.ScanningSteps;
            var YPOSSTEP = (this.yPosMax - this.yPosMin) / Settings.Default.ScanningSteps;
            var XOFFSET = XPOSSTEP / Settings.Default.ScanningRetries;

            if (Settings.Default.customScanArea)
            {
                for (var tryCount = 0; tryCount < Settings.Default.ScanningRetries; ++tryCount)
                {
                    for (var x = this.xPosMin + XOFFSET * tryCount; x < this.xPosMax; x += XPOSSTEP)
                    {
                        for (var y = this.yPosMin; y < this.yPosMax; y += YPOSSTEP)
                        {
                            if (this.MoveMouseAndCheckCursor(x, y)) return;
                        }
                    }
                }
            }
            else
            {
                for (var tryCount = 0; tryCount < Settings.Default.ScanningRetries; ++tryCount)
                {
                    for (var x = this.xPosMin + XOFFSET * tryCount; x < this.xPosMax; x += XPOSSTEP)
                    {
                        for (var y = this.yPosMin; y < this.yPosMax; y += YPOSSTEP)
                        {
                            if (this.MoveMouseAndCheckCursor(this.wowRectangle.X + x, this.wowRectangle.Y + y)) return;
                        }
                    }
                }
            }

            throw new Exception("Fish not found"); // Will be catch in Manager:EyeProcess_RunWorkerCompleted
        }

        private void LookForBobber_Spiral()
        {
            var XPOSSTEP = (this.xPosMax - this.xPosMin) / Settings.Default.ScanningSteps;
            var YPOSSTEP = (this.yPosMax - this.yPosMin) / Settings.Default.ScanningSteps;
            var XOFFSET = XPOSSTEP / Settings.Default.ScanningRetries;
            var YOFFSET = YPOSSTEP / Settings.Default.ScanningRetries;

            if (Settings.Default.customScanArea)
            {
                for (var tryCount = 0; tryCount < Settings.Default.ScanningRetries; ++tryCount)
                {
                    var x = (this.xPosMin + this.xPosMax) / 2 + XOFFSET * tryCount;
                    var y = (this.yPosMin + this.yPosMax) / 2 + YOFFSET * tryCount;

                    for (var i = 0; i <= 2 * Settings.Default.ScanningSteps; i++)
                    {
                        for (var j = 0; j <= i / 2; j++)
                        {
                            int dx = 0, dy = 0;

                            if (i % 2 == 0)
                            {
                                if (i / 2 % 2 == 0)
                                {
                                    dx = XPOSSTEP;
                                    dy = 0;
                                }
                                else
                                {
                                    dx = -XPOSSTEP;
                                    dy = 0;
                                }
                            }
                            else
                            {
                                if (i / 2 % 2 == 0)
                                {
                                    dx = 0;
                                    dy = YPOSSTEP;
                                }
                                else
                                {
                                    dx = 0;
                                    dy = -YPOSSTEP;
                                }
                            }

                            x += dx;
                            y += dy;

                            if (this.MoveMouseAndCheckCursor(x, y)) return;
                        }
                    }
                }
            }
            else
            {
                for (var tryCount = 0; tryCount < Settings.Default.ScanningRetries; ++tryCount)
                {
                    var x = (this.xPosMin + this.xPosMax) / 2 + XOFFSET * tryCount;
                    var y = (this.yPosMin + this.yPosMax) / 2 + YOFFSET * tryCount;

                    for (var i = 0; i <= 2 * Settings.Default.ScanningSteps; i++)
                    {
                        for (var j = 0; j <= i / 2; j++)
                        {
                            int dx = 0, dy = 0;

                            if (i % 2 == 0)
                            {
                                if (i / 2 % 2 == 0)
                                {
                                    dx = XPOSSTEP;
                                    dy = 0;
                                }
                                else
                                {
                                    dx = -XPOSSTEP;
                                    dy = 0;
                                }
                            }
                            else
                            {
                                if (i / 2 % 2 == 0)
                                {
                                    dx = 0;
                                    dy = YPOSSTEP;
                                }
                                else
                                {
                                    dx = 0;
                                    dy = -YPOSSTEP;
                                }
                            }

                            x += dx;
                            y += dy;

                            if (this.MoveMouseAndCheckCursor(this.wowRectangle.X + x, this.wowRectangle.Y + y)) return;
                        }
                    }
                }
            }

            throw new Exception("Fish not found"); // Will be catch in Manager:EyeProcess_RunWorkerCompleted
        }

        private bool MoveMouseAndCheckCursor(int x, int y)
        {
            if (this.m_manager.IsStoppedOrPaused()) throw new Exception("Bot paused or stopped");

            Win32.MoveMouse(x, y);

            // Sleep (give the OS a chance to change the cursor)
            Thread.Sleep(Settings.Default.ScanningDelay);

            var actualCursor = Win32.GetCurrentCursor();

            if (actualCursor.flags == this.m_noFishCursor.flags && actualCursor.hCursor == this.m_noFishCursor.hCursor) return false;

            // Compare the actual icon with our fishIcon if user want it
            if (Settings.Default.CheckCursor) if (!this.ImageCompare(Win32.GetCursorIcon(actualCursor), Resources.fishIcon35x35)) return false;

            // We found a fish !
            return true;
        }
    }
}