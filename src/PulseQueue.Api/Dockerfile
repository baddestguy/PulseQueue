FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY PulseQueue.sln ./
COPY src/PulseQueue.Api/PulseQueue.Api.csproj src/PulseQueue.Api/
COPY src/PulseQueue.Domain/PulseQueue.Domain.csproj src/PulseQueue.Domain/
COPY src/PulseQueue.Infrastructure/PulseQueue.Infrastructure.csproj src/PulseQueue.Infrastructure/
RUN dotnet restore src/PulseQueue.Api/PulseQueue.Api.csproj
COPY . .
RUN dotnet publish src/PulseQueue.Api/PulseQueue.Api.csproj -c Release -o /app/publish --no-restore

FROM runtime AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "PulseQueue.Api.dll"]
