#FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
#WORKDIR /app
#EXPOSE 80
#EXPOSE 443
#
#FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
#WORKDIR /src
#COPY ["notion-clone.csproj", "./"]
#RUN dotnet restore "notion-clone.csproj"
#COPY . .
#WORKDIR "/src/"
#RUN dotnet build "notion-clone.csproj" -c Release -o /app/build
#
#FROM build AS publish
#RUN dotnet publish "notion-clone.csproj" -c Release -o /app/publish /p:UseAppHost=false
#
#FROM base AS final
#WORKDIR /app
#COPY --from=publish /app/publish .
#ENTRYPOINT ["dotnet", "notion-clone.dll"]

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build-env
WORKDIR /app

# Copy everything and publish the release
COPY . ./
RUN dotnet restore
RUN dotnet publish -c Release -o out

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:7.0
WORKDIR /app
COPY --from=build-env /app/out .

# Set the environment variable
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "notion-clone.dll"]