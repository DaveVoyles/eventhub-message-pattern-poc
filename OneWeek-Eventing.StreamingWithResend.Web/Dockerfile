FROM mcr.microsoft.com/dotnet/core/aspnet:3.0-buster-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:3.0-buster AS build
WORKDIR /src
COPY ["OneWeek-Eventing.StreamingWithResend.Web/OneWeek-Eventing.StreamingWithResend.Web.csproj", "OneWeek-Eventing.StreamingWithResend.Web/"]
RUN dotnet restore "OneWeek-Eventing.StreamingWithResend.Web/OneWeek-Eventing.StreamingWithResend.Web.csproj"
COPY . .
WORKDIR "/src/OneWeek-Eventing.StreamingWithResend.Web"
RUN dotnet build "OneWeek-Eventing.StreamingWithResend.Web.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "OneWeek-Eventing.StreamingWithResend.Web.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "OneWeek-Eventing.StreamingWithResend.Web.dll"]