namespace UltimateFishBot.Classes.BodyParts
{
    using System.Threading;
    using System.Windows.Forms;

    using UltimateFishBot.Classes.Helpers;
    using UltimateFishBot.Properties;

    internal class Legs
    {
        public enum Path
        {
            FRONT_BACK = 0, 

            LEFT_RIGHT = 1, 

            JUMP = 2
        }

        public void DoMovement(T2S t2s)
        {
            switch ((Path)Settings.Default.AntiAfkMoves)
            {
                case Path.FRONT_BACK:
                    this.MovePath(new[] { Keys.Up, Keys.Down });
                    break;
                case Path.LEFT_RIGHT:
                    this.MovePath(new[] { Keys.Left, Keys.Right });
                    break;
                case Path.JUMP:
                    this.MovePath(new[] { Keys.Space });
                    break;
                default:
                    this.MovePath(new[] { Keys.Left, Keys.Right });
                    break;
            }

            t2s?.Say("Anti A F K");
        }

        private void MovePath(Keys[] moves)
        {
            foreach (var move in moves)
            {
                this.SingleMove(move);
                Thread.Sleep(250);
            }
        }

        private void SingleMove(Keys move)
        {
            Win32.SendKeyboardAction(move, Win32.keyState.KEYDOWN);
            Thread.Sleep(250);
            Win32.SendKeyboardAction(move, Win32.keyState.KEYUP);
        }
    }
}