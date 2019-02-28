using System;
using System.IO;
using Akka.Actor;
using Akka.Bootstrap.Docker;
using Akka.Configuration;
using Akka.Util;
using Petabridge.Cmd.Cluster;
using Petabridge.Cmd.Host;

namespace Akka.Cluster.Sharding.Repro.Node
{
    class Program
    {
        static int Main(string[] args)
        {
            var sqlConnectionString = Environment.GetEnvironmentVariable("SQL_CONNECTION_STR");
            if (string.IsNullOrEmpty(sqlConnectionString) || string.IsNullOrWhiteSpace(sqlConnectionString))
            {
                Console.WriteLine("ERROR! No SQL Connection String specified. Exiting with code -1");
                return -1;
            }

            var config = ConfigurationFactory.ParseString(File.ReadAllText("sharding.hocon"));

            var sqlConnectionConfig = ConfigurationFactory.ParseString(
                "akka.persistence.journal.sql-server.connection-string = \"" + sqlConnectionString + "\"" +
                Environment.NewLine +
                "akka.persistence.snapshot-store.sql-server.connection-string = \"" + sqlConnectionString + "\"")
                .WithFallback(config);

            var actorSystem = ActorSystem.Create("ShardFight", sqlConnectionConfig.BootstrapFromDocker().WithFallback(ClusterSharding.DefaultConfig()));

            ICancelable shardTask = null;

            Cluster.Get(actorSystem).RegisterOnMemberUp(() =>
            {
                var sharding = ClusterSharding.Get(actorSystem);
                var myShardRegion = sharding.Start("fubers", str => Props.Create(() => new PersistentEntityActor(str)),
                    ClusterShardingSettings.Create(actorSystem).WithRole("shards"), new FuberMessageExtractor());

                shardTask = actorSystem.Scheduler.Advanced.ScheduleRepeatedlyCancelable(TimeSpan.FromMilliseconds(250),
                    TimeSpan.FromMilliseconds(250),
                    () =>
                    {
                        myShardRegion.Tell(new FuberEnvelope(ThreadLocalRandom.Current.Next().ToString(), ThreadLocalRandom.Current.Next().ToString()));
                    });
            });

            var pbm = PetabridgeCmd.Get(actorSystem);
            pbm.RegisterCommandPalette(ClusterCommands.Instance); // enable cluster management commands
            pbm.Start();

            actorSystem.WhenTerminated.Wait();
            shardTask?.Cancel(); // should already be cancelled
            return 0;
        }
    }
}
