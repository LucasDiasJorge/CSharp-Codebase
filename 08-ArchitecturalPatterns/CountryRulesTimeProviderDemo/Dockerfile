FROM mcr.microsoft.com/dotnet/sdk:9.0-alpine AS build
WORKDIR /src

COPY CountryRulesTimeProviderDemo.csproj ./
RUN dotnet restore CountryRulesTimeProviderDemo.csproj

COPY . .
RUN dotnet publish CountryRulesTimeProviderDemo.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0-alpine AS runtime
WORKDIR /app

# Instala dados de timezone e bibliotecas de globalizacao para resolver fusos no Alpine.
RUN apk add --no-cache tzdata icu-libs
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false
ENV TZ=UTC
ENV ASPNETCORE_URLS=http://+:8080

EXPOSE 8080

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "CountryRulesTimeProviderDemo.dll"]
