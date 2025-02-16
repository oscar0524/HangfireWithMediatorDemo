FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

USER app
FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG configuration=Release
WORKDIR /src
COPY ["HangfireWithMediator/HangfireWithMediator.csproj", "HangfireWithMediator/"]
RUN dotnet restore "HangfireWithMediator/HangfireWithMediator.csproj"
COPY . .
WORKDIR "/src/HangfireWithMediator"
RUN dotnet build "HangfireWithMediator.csproj" -c $configuration -o /app/build

FROM build AS publish
ARG configuration=Release
RUN dotnet publish "HangfireWithMediator.csproj" -c $configuration -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "HangfireWithMediator.dll"]
