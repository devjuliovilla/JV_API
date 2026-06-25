FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY WebApi.slnx ./
COPY src/Domain/Domain.csproj src/Domain/
COPY src/Shared/Shared.csproj src/Shared/
COPY src/Infrastructure/Infrastructure.csproj src/Infrastructure/
COPY src/WebApi/WebApi.csproj src/WebApi/
COPY src/Tests/Tests.csproj src/Tests/
RUN dotnet restore src/WebApi/WebApi.csproj

COPY . .
RUN dotnet publish src/WebApi/WebApi.csproj -c Release --no-restore -o /publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
COPY --from=build /publish .
ENTRYPOINT ["dotnet", "WebApi.dll"]
