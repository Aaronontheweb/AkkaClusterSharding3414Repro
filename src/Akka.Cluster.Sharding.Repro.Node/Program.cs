﻿using System;
using System.IO;
using System.Linq;
using System.Net;
using Akka.Actor;
using Akka.Bootstrap.Docker;
using Akka.Configuration;
using Akka.Util;
using Petabridge.Cmd.Cluster;
using Petabridge.Cmd.Cluster.Sharding;
using Petabridge.Cmd.Host;

namespace Akka.Cluster.Sharding.Repro.Node
{
    class Program
    {
        static int Main(string[] args)
        {
            var sqlConnectionString = Environment.GetEnvironmentVariable("SQL_CONNECTION_STR");
            var sqlHostName = Environment.GetEnvironmentVariable("SQL_HOSTNAME");
            if (string.IsNullOrEmpty(sqlConnectionString) || string.IsNullOrEmpty(sqlHostName))
            {
                Console.WriteLine("ERROR! No SQL Connection String specified. Exiting with code -1");
                return -1;
            }

            //var hostIp = Dns.GetHostEntry(sqlHostName);
            //sqlConnectionString = sqlConnectionString.Replace("{HOSTNAME}", hostIp.AddressList.First().ToString());
            Console.WriteLine("Connecting to SQL Server via {0}", sqlConnectionString);

            var config = ConfigurationFactory.ParseString(File.ReadAllText("sharding.hocon"));

            var sqlHocon = "akka.persistence.journal.sql-server.connection-string = \"" + sqlConnectionString + "\"" +
                           Environment.NewLine +
                           "akka.persistence.snapshot-store.sql-server.connection-string = \"" + sqlConnectionString +
                           "\"";

            Console.WriteLine("Using SQL Hocon:" + Environment.NewLine + " {0}", sqlHocon);

            var sqlConnectionConfig = ConfigurationFactory.ParseString(sqlHocon)
                .WithFallback(config);

            var actorSystem = ActorSystem.Create("ShardFight", sqlConnectionConfig.BootstrapFromDocker().WithFallback(ClusterSharding.DefaultConfig()));

            ICancelable shardTask = null;

            Cluster.Get(actorSystem).RegisterOnMemberUp(() =>
            {
                var sharding = ClusterSharding.Get(actorSystem);
                IActorRef myShardRegion = sharding.Start("entities", str => Props.Create(() => new PersistentEntityActor(str)),
                    ClusterShardingSettings.Create(actorSystem).WithRole("shard"), new FuberMessageExtractor());

                shardTask = actorSystem.Scheduler.Advanced.ScheduleRepeatedlyCancelable(TimeSpan.FromMilliseconds(250),
                    TimeSpan.FromMilliseconds(250),
                    () =>
                    {
                        myShardRegion.Tell(new FuberEnvelope(ThreadLocalRandom.Current.Next(0, 100000).ToString(), ThreadLocalRandom.Current.Next().ToString()));
                    });
            });

            var pbm = PetabridgeCmd.Get(actorSystem);
            pbm.RegisterCommandPalette(ClusterCommands.Instance); // enable cluster management commands
            pbm.RegisterCommandPalette(ClusterShardingCommands.Instance); //enable cluster.sharding management commands
            pbm.Start();

            actorSystem.WhenTerminated.Wait();
            shardTask?.Cancel(); // should already be cancelled
            return 0;
        }
    }
}
