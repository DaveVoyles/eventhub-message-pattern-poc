FROM mcr.microsoft.com/dotnet/core/aspnet:3.0-buster-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:3.0-buster AS build
WORKDIR /src
COPY ["OneWeek-Eventing.Filtering.Web/OneWeek-Eventing.Filtering.Web.csproj", "OneWeek-Eventing.Filtering.Web/"]
RUN dotnet restore "OneWeek-Eventing.Filtering.Web/OneWeek-Eventing.Filtering.Web.csproj"
COPY . .
WORKDIR "/src/OneWeek-Eventing.Filtering.Web"
RUN dotnet build "OneWeek-Eventing.Filtering.Web.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "OneWeek-Eventing.Filtering.Web.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "OneWeek-Eventing.Filtering.Web.dll"]