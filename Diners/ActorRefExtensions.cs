namespace Diners
{
    using Akka.Actor;

    public static class ActorRefExtensions
    {
        public static string Name(this ActorRef actor)
        {
            var result = actor.Path.Name;
            return result;
        }
    }
}
