using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Akka.Persistence;

namespace Akka.Cluster.Sharding.Repro.Node
{
    public sealed class PersistentEntityActor : ReceivePersistentActor
    {
        private List<string> _currentItems = new List<string>();

        public PersistentEntityActor(string persistenceId)
        {
            PersistenceId = persistenceId;

            Recover<SnapshotOffer>(offer =>
            {
                if (offer.Snapshot is IEnumerable<string> strs)
                {
                    _currentItems = strs.ToList();
                    Log.Info("Recovered [{0}]", string.Join(",", _currentItems));
                }
            });

            Recover<string>(str =>
            {
                _currentItems.Add(str);

                Log.Info("Recovered [{0}]", str);
            });

            Command<string>(str =>
            {
                if (LastSequenceNr % 10 == 0)
                {
                    SaveSnapshot(_currentItems.ToArray());
                }

                Persist(str, s =>
                {
                    _currentItems.Add(s);
                    Log.Info("Added [{0}] - now have [{1}]", s, string.Join(",", _currentItems));
                });
            });

            Command<SaveSnapshotSuccess>(success =>
            {
                DeleteMessages(success.Metadata.SequenceNr);
                Log.Info("Successfully saved snapshot. Deleting messages up until [{0}]", success.Metadata.SequenceNr);
            });
        }

        public override string PersistenceId { get; }
    }
}
