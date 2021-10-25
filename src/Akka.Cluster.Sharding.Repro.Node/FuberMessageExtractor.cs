using System;
using System.Collections.Generic;
using System.Text;

namespace Akka.Cluster.Sharding.Repro.Node
{
    public sealed class FuberEnvelope
    {
        public FuberEnvelope(string fuberId, string fuberItem)
        {
            FuberId = fuberId;
            FuberItem = fuberItem;
        }

        public string FuberId { get; }

        public string FuberItem { get; }
    }

    public class FuberMessageExtractor : HashCodeMessageExtractor
    {
        public FuberMessageExtractor() : base(50)
        {
        }

        public override string EntityId(object message)
        {
            if (message is FuberEnvelope fuber)
            {
                return fuber.FuberId;
            }

            if(message is ShardRegion.StartEntity e){
                return e.EntityId;
            }

            return null;
        }

        public override object EntityMessage(object message)
        {
            if (message is FuberEnvelope fuber)
            {
                return fuber.FuberItem;
            }

            return null;
        }
    }
}
