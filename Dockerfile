# Use the official .NET runtime as a parent image
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Use the SDK image to build the app
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["LoyaltyRewardsApi.csproj", "."]
RUN dotnet restore "LoyaltyRewardsApi.csproj"
COPY . .
WORKDIR "/src"
RUN dotnet build "LoyaltyRewardsApi.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "LoyaltyRewardsApi.csproj" -c Release -o /app/publish

# Final stage/image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Copy the SQLite database
COPY loyalty_rewards.db .

# Set environment variables
ENV ASPNETCORE_URLS=http://+:10000
ENV ASPNETCORE_ENVIRONMENT=Production

EXPOSE 10000
ENTRYPOINT ["dotnet", "LoyaltyRewardsApi.dll"]
