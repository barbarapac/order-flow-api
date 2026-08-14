FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY OrderFlow.sln ./
COPY src/OrderFlow.Domain/OrderFlow.Domain.csproj src/OrderFlow.Domain/
COPY src/OrderFlow.Application/OrderFlow.Application.csproj src/OrderFlow.Application/
COPY src/OrderFlow.Infrastructure/OrderFlow.Infrastructure.csproj src/OrderFlow.Infrastructure/
COPY src/OrderFlow.WebApi/OrderFlow.WebApi.csproj src/OrderFlow.WebApi/
COPY test/OrderFlow.UnitTest/OrderFlow.UnitTest.csproj test/OrderFlow.UnitTest/
RUN dotnet restore OrderFlow.sln

COPY . .
RUN dotnet publish src/OrderFlow.WebApi/OrderFlow.WebApi.csproj \
    --configuration Release \
    --no-restore \
    --output /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "OrderFlow.WebApi.dll"]
