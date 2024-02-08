FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["notion-clone.csproj", "./"]
RUN dotnet restore "notion-clone.csproj"
COPY . .
WORKDIR "/src/"
RUN dotnet build "notion-clone.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "notion-clone.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "notion-clone.dll"]
