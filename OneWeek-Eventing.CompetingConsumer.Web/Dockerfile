FROM mcr.microsoft.com/dotnet/core/aspnet:2.1-stretch-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:2.1-stretch AS build
WORKDIR /src
COPY ["OneWeek-Eventing.CompetingConsumer.Web/OneWeek-Eventing.CompetingConsumer.Web.csproj", "OneWeek-Eventing.CompetingConsumer.Web/"]
RUN dotnet restore "OneWeek-Eventing.CompetingConsumer.Web/OneWeek-Eventing.CompetingConsumer.Web.csproj"
COPY . .
WORKDIR "/src/OneWeek-Eventing.CompetingConsumer.Web"
RUN dotnet build "OneWeek-Eventing.CompetingConsumer.Web.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "OneWeek-Eventing.CompetingConsumer.Web.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "OneWeek-Eventing.CompetingConsumer.Web.dll"]