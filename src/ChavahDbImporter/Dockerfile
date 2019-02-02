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

ENV RAVEN_Setup_Mode='None'
ENV RAVEN_Security_UnsecuredAccessAllowed='Local'
ENV RAVEN_License_Eula_Accepted=true
ENV RAVEN_ARGS="--log-to-console"

COPY src/ChavahDbImporter/healthchec2.sh /opt/RavenDB/

HEALTHCHECK --start-period=60s CMD /opt/RavenDB/healthcheck2.sh

WORKDIR /opt/RavenDB/Server
VOLUME /opt/RavenDB/Server/RavenData /opt/RavenDB/config

COPY src/ChavahDbImporter/run-raven2.sh /opt/RavenDB/Server/

COPY --from=publish /app /opt/app
RUN chmod +x /opt/app/ChavahDbImporter

RUN chmod +x /opt/RavenDB/healthchec2.sh
RUN chmod +x /opt/RavenDB/Server/run-raven2.sh

# CMD ./run-raven2.sh
# CMD /opt/RavenDB/run-raven.sh
ENTRYPOINT [ "sh", "-c", "/opt/RavenDB/run-raven.sh && /opt/app/ChavahDbImporter" ] 

# debug only
# ENTRYPOINT ["tail", "-f", "/dev/null"]

# CMD [ "/opt/app/ChavahDbImporter"] 
