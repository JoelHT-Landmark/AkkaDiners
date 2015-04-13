namespace Diners
{
    using Akka.Actor;

    public class ForkPickupRequestRejectedEvent
    {
        public ForkPickupRequestRejectedEvent(ActorRef fork)
        {
            this.Fork = fork;
        }

        public ActorRef Fork { get; private set; }
    }

    public class ForkPickupRequestAcceptedEvent
    {
        public ForkPickupRequestAcceptedEvent(ActorRef fork)
        {
            this.Fork = fork;
        }

        public ActorRef Fork { get; private set; }
    }
}
