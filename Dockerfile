FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime

RUN apt-get update \
    && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*    

WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

WORKDIR /src

COPY Directory.Packages.props .

COPY src/HrAgencySystem.Api/HrAgencySystem.Api.csproj HrAgencySystem.Api/
COPY src/EmailTemplates/HrAgencySystem.EmailTemplates.Contracts/HrAgencySystem.EmailTemplates.Contracts.csproj EmailTemplates/HrAgencySystem.EmailTemplates.Contracts/
COPY src/EmailTemplates/HrAgencySystem.EmailTemplates.Messaging/HrAgencySystem.EmailTemplates.Messaging.csproj EmailTemplates/HrAgencySystem.EmailTemplates.Messaging/
COPY src/HrAgencySystem.Agency/HrAgencySystem.Agency.csproj HrAgencySystem.Agency/
COPY src/HrAgencySystem.Audit/HrAgencySystem.Audit.csproj HrAgencySystem.Audit/
COPY src/HrAgencySystem.Company/HrAgencySystem.Company.csproj HrAgencySystem.Company/
COPY src/HrAgencySystem.Compliance/HrAgencySystem.Compliance.csproj HrAgencySystem.Compliance/
COPY src/HrAgencySystem.Feeds/HrAgencySystem.Feeds.csproj HrAgencySystem.Feeds/
COPY src/HrAgencySystem.Files/HrAgencySystem.Files.csproj HrAgencySystem.Files/
COPY src/HrAgencySystem.Forms/HrAgencySystem.Forms.csproj HrAgencySystem.Forms/
COPY src/HrAgencySystem.Identity/HrAgencySystem.Identity.csproj HrAgencySystem.Identity/
COPY src/HrAgencySystem.JobDescription/HrAgencySystem.JobDescription.csproj HrAgencySystem.JobDescription/
COPY src/HrAgencySystem.Organization/HrAgencySystem.Organization.csproj HrAgencySystem.Organization/
COPY src/HrAgencySystem.PlatformSeeder/HrAgencySystem.PlatformSeeder.csproj HrAgencySystem.PlatformSeeder/
COPY src/HrAgencySystem.Recruitment/HrAgencySystem.Recruitment.csproj HrAgencySystem.Recruitment/
COPY src/HrAgencySystem.Recruitment.Contracts/HrAgencySystem.Recruitment.Contracts.csproj HrAgencySystem.Recruitment.Contracts/
COPY src/HrAgencySystem.Reports.ReadModel/HrAgencySystem.Reports.ReadModel.csproj HrAgencySystem.Reports.ReadModel/
COPY src/HrAgencySystem.Sales/HrAgencySystem.Sales.csproj HrAgencySystem.Sales/
COPY src/HrAgencySystem.LegalEntities/HrAgencySystem.LegalEntities.csproj HrAgencySystem.LegalEntities/
COPY src/HrAgencySystem.Teams/HrAgencySystem.Teams.csproj HrAgencySystem.Teams/
COPY src/HrAgencySystem.Teams.Contracts/HrAgencySystem.Teams.Contracts.csproj HrAgencySystem.Teams.Contracts/
COPY src/HrAgencySystem.Projects/HrAgencySystem.Projects.csproj HrAgencySystem.Projects/
COPY src/HrAgencySystem.Projects.Contracts/HrAgencySystem.Projects.Contracts.csproj HrAgencySystem.Projects.Contracts/
COPY src/HrAgencySystem.Workers/HrAgencySystem.Workers.csproj HrAgencySystem.Workers/
COPY src/HrAgencySystem.Workers.Contracts/HrAgencySystem.Workers.Contracts.csproj HrAgencySystem.Workers.Contracts/
COPY src/services/HrAgencySystem.FileService.Contracts/HrAgencySystem.FileService.Contracts.csproj services/HrAgencySystem.FileService.Contracts/
COPY src/services/HrAgencySystem.ReportsService.Contracts/HrAgencySystem.ReportsService.Contracts.csproj services/HrAgencySystem.ReportsService.Contracts/
COPY src/HrAgencySystem.SharedKernel/HrAgencySystem.SharedKernel.csproj HrAgencySystem.SharedKernel/

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