FROM microsoft/dotnet:2.2-runtime AS base
WORKDIR /app

FROM microsoft/dotnet:2.2-sdk AS build
WORKDIR /src
COPY src/ChavahDbImporter/ChavahDbImporter.csproj src/ChavahDbImporter/
RUN dotnet restore src/ChavahDbImporter/ChavahDbImporter.csproj
COPY . .
WORKDIR /src/src/ChavahDbImporter
RUN dotnet build ChavahDbImporter.csproj -c Release -o /app

FROM build AS publish
RUN dotnet publish ChavahDbImporter.csproj -c Release -r ubuntu.16.04-x64 -o /app

FROM ravendb/ravendb AS final

EXPOSE 8080 38888 161
ENV RAVEN_Setup_Mode=None
ENV RAVEN_Security_UnsecuredAccessAllowed=PublicNetwork
ENV RAVEN_License_Eula_Accepted=true
ENV RAVEN_ARGS="--log-to-console"

HEALTHCHECK --start-period=60s CMD /opt/RavenDB/healthcheck.sh
WORKDIR /opt/RavenDB/Server
VOLUME /opt/RavenDB/Server/RavenData /opt/RavenDB/config
#CMD /opt/RavenDB/run-raven.sh

#WORKDIR /app
COPY --from=publish /app ./Importer
RUN chmod 777 ./Importer/ChavahDbImporter
CMD [ "sh", "-c", "/opt/RavenDB/run-raven.sh && ./Importer/ChavahDbImporter" ] 
