FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime

RUN apt-get update \
    && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*    

WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

WORKDIR /src

COPY Directory.Packages.props .

COPY src/HrAgencySystem.Api/HrAgencySystem.Api.csproj HrAgencySystem.Api/
COPY src/HrAgencySystem.Audit/HrAgencySystem.Audit.csproj HrAgencySystem.Audit/
COPY src/HrAgencySystem.Company/HrAgencySystem.Company.csproj HrAgencySystem.Company/
COPY src/HrAgencySystem.Files/HrAgencySystem.Files.csproj HrAgencySystem.Files/
COPY src/HrAgencySystem.Identity/HrAgencySystem.Identity.csproj HrAgencySystem.Identity/
COPY src/HrAgencySystem.JobDescription/HrAgencySystem.JobDescription.csproj HrAgencySystem.JobDescription/
COPY src/HrAgencySystem.Organization/HrAgencySystem.Organization.csproj HrAgencySystem.Organization/
COPY src/HrAgencySystem.PlatformSeeder/HrAgencySystem.PlatformSeeder.csproj HrAgencySystem.PlatformSeeder/
COPY src/HrAgencySystem.Recruitment/HrAgencySystem.Recruitment.csproj HrAgencySystem.Recruitment/
COPY src/HrAgencySystem.Recruitment.Contracts/HrAgencySystem.Recruitment.Contracts.csproj HrAgencySystem.Recruitment.Contracts/
COPY src/HrAgencySystem.Sales/HrAgencySystem.Sales.csproj HrAgencySystem.Sales/
COPY src/HrAgencySystem.SharedKernel/HrAgencySystem.SharedKernel.csproj HrAgencySystem.SharedKernel/
COPY src/HrAgencySystem.Suggestion/HrAgencySystem.Suggestion.csproj HrAgencySystem.Suggestion/

RUN dotnet restore HrAgencySystem.Api/HrAgencySystem.Api.csproj

COPY src .

RUN dotnet publish HrAgencySystem.Api/HrAgencySystem.Api.csproj \
    --configuration Release \
    --no-restore \
    --output /app/publish \
    /p:UseAppHost=false

FROM runtime AS final

WORKDIR /app

COPY --from=build --chown=app:app /app/publish .

USER app

ENV ASPNETCORE_HTTP_PORTS=8080

EXPOSE 8080

ENTRYPOINT ["dotnet", "HrAgencySystem.Api.dll"]