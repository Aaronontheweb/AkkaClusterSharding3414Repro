//using System;
//using System.Collections.Generic;
//using System.Text;
//using Akka.Actor;
//using Akka.Event;
//using Akka.HealthCheck.Liveness;
//using Akka.Persistence;

//namespace Akka.Cluster.Sharding.Repro.Node.HealthCheck
//{
//    public class AkkaPersistenceHealthCheckSupervisor : UntypedActor, IWithUnboundedStash
//    {
//        public static Props PersistentHealthCheckProps()
//        {
//            // need to use the stopping strategy in case things blow up right away
//            return Props.Create(() => new AkkaPersistenceHealthCheckSupervisor())
//                .WithSupervisorStrategy(Actor.SupervisorStrategy.StoppingStrategy);
//        }

//        private readonly ILoggingAdapter _log = Context.GetLogger();
//        private readonly HashSet<IActorRef> _subscribers = new HashSet<IActorRef>();
//        private LivenessStatus _currentStatus = new LivenessStatus(false, "unknown");
//        private ICancelable _systemLiveTimeout;
//        private bool _journalLive;
//        private bool _snapshotStoreLive;

//        public IStash Stash { get; set; }

//        private bool HandleSubscriptions(object msg)
//        {
//            switch (msg)
//            {
//                case GetCurrentLiveness _:
//                    Sender.Tell(_currentStatus);
//                    break;
//                case SubscribeToLiveness sub:
//                    _subscribers.Add(sub.Subscriber);
//                    Context.Watch(sub.Subscriber);
//                    sub.Subscriber.Tell(_currentStatus);
//                    break;
//                case UnsubscribeFromLiveness unsub:
//                    _subscribers.Remove(unsub.Subscriber);
//                    Context.Unwatch(unsub.Subscriber);
//                    break;
//                case Terminated term:
//                    _subscribers.Remove(term.ActorRef);
//                    break;
//                default:
//                    return false;
//            }

//            return true;
//        }

//        private bool Recreating()
//        {
//            return false;
//        }

//        private bool Completed()
//        {

//        }

//        protected override void OnReceive(object message)
//        {
//            throw new NotImplementedException();
//        }

//        protected override void PreStart()
//        {
//            var probe = Context.ActorOf(Props.Create(() => new AkkaPersistenceHealthCheckProbe(Self)));
//            Context.Watch(probe);
//        }
//    }

//    /// <summary>
//    /// Validate that the snapshot store and the journal and both working
//    /// </summary>
//    public class AkkaPersistenceHealthCheckProbe : ReceivePersistentActor
//    {
//        private readonly IActorRef _probe;

//        public AkkaPersistenceHealthCheckProbe(IActorRef probe)
//        {
//            _probe = probe;
//            PersistenceId = "Akka.HealthCheck";
//        }

//        public override string PersistenceId { get; }
//    }
//}
