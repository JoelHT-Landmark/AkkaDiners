namespace Diners
{
    using System;

    using Akka.Actor;
    using Metrics;

    public class Philosopher : ReceiveActor
    {
        private Random random = new Random();

        private static readonly Counter dinersEating = Metric.Counter("DinersEating", Unit.Items);
        private static readonly Counter dinersWaiting = Metric.Counter("DinersWaiting", Unit.Items);
        private static readonly Counter dinersMeditating = Metric.Counter("DinersMeditating", Unit.Items);
        private static readonly Histogram dinerForks = Metric.Histogram("DinerForks", Unit.Items);
        private static readonly Timer dinerTimer = Metric.Timer("DinerActions", Unit.Events);

        public enum PhilosopherState
        {
            Bored, 
            Waiting,
            Eating,
            Meditating,
        }

        public Philosopher()
        {
            Receive<BeginEatingOrder>(o => BeginEating());
            Receive<StopEatingOrder>(o => this.StopEating());

            Receive<AssignLeftForkOrder>(o => AssignLeftFork(o.LeftFork));
            Receive<AssignRightForkOrder>(o => AssignRightFork(o.RightFork));
            Receive<ForkPickupRequestRejectedEvent>(o => this.DropFork(o.Fork));
            Receive<ForkPickupRequestAcceptedEvent>(o => this.PickUpFork(o.Fork));
        }

        private void PickUpFork(ActorRef fork)
        {
            if (this.LeftFork == fork)
            {
                Console.WriteLine("{0} has {1} in his left hand", this.Self.Name(), fork.Name());
                this.OwnsLeftFork = true;
            }
            else if(this.RightFork == fork)
            {
                Console.WriteLine("{0} has {1} in his right hand", this.Self.Name(), fork.Name());
                this.OwnsRightFork = true;
            }

            if (!this.OwnsLeftFork || !this.OwnsRightFork)
            {
                // randomly drop held forks
                if (this.random.Next(1) == 1)
                {
                    this.DropFork(this.LeftFork);
                    this.DropFork(this.RightFork);

                    dinerForks.Update(0, this.Self.Name());
                }
                else
                {
                    dinerForks.Update(1, this.Self.Name());
                }

                this.StartWaiting();
                return;
            }

            dinerForks.Update(2, this.Self.Name());
            this.StartEating();
        }

        private void StartEating()
        {
            var period = random.Next(30);
            Console.ForegroundColor = ConsoleColor.DarkGreen;
            Console.WriteLine("{0} starts eating for {1}s...", this.Self.Name(), period);
            Console.ResetColor();

            this.State = PhilosopherState.Eating;
            dinersEating.Increment();

            Wait.For(new TimeSpan(0, 0, period)).Then(() => {
                dinersEating.Decrement();
                this.Self.Tell(new StopEatingOrder());
            });            
        }

        private void StopEating()
        {
            Console.ForegroundColor = ConsoleColor.DarkGreen;
            Console.WriteLine("{0} stops eating...", this.Self.Name());
            Console.ResetColor();

            this.DropFork(this.LeftFork);
            this.DropFork(this.RightFork);

            this.Meditate();
        }

        private void Meditate()
        {
            var period = this.random.Next(30);

            this.State = PhilosopherState.Meditating;
            dinersMeditating.Increment();

            Console.ForegroundColor = ConsoleColor.DarkBlue;
            Console.WriteLine("{0} starts meditating for {1}s...", this.Self.Name(), period);
            Console.ResetColor();
            
            Wait.For(new TimeSpan(0, 0, period)).Then(() => {
                dinersMeditating.Decrement();
                this.Self.Tell(new BeginEatingOrder());
                });            
        }

        private void StartWaiting()
        {
            var period = this.random.Next(30);

            this.State = PhilosopherState.Waiting;
            dinersWaiting.Increment();

            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.WriteLine("{0} waits for {1}s...", this.Self.Name(), period);
            Console.ResetColor();

            Wait.For(new TimeSpan(0, 0, period)).Then(() =>
            {
                dinersWaiting.Decrement();
                this.Self.Tell(new BeginEatingOrder());
            });            
        }

        private void DropFork(ActorRef fork)
        {
            if (this.LeftFork == fork)
            {
                //Console.WriteLine("{0} doesn't have  {1} in his left hand", this.Self.Name(), fork.Name());
                this.OwnsLeftFork = false;
            }
            else if(this.RightFork == fork)
            {
                //Console.WriteLine("{0} doesn't have {1} in his right hand", this.Self.Name(), fork.Name());
                this.OwnsRightFork = false;
            }

            fork.Tell(new ForkDropRequest(this.Self));
        }

        private void AssignLeftFork(ActorRef leftFork)
        {
            this.LeftFork = leftFork;
            Console.WriteLine("{0} is using {1} as his left fork", this.Self.Name(), leftFork.Name());
        }

        private void AssignRightFork(ActorRef rightFork)
        {
            this.RightFork = rightFork;
            Console.WriteLine("{0} is using {1} as his right fork", this.Self.Name(), rightFork.Name());
        }

        private void BeginEating()
        {
            Console.WriteLine("{0} is thinking about eating.", this.Self.Name());

            // How can we eat without any forks?
            if ((this.LeftFork == null) || (this.RightFork == null))
            {
                Console.WriteLine("{0} is confused - which forks should he use?", this.Self.Name());
                this.StartWaiting();
                return;
            }

            // we're already eating
            if (this.State == PhilosopherState.Eating)
            {
                Console.WriteLine("{0} is still eating...", this.Self.Name());
                return;
            }

            // do we have both forks? let's go!
            if (this.OwnsLeftFork && this.OwnsRightFork)
            {
                this.StartEating();
                return;
            }

            // otherwise try and get any missing forks and wait.
            if (!this.OwnsLeftFork)
            {
                this.LeftFork.Tell(new ForkPickupRequest(this.Self));                
            }

            if (!this.OwnsRightFork)
            {
                this.RightFork.Tell(new ForkPickupRequest(this.Self));                
            }

            this.StartWaiting();
        }

        public PhilosopherState State { get; private set; }

        public bool OwnsLeftFork { get; private set; }

        public bool OwnsRightFork { get; private set; }

        public ActorRef LeftFork { get; private set; }

        public ActorRef RightFork { get; private set; }
    }
}
