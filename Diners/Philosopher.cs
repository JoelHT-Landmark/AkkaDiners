namespace Diners
{
    using System;

    using Akka.Actor;
    using Metrics;
    using Serilog;
    using System.Diagnostics;
    using System.Collections.Generic;

    public class Philosopher : ReceiveActor
    {
        private Random random = new Random();

        private static readonly Counter dinersEating = Metric.Counter("DinersEating", Unit.Items);
        private static readonly Counter dinersWaiting = Metric.Counter("DinersWaiting", Unit.Items);
        private static readonly Counter dinersMeditating = Metric.Counter("DinersMeditating", Unit.Items);
        private static readonly Histogram dinerForks = Metric.Histogram("DinerForks", Unit.Items);
        private static readonly Timer dinerTimer = Metric.Timer("DinerActions", Unit.Events);

        private readonly Dictionary<PhilosopherState, Stopwatch> stopwatches = new Dictionary<PhilosopherState, Stopwatch>();
        private readonly object padlock = new object();

        public enum PhilosopherState
        {
            Bored, 
            Waiting,
            Eating,
            Meditating,
        }

        public Philosopher()
        {
            Log.Verbose("Entering Philosopher.cTor()");

            Log.Information(
                "Configuring {philosopher} to receive {commands}.", 
                this.Self.Name(), 
                new[] { typeof(BeginEatingOrder), typeof(StopEatingOrder), typeof(AssignLeftForkOrder), typeof(AssignRightForkOrder) });

            Receive<BeginEatingOrder>(o => BeginEating());
            Receive<StopEatingOrder>(o => this.StopEating());

            Receive<AssignLeftForkOrder>(o => AssignLeftFork(o.LeftFork));
            Receive<AssignRightForkOrder>(o => AssignRightFork(o.RightFork));

            Log.Information(
                "Configuring {philosopher} to receive {events}.", 
                this.Self.Name(), 
                new[] { typeof(ForkPickupRequestRejectedEvent), typeof(ForkPickupRequestAcceptedEvent) });

            Receive<ForkPickupRequestRejectedEvent>(o => this.DropFork(o.Fork));
            Receive<ForkPickupRequestAcceptedEvent>(o => this.PickUpFork(o.Fork));

            Log.Verbose("Leaving Philosopher.cTor()");
        }

        private void PickUpFork(ActorRef fork)
        {
            Log.Verbose("Entering Philosopher.PickUpFork()");

            var logContext = Log.ForContext("philosopher", this.Self.Name())
                                .ForContext("fork", fork.Name());

            if (this.LeftFork == fork)
            {
                Console.WriteLine("{0} now has a fork in his left hand", this.Self.Name());
                this.OwnsLeftFork = true;
                logContext.Information("{philosopher} owns {fork} as his {handedness} fork", this.Self.Name(), fork.Name(), "Left");
            }
            else if(this.RightFork == fork)
            {
                Console.WriteLine("{0} now has a fork in his right hand", this.Self.Name());
                this.OwnsRightFork = true;
                logContext.Information("{philosopher} owns {fork} as his {handedness} fork", this.Self.Name(), fork.Name(), "Right");
            }

            // randomly drop held forks
            if (this.random.Next(100) == 95)
            {
                logContext.Debug("{philosopher} is about to drop any forks held.", this.Self.Name());
                Console.WriteLine("Oops! Clumsy {0} has randomly dropped his forks!", this.Self.Name());

                if (this.OwnsLeftFork)
                {
                    this.DropFork(this.LeftFork);
                }

                if (this.OwnsRightFork)
                {
                    this.DropFork(this.RightFork);
                }

                dinerForks.Update(0, this.Self.Name());
                this.StartWaiting();

                Log.Verbose("Leaving Philosopher.PickUpFork()");
                return;
            }

            if (!this.OwnsLeftFork || !this.OwnsRightFork)
            {
                logContext.Debug("{philosopher} has only one fork.");

                dinerForks.Update(1, this.Self.Name());

                this.StartWaiting();

                Log.Verbose("Leaving Philosopher.PickUpFork()");
                return;
            }

            logContext.Debug("{philsopher} is about to start eating - both forks held.", this.Self.Name());

            dinerForks.Update(2, this.Self.Name());
            this.StartEating();

            Log.Verbose("Leaving Philosopher.PickUpFork()");
        }

        private void StartEating()
        {
            Log.Verbose("Entering Philosopher.StartEating()");

            var period = random.Next(30);
            Console.ForegroundColor = ConsoleColor.DarkGreen;
            Console.WriteLine("{0} starts eating for {1}s...", this.Self.Name(), period);
            Console.ResetColor();

            this.SetPhilosopherState(PhilosopherState.Eating);

            dinersEating.Increment();

            Wait.For(new TimeSpan(0, 0, period)).Then(() => {
                dinersEating.Decrement();

                var order = new StopEatingOrder();
                Log.Information("Sending {order} to {philosopher}", order, this.Self.Name());
                this.Self.Tell(order);
            });

            Log.Verbose("Leaving Philosopher.StartEating()");
        }

        private void StopEating()
        {
            Log.Verbose("Entering Philosopher.StopEating()");

            Console.ForegroundColor = ConsoleColor.DarkGreen;
            Console.WriteLine("{0} stops eating...", this.Self.Name());
            Console.ResetColor();

            this.DropFork(this.LeftFork);
            this.DropFork(this.RightFork);

            this.Meditate();

            Log.Verbose("Leaving Philosopher.StopEating()");
        }

        private void Meditate()
        {
            Log.Verbose("Entering Philosopher.Meditate()");

            var period = this.random.Next(30);

            this.SetPhilosopherState(PhilosopherState.Meditating);

            Console.ForegroundColor = ConsoleColor.DarkBlue;
            Console.WriteLine("{0} starts meditating for {1}s...", this.Self.Name(), period);
            Console.ResetColor();
            
            Wait.For(new TimeSpan(0, 0, period)).Then(() => {
                dinersMeditating.Decrement();
                var order = new BeginEatingOrder();
                Log.Information("Sending {order} to {philosopher}", order, this.Self.Name());
                this.Self.Tell(order);
                });

            Log.Verbose("Leaving Philosopher.Meditate()");
        }

        private void StartWaiting()
        {
            Log.Verbose("Entering Philosopher.StartWaiting()");

            var period = this.random.Next(30);

            this.SetPhilosopherState(PhilosopherState.Waiting);
            dinersWaiting.Increment();

            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.WriteLine("{0} waits for {1}s...", this.Self.Name(), period);
            Console.ResetColor();

            Wait.For(new TimeSpan(0, 0, period)).Then(() =>
            {
                dinersWaiting.Decrement();
                var order = new BeginEatingOrder();
                Log.Information("Sending {order} to {philosopher}", order, this.Self.Name());
                this.Self.Tell(order);
            });

            Log.Verbose("Leaving Philosopher.StartWaiting()");
        }

        private void DropFork(ActorRef fork)
        {
            Log.Verbose("Entering Philosopher.DropFork()");

            if (this.LeftFork == fork)
            {
                this.OwnsLeftFork = false;

                Console.WriteLine("{0} drops his left fork", this.Self.Name());
                Log.Information("{philosopher} released {fork} as his {handedness} fork", this.Self.Name(), fork.Name(), "Left");
            }
            else if(this.RightFork == fork)
            {
                this.OwnsRightFork = false;

                Console.WriteLine("{0} drops his right fork", this.Self.Name());
                Log.Information("{philosopher} released {fork} as his {handedness} fork", this.Self.Name(), fork.Name(), "Right");
            }

            var request = new ForkDropRequest(this.Self);
            Log.Information("Sending {request} to {fork} for {philosopher}", request, fork.Name(), this.Self.Name());
            fork.Tell(request);

            Log.Verbose("Leaving Philosopher.DropFork()");
        }

        private void AssignLeftFork(ActorRef leftFork)
        {
            Log.Verbose("Entering Philosopher.AssignLeftFork()");

            this.LeftFork = leftFork;

            Console.WriteLine("{0} is using {1} as his left fork", this.Self.Name(), leftFork.Name());
            Log.Information("{philosopher} assigned {fork} as his {handedness} fork", this.Self.Name(), this.LeftFork.Name(), "Left");

            Log.Verbose("Leaving Philosopher.AssignLeftFork()");
        }

        private void AssignRightFork(ActorRef rightFork)
        {
            Log.Verbose("Entering Philosopher.AssignRightFork()");

            this.RightFork = rightFork;

            Console.WriteLine("{0} is using {1} as his right fork", this.Self.Name(), rightFork.Name());
            Log.Information("{philosopher} assigned {fork} as his {handedness} fork", this.Self.Name(), this.RightFork.Name(), "Right");

            Log.Verbose("Leaving Philosopher.AssignRightFork()");
        }

        private void BeginEating()
        {
            Log.Verbose("Entering Philosopher.BeginEating()");

            Console.WriteLine("{0} is thinking about eating.", this.Self.Name());

            // How can we eat without any forks?
            if ((this.LeftFork == null) || (this.RightFork == null))
            {
                Console.WriteLine("{0} is confused - he has no forks.", this.Self.Name());
                this.StartWaiting();
                return;
            }

            // we're already eating
            if (this.State == PhilosopherState.Eating)
            {
                Console.WriteLine("{0} continues eating...", this.Self.Name());
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
                var request = new ForkPickupRequest(this.Self);
                Log.Information("Sending {request} to {fork} for {philosopher}", request, this.LeftFork.Name(), this.Self.Name());
                this.LeftFork.Tell(request);                
            }

            if (!this.OwnsRightFork)
            {
                var request = new ForkPickupRequest(this.Self);
                Log.Information("Sending {request} to {fork} for {philosopher}", request, this.RightFork.Name(), this.Self.Name());
                this.RightFork.Tell(request);
            }

            this.StartWaiting();

            Log.Verbose("Leaving Philosopher.StartEating()");
        }

        private void SetPhilosopherState(PhilosopherState state)
        {
            Log.Verbose("Setting state for {philosopher} from {oldState} to {newState}", this.Self.Name(), this.State, state);

            StopStopwatch(this.State);
            this.State = state;
            StartStopwatch(state);
        }

        private void StopStopwatch(PhilosopherState state)
        {
            var stopwatch = GetStopwatchForState(state);
            if (stopwatch.IsRunning)
            {
                stopwatch.Stop();
                Log.Information("{philosopher} was {state} for {duration}ms", this.Self.Name(), state, stopwatch.ElapsedMilliseconds);
            }

            Log.Verbose("Resetting {state} stopwatch for {philosopher}", state, this.Self.Name());
            stopwatch.Reset();
        }

        private void StartStopwatch(PhilosopherState state)
        {
            var stopwatch = GetStopwatchForState(state);
            if (stopwatch.IsRunning)
            {
                Log.Warning("{state} stopwatch for {philosopher} is already running.", state, this.Self.Name());
                return;
            }

            Log.Verbose("Starting {state} stopwatch for {philosopher}", state, this.Self.Name());
            stopwatch.Start();
        }

        private Stopwatch GetStopwatchForState(PhilosopherState state)
        {
            if (!stopwatches.ContainsKey(state))
            {
                lock (padlock)
                {
                    var newStopwatch = new Stopwatch();

                    if (!stopwatches.ContainsKey(state))
                    {
                        this.stopwatches.Add(state, newStopwatch);
                    }
                }
            }

            var stopwatch = this.stopwatches[state];
            return stopwatch;
        }

        public PhilosopherState State { get; private set; }

        public bool OwnsLeftFork { get; private set; }

        public bool OwnsRightFork { get; private set; }

        public ActorRef LeftFork { get; private set; }

        public ActorRef RightFork { get; private set; }
    }
}
