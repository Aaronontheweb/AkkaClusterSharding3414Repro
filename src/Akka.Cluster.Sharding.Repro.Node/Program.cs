using System;
using System.IO;
using Akka.Actor;
using Akka.Bootstrap.Docker;
using Akka.Configuration;

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

            var actorSystem = ActorSystem.Create("ShardFight", sqlConnectionConfig.BootstrapFromDocker());


            actorSystem.WhenTerminated.Wait();
            return 0;
        }
    }
}
