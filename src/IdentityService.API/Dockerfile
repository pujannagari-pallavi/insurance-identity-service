FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY src/IdentityService.API/IdentityService.API.csproj src/IdentityService.API/
COPY src/IdentityService.Application/IdentityService.Application.csproj src/IdentityService.Application/
COPY src/IdentityService.Domain/IdentityService.Domain.csproj src/IdentityService.Domain/
COPY src/IdentityService.Infrastructure/IdentityService.Infrastructure.csproj src/IdentityService.Infrastructure/
RUN dotnet restore src/IdentityService.API/IdentityService.API.csproj

COPY src/ src/
RUN dotnet publish src/IdentityService.API/IdentityService.API.csproj --configuration Release --output /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["sh", "-c", "if [ -n \"$PORT\" ]; then exec dotnet IdentityService.API.dll --urls http://0.0.0.0:$PORT; else exec dotnet IdentityService.API.dll; fi"]