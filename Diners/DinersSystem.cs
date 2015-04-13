
namespace Diners
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;

    using Akka.Actor;

    public class DinersSystem
    {
        private readonly ActorSystem dinersSystem;

        private readonly List<ActorRef> philosophers;

        private readonly List<ActorRef> forks;
 
        public DinersSystem()
        {
            this.dinersSystem = ActorSystem.Create("DiningPhilosophers");

            this.philosophers = new List<ActorRef>()
                                   {
                                       this.dinersSystem.ActorOf<Philosopher>("Antiphon"),
                                       this.dinersSystem.ActorOf<Philosopher>("Archimedes"),
                                       this.dinersSystem.ActorOf<Philosopher>("Euclid"),
                                       this.dinersSystem.ActorOf<Philosopher>("Hippocrates"),
                                       this.dinersSystem.ActorOf<Philosopher>("Pythagoras")
                                   };

            this.forks = new List<ActorRef>();

            ActorRef lastPhilosopher = null;
            ActorRef firstFork = null;

            var forkId = 0;

            foreach (var philosopher in this.philosophers)
            {                
                var forkRef = this.dinersSystem.ActorOf<Fork>(string.Format("Fork_{0}", forkId));

                if (firstFork == null)
                {
                    firstFork = forkRef;
                }

                philosopher.Tell(new AssignRightForkOrder(forkRef));
                if (lastPhilosopher != null)
                {
                    lastPhilosopher.Tell(new AssignLeftForkOrder(forkRef));
                }

                lastPhilosopher = philosopher;
                forkId++;
            }

            this.philosophers.Last().Tell(new AssignLeftForkOrder(firstFork));

            Console.WriteLine("Ready to feed {0} hungry philosophers...", this.philosophers.Count);
        }

        public void StartDining()
        {
            // kick things off with Pythagoras - he's hungriest
            foreach (var philosopher in this.philosophers)
            {
                philosopher.Tell(new BeginEatingOrder());
            }
        }
    }
}
