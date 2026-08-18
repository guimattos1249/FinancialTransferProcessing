FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["src/FinancialTransferProcessing.API/FinancialTransferProcessing.API.csproj", "src/FinancialTransferProcessing.API/"]
COPY ["src/FinancialTransferProcessing.Application/FinancialTransferProcessing.Application.csproj", "src/FinancialTransferProcessing.Application/"]
COPY ["src/FinancialTransferProcessing.Domain/FinancialTransferProcessing.Domain.csproj", "src/FinancialTransferProcessing.Domain/"]
COPY ["src/FinancialTransferProcessing.Infrastructure/FinancialTransferProcessing.Infrastructure.csproj", "src/FinancialTransferProcessing.Infrastructure/"]
RUN dotnet restore "src/FinancialTransferProcessing.API/FinancialTransferProcessing.API.csproj"

COPY . .
RUN dotnet publish "src/FinancialTransferProcessing.API/FinancialTransferProcessing.API.csproj" \
    --configuration Release \
    --output /app/publish \
    --no-restore \
    /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

USER $APP_UID
ENTRYPOINT ["dotnet", "FinancialTransferProcessing.API.dll"]
