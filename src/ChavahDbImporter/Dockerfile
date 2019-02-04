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

COPY src/ChavahDbImporter/hc.sh /opt/app/hc.sh
RUN chmod +x /opt/app/hc.sh
HEALTHCHECK --start-period=60s CMD /opt/app/hc.sh

WORKDIR /opt/RavenDB/Server
VOLUME /opt/RavenDB/Server/RavenData /opt/RavenDB/config

#WORKDIR /opt/app
COPY src/ChavahDbImporter/start.sh /opt/app/start.sh
RUN chmod +x /opt/app/start.sh

COPY --from=publish /app /opt/app
RUN chmod +x /opt/app/ChavahDbImporter

CMD [ "/opt/app/start.sh" ]
# CMD ./run-raven2.sh
# ENTRYPOINT [ "sh","-c","opt/app/start.sh" ]
# ENTRYPOINT [ "sh", "-c", "/opt/RavenDB/run-raven.sh && /opt/app/ChavahDbImporter" ] 

# debug only
# ENTRYPOINT ["tail", "-f", "/dev/null"]

#CMD [ "/opt/app/ChavahDbImporter"] 
