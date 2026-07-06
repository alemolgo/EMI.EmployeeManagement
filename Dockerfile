# ── Stage 1: Build ──────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY src/EMI.EmployeeManagement.sln .
COPY src/EMI.EmployeeManagement.API/          EMI.EmployeeManagement.API/
COPY src/EMI.EmployeeManagement.BLL/          EMI.EmployeeManagement.BLL/
COPY src/EMI.EmployeeManagement.DAL/            EMI.EmployeeManagement.DAL/
COPY src/EMI.EmployeeManagement.Entities/     EMI.EmployeeManagement.Entities/
COPY src/EMI.EmployeeManagement.Common/         EMI.EmployeeManagement.Common/

RUN dotnet restore EMI.EmployeeManagement.API/EMI.EmployeeManagement.API.csproj

RUN dotnet publish EMI.EmployeeManagement.API/EMI.EmployeeManagement.API.csproj \
    -c Release \
    -o /app/publish \
    --no-restore

# ── Stage 2: Runtime ────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "EMI.EmployeeManagement.API.dll"]
