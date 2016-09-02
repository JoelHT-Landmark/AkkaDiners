namespace Diners
{
    using System;

    using Akka.Actor;
    using Metrics;
    using Serilog;

    public class Fork : ReceiveActor
    {
        private static readonly Meter forkPickupMeter = Metric.Meter("ForkPickups", Unit.Events);
        private static readonly Meter dinerPickupMeter = Metric.Meter("DinerPickups", Unit.Events);

        public Fork()
        {
            Receive<ForkPickupRequest>(r => this.HandlePickupRequest(r));
            Receive<ForkDropRequest>(r => this.HandleDropRequest(r));
        }

        private void HandleDropRequest(ForkDropRequest forkDropRequest)
        {
            var philosopher = this.Sender;

            if (this.Philosopher == philosopher)
            {
                Log.Information("{philosopher} drops a fork", philosopher.Name(), this.Self.Name());
                this.Philosopher = null;
            }
        }

        private void HandlePickupRequest(ForkPickupRequest request)
        {
            var philosopher = this.Sender;

            if (philosopher == this.Philosopher)
            {
                return;
            }

            if (this.Philosopher != null)
            {
                Log.Information("{philosopher} tried to pickup a fork - but {competitor} was holding it already", philosopher.Name(), this.Philosopher.Name());
                philosopher.Tell(new ForkPickupRequestRejectedEvent(this.Self));
                return;
            }

            Log.Information("{philosopher} picks up a fork", philosopher.Name());
            forkPickupMeter.Mark(this.Self.Name());
            dinerPickupMeter.Mark(philosopher.Name());

            this.Philosopher = request.Philosopher;
            philosopher.Tell(new ForkPickupRequestAcceptedEvent(this.Self));
        }

        public ActorRef Philosopher { get; private set; }
    }
}
