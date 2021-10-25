FROM mcr.microsoft.com/dotnet/sdk:2.1 AS base
WORKDIR /app

# should be a comma-delimited list
ENV CLUSTER_SEEDS "[]"
ENV CLUSTER_IP ""
ENV CLUSTER_PORT "5213"

#Akka.Remote inbound listening endpoint
EXPOSE 5213 

# Install Petabridge.Cmd client
RUN dotnet tool install --global pbm 

# Needed because https://stackoverflow.com/questions/51977474/install-dotnet-core-tool-dockerfile
ENV PATH="${PATH}:/root/.dotnet/tools"

COPY ./bin/Release/netcoreapp2.1/publish/ /app

FROM mcr.microsoft.com/dotnet/runtime:2.1 AS app
WORKDIR /app

COPY --from=base /app /app

# copy .NET Core global tool
COPY --from=base /root/.dotnet /root/.dotnet/

CMD ["dotnet", "Akka.Cluster.Sharding.Repro.Node.dll"]