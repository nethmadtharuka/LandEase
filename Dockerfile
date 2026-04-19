FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy csproj files first for restore caching
COPY LandEase.slnx ./
COPY LandEase.API/LandEase.API.csproj LandEase.API/
COPY LandEase.Application/LandEase.Application.csproj LandEase.Application/
COPY LandEase.Domain/LandEase.Domain.csproj LandEase.Domain/
COPY LandEase.Infrastructure/LandEase.Infrastructure.csproj LandEase.Infrastructure/

RUN dotnet restore LandEase.API/LandEase.API.csproj

# Copy the rest and publish
COPY . .
RUN dotnet publish LandEase.API/LandEase.API.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
EXPOSE 8080

# ASP.NET Core in containers should listen on 0.0.0.0
ENV ASPNETCORE_URLS=http://+:8080

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "LandEase.API.dll"]

