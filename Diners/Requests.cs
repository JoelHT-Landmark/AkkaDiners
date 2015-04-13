namespace Diners
{
    using Akka.Actor;

    public class ForkPickupRequest
    {
        public ForkPickupRequest(ActorRef philosopher)
        {
            this.Philosopher = philosopher;
        }

        public ActorRef Philosopher { get; private set; }

    }

    public class ForkDropRequest
    {
        public ForkDropRequest(ActorRef philosopher)
        {
            this.Philosopher = philosopher;
        }

        public ActorRef Philosopher { get; private set; }

    }
}
