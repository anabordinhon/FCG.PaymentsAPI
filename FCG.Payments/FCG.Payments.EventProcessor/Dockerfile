FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["FCG.Payments.EventProcessor/FCG.Payments.EventProcessor.csproj", "FCG.Payments.EventProcessor/"]
COPY ["FCG.Payments.Application/FCG.Payments.Application.csproj", "FCG.Payments.Application/"]
COPY ["FCG.Payments.Infrastructure/FCG.Payments.Infrastructure.csproj", "FCG.Payments.Infrastructure/"]
COPY ["FCG.Payments.Domain/FCG.Payments.Domain.csproj", "FCG.Payments.Domain/"]
RUN dotnet restore "FCG.Payments.EventProcessor/FCG.Payments.EventProcessor.csproj"
COPY . .
RUN dotnet build "FCG.Payments.EventProcessor/FCG.Payments.EventProcessor.csproj" -c $BUILD_CONFIGURATION --no-restore
RUN dotnet publish "FCG.Payments.EventProcessor/FCG.Payments.EventProcessor.csproj" -c $BUILD_CONFIGURATION -o /app/publish --no-build /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "FCG.Payments.EventProcessor.dll"]
