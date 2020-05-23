#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY ["Ocelot.Configuration.UI/Ocelot.Configuration.UI.csproj", "Ocelot.Configuration.UI/"]
RUN dotnet restore "Ocelot.Configuration.UI/Ocelot.Configuration.UI.csproj"
COPY . .
WORKDIR "/src/Ocelot.Configuration.UI"
RUN dotnet build "Ocelot.Configuration.UI.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Ocelot.Configuration.UI.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Ocelot.Configuration.UI.dll"]