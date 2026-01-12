FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["FCG.Payments.EventProcessor/FCG.Payments.EventProcessor.csproj", "FCG.Payments.EventProcessor/"]
COPY ["FCG.Payments.Application/FCG.Payments.Application.csproj", "FCG.Payments.Application/"]
COPY ["FCG.Payments.Infrastructure/FCG.Payments.Infrastructure.csproj", "FCG.Payments.Infrastructure/"]
COPY ["FCG.Payments.Domain/FCG.Payments.Domain.csproj", "FCG.Payments.Domain/"]

RUN dotnet restore "FCG.Payments.EventProcessor/FCG.Payments.EventProcessor.csproj"

COPY . .

WORKDIR "/src/FCG.Payments.EventProcessor"
RUN dotnet build "FCG.Payments.EventProcessor.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "FCG.Payments.EventProcessor.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "FCG.Payments.EventProcessor.dll"]