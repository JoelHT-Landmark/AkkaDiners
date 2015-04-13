namespace Diners
{
    using System;

    using Akka.Actor;

    public class Fork : ReceiveActor
    {
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
                Console.WriteLine("{0} drops {1}", philosopher.Name(), this.Self.Name());
                this.Philosopher = null;
            }
        }

        private void HandlePickupRequest(ForkPickupRequest request)
        {
            var philosopher = this.Sender;

            if (philosopher == this.Philosopher)
            {
                Console.WriteLine("{0} is already holding {1}", philosopher.Name(), this.Self.Name());
                return;
            }

            if (this.Philosopher != null)
            {
                Console.WriteLine("{0} tries to pickup {1} - but {2} is holding it already", philosopher.Name(), this.Self.Name(), this.Philosopher.Name());
                philosopher.Tell(new ForkPickupRequestRejectedEvent(this.Self));
                return;
            }

            Console.WriteLine("{0} picks up {1}", philosopher.Name(), this.Self.Name());

            this.Philosopher = request.Philosopher;
            philosopher.Tell(new ForkPickupRequestAcceptedEvent(this.Self));
        }

        public ActorRef Philosopher { get; private set; }
    }
}
