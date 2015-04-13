namespace Diners
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    public class Wait
    {
        private TimeSpan timeSpan;

        private Wait()
        {
        }

        private Wait(TimeSpan timeSpan)
        {
            this.timeSpan = timeSpan;
        }

        public static Wait For(TimeSpan timeSpan)
        {
            var result = new Wait(timeSpan);
            return result;
        }

        internal async void Then(Action p)
        {
            Thread.Sleep(this.timeSpan);
            p();
        }
    }
}
