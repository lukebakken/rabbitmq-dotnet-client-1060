# https://hub.docker.com/_/microsoft-dotnet
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /source

# copy csproj and restore as distinct layers
COPY *.csproj .
RUN dotnet restore --use-current-runtime

# copy and publish app and libraries
COPY . .
RUN dotnet publish --configuration Release --output /app --use-current-runtime --self-contained false --no-restore

# final stage/image
FROM mcr.microsoft.com/dotnet/runtime:6.0
WORKDIR /app
COPY --from=build /app .
COPY --from=build /source/ca_certificate.pem /etc/ssl/certs
RUN ln -vs /etc/ssl/certs/ca_certificate.pem "/etc/ssl/certs/$(openssl x509 -hash -noout -in /etc/ssl/certs/ca_certificate.pem).0"
ENTRYPOINT ["dotnet", "consumer.dll"]
