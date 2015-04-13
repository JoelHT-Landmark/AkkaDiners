namespace Diners
{
    using Akka.Actor;

    public class BeginEatingOrder
    {
    }

    public class StopEatingOrder
    {
    }


    public class AssignLeftForkOrder
    {
        public AssignLeftForkOrder(ActorRef reference)
        {
            this.LeftFork = reference;
        }

        public ActorRef LeftFork { get; private set; }
    }

    public class AssignRightForkOrder
    {
        public AssignRightForkOrder(ActorRef reference)
        {
            this.RightFork = reference;
        }

        public ActorRef RightFork { get; private set; }
    }

}
