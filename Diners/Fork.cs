namespace Diners
{
    using System;

    using Akka.Actor;
    using Metrics;

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
                Console.WriteLine("{0} drops a fork", philosopher.Name(), this.Self.Name());
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
                Console.WriteLine("{0} tries to pickup a fork - but {1} is holding it already", philosopher.Name(), this.Philosopher.Name());
                philosopher.Tell(new ForkPickupRequestRejectedEvent(this.Self));
                return;
            }

            Console.WriteLine("{0} picks up a fork", philosopher.Name());
            forkPickupMeter.Mark(this.Self.Name());
            dinerPickupMeter.Mark(philosopher.Name());

            this.Philosopher = request.Philosopher;
            philosopher.Tell(new ForkPickupRequestAcceptedEvent(this.Self));
        }

        public ActorRef Philosopher { get; private set; }
    }
}
