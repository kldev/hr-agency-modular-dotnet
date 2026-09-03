using HrAgencySystem.Recruitment.Documents;
using HrAgencySystem.SharedKernel.Port;
using Marten;

namespace HrAgencySystem.Recruitment.Infrastructure.Persistence;

public sealed class TagSeeder(IDocumentSession session) : ISeeder
{
    public async Task SeedAsync(CancellationToken ct)
    {
        if (await session.Query<Tag>().AnyAsync(ct))
            return;

        CreateLanguageTags();
        CreateItSkillTags();
        CreateProductionSkillTags();
        CreateDrivingLicenseTags();

        await session.SaveChangesAsync(ct);
    }

    private void CreateLanguageTags()
    {
        CreateTag("PL", "Polish", TagCategory.Language);
        CreateTag("EN", "English", TagCategory.Language);
        CreateTag("DE", "German", TagCategory.Language);
        CreateTag("FR", "French", TagCategory.Language);
        CreateTag("NL", "Dutch", TagCategory.Language);
        CreateTag("UA", "Ukrainian", TagCategory.Language);
        CreateTag("RU", "Russian", TagCategory.Language);
        CreateTag("RO", "Romanian", TagCategory.Language);
        CreateTag("BG", "Bulgarian", TagCategory.Language);
        CreateTag("SK", "Slovak", TagCategory.Language);
        CreateTag("CS", "Czech", TagCategory.Language);
        CreateTag("ES", "Spanish", TagCategory.Language);
        CreateTag("IT", "Italian", TagCategory.Language);
        CreateTag("HU", "Hungarian", TagCategory.Language);
    }


    private void CreateItSkillTags()
    {
        // Languages
        CreateTag("JAVA", "Java", TagCategory.SkillIt);
        CreateTag("C_SHARP", "C#", TagCategory.SkillIt);
        CreateTag("C_PLUS_PLUS", "C++", TagCategory.SkillIt);
        CreateTag("C", "C", TagCategory.SkillIt);
        CreateTag("JAVASCRIPT", "JavaScript", TagCategory.SkillIt);
        CreateTag("TYPESCRIPT", "TypeScript", TagCategory.SkillIt);
        CreateTag("PYTHON", "Python", TagCategory.SkillIt);
        CreateTag("PHP", "PHP", TagCategory.SkillIt);
        CreateTag("KOTLIN", "Kotlin", TagCategory.SkillIt);
        CreateTag("SWIFT", "Swift", TagCategory.SkillIt);
        CreateTag("GO", "Go", TagCategory.SkillIt);
        CreateTag("RUST", "Rust", TagCategory.SkillIt);
        CreateTag("RUBY", "Ruby", TagCategory.SkillIt);
        CreateTag("SCALA", "Scala", TagCategory.SkillIt);
        CreateTag("DART", "Dart", TagCategory.SkillIt);
        CreateTag("R", "R", TagCategory.SkillIt);
        CreateTag("POWERSHELL", "PowerShell", TagCategory.SkillIt);
        CreateTag("BASH", "Bash", TagCategory.SkillIt);

        // .NET
        CreateTag("DOTNET", ".NET", TagCategory.SkillIt);
        CreateTag("DOTNET_CORE", ".NET Core", TagCategory.SkillIt);
        CreateTag("ASP_NET_CORE", "ASP.NET Core", TagCategory.SkillIt);
        CreateTag("ENTITY_FRAMEWORK", "Entity Framework", TagCategory.SkillIt);
        CreateTag("EF_CORE", "Entity Framework Core", TagCategory.SkillIt);
        CreateTag("DAPPER", "Dapper", TagCategory.SkillIt);
        CreateTag("BLAZOR", "Blazor", TagCategory.SkillIt);
        CreateTag("MAUI", ".NET MAUI", TagCategory.SkillIt);
        CreateTag("WPF", "WPF", TagCategory.SkillIt);
        CreateTag("WINFORMS", "WinForms", TagCategory.SkillIt);

        // Java / JVM
        CreateTag("SPRING", "Spring", TagCategory.SkillIt);
        CreateTag("SPRING_BOOT", "Spring Boot", TagCategory.SkillIt);
        CreateTag("SPRING_CLOUD", "Spring Cloud", TagCategory.SkillIt);
        CreateTag("HIBERNATE", "Hibernate", TagCategory.SkillIt);
        CreateTag("JPA", "JPA", TagCategory.SkillIt);
        CreateTag("MICRONAUT", "Micronaut", TagCategory.SkillIt);
        CreateTag("QUARKUS", "Quarkus", TagCategory.SkillIt);
        CreateTag("GRADLE", "Gradle", TagCategory.SkillIt);
        CreateTag("MAVEN", "Maven", TagCategory.SkillIt);

        // Frontend
        CreateTag("REACT", "React", TagCategory.SkillIt);
        CreateTag("NEXTJS", "Next.js", TagCategory.SkillIt);
        CreateTag("ANGULAR", "Angular", TagCategory.SkillIt);
        CreateTag("VUE", "Vue.js", TagCategory.SkillIt);
        CreateTag("NUXT", "Nuxt", TagCategory.SkillIt);
        CreateTag("SVELTE", "Svelte", TagCategory.SkillIt);
        CreateTag("VITE", "Vite", TagCategory.SkillIt);
        CreateTag("HTML", "HTML", TagCategory.SkillIt);
        CreateTag("CSS", "CSS", TagCategory.SkillIt);
        CreateTag("SASS", "Sass", TagCategory.SkillIt);
        CreateTag("WEBPACK", "Webpack", TagCategory.SkillIt);
        CreateTag("REDUX", "Redux", TagCategory.SkillIt);
        CreateTag("RXJS", "RxJS", TagCategory.SkillIt);

        // Databases
        CreateTag("SQL", "SQL", TagCategory.SkillIt);
        CreateTag("POSTGRESQL", "PostgreSQL", TagCategory.SkillIt);
        CreateTag("MYSQL", "MySQL", TagCategory.SkillIt);
        CreateTag("MARIADB", "MariaDB", TagCategory.SkillIt);
        CreateTag("SQL_SERVER", "Microsoft SQL Server", TagCategory.SkillIt);
        CreateTag("ORACLE", "Oracle Database", TagCategory.SkillIt);
        CreateTag("MONGODB", "MongoDB", TagCategory.SkillIt);
        CreateTag("REDIS", "Redis", TagCategory.SkillIt);
        CreateTag("ELASTICSEARCH", "Elasticsearch", TagCategory.SkillIt);
        CreateTag("OPENSEARCH", "OpenSearch", TagCategory.SkillIt);
        CreateTag("CASSANDRA", "Cassandra", TagCategory.SkillIt);
        CreateTag("DYNAMODB", "DynamoDB", TagCategory.SkillIt);
        CreateTag("NEO4J", "Neo4j", TagCategory.SkillIt);
        CreateTag("MARTEN", "Marten", TagCategory.SkillIt);

        // Cloud
        CreateTag("AWS", "AWS", TagCategory.SkillIt);
        CreateTag("AZURE", "Microsoft Azure", TagCategory.SkillIt);
        CreateTag("GCP", "Google Cloud Platform", TagCategory.SkillIt);
        CreateTag("ORACLE_CLOUD", "Oracle Cloud", TagCategory.SkillIt);
        CreateTag("IBM_CLOUD", "IBM Cloud", TagCategory.SkillIt);

        // AWS
        CreateTag("AWS_EC2", "AWS EC2", TagCategory.SkillIt);
        CreateTag("AWS_S3", "AWS S3", TagCategory.SkillIt);
        CreateTag("AWS_LAMBDA", "AWS Lambda", TagCategory.SkillIt);
        CreateTag("AWS_ECS", "AWS ECS", TagCategory.SkillIt);
        CreateTag("AWS_EKS", "AWS EKS", TagCategory.SkillIt);
        CreateTag("AWS_RDS", "AWS RDS", TagCategory.SkillIt);
        CreateTag("AWS_CLOUDWATCH", "AWS CloudWatch", TagCategory.SkillIt);
        CreateTag("AWS_CLOUDFORMATION", "AWS CloudFormation", TagCategory.SkillIt);

        // Azure
        CreateTag("AZURE_APP_SERVICE", "Azure App Service", TagCategory.SkillIt);
        CreateTag("AZURE_FUNCTIONS", "Azure Functions", TagCategory.SkillIt);
        CreateTag("AZURE_AKS", "Azure Kubernetes Service", TagCategory.SkillIt);
        CreateTag("AZURE_DEVOPS", "Azure DevOps", TagCategory.SkillIt);
        CreateTag("AZURE_STORAGE", "Azure Storage", TagCategory.SkillIt);
        CreateTag("AZURE_SQL", "Azure SQL", TagCategory.SkillIt);
        CreateTag("AZURE_KEY_VAULT", "Azure Key Vault", TagCategory.SkillIt);
        CreateTag("AZURE_COSMOS_DB", "Azure Cosmos DB", TagCategory.SkillIt);
        CreateTag("AZURE_CONTAINER_APPS", "Azure Container Apps", TagCategory.SkillIt);

        // GCP
        CreateTag("GCP_COMPUTE_ENGINE", "Google Compute Engine", TagCategory.SkillIt);
        CreateTag("GCP_CLOUD_RUN", "Google Cloud Run", TagCategory.SkillIt);
        CreateTag("GCP_CLOUD_FUNCTIONS", "Google Cloud Functions", TagCategory.SkillIt);
        CreateTag("GCP_GKE", "Google Kubernetes Engine", TagCategory.SkillIt);
        CreateTag("GCP_CLOUD_STORAGE", "Google Cloud Storage", TagCategory.SkillIt);
        CreateTag("GCP_CLOUD_SQL", "Google Cloud SQL", TagCategory.SkillIt);
        CreateTag("GCP_PUB_SUB", "Google Cloud Pub/Sub", TagCategory.SkillIt);
        CreateTag("GCP_BIGQUERY", "Google BigQuery", TagCategory.SkillIt);
        CreateTag("GCP_VERTEX_AI", "Google Vertex AI", TagCategory.SkillIt);

        // DevOps / Containers
        CreateTag("DOCKER", "Docker", TagCategory.SkillIt);
        CreateTag("KUBERNETES", "Kubernetes", TagCategory.SkillIt);
        CreateTag("HELM", "Helm", TagCategory.SkillIt);
        CreateTag("TERRAFORM", "Terraform", TagCategory.SkillIt);
        CreateTag("ANSIBLE", "Ansible", TagCategory.SkillIt);
        CreateTag("PULUMI", "Pulumi", TagCategory.SkillIt);
        CreateTag("ARGOCD", "Argo CD", TagCategory.SkillIt);
        CreateTag("JENKINS", "Jenkins", TagCategory.SkillIt);
        CreateTag("GITHUB_ACTIONS", "GitHub Actions", TagCategory.SkillIt);
        CreateTag("GITLAB_CI", "GitLab CI/CD", TagCategory.SkillIt);
        CreateTag("CIRCLECI", "CircleCI", TagCategory.SkillIt);
        CreateTag("TEAMCITY", "TeamCity", TagCategory.SkillIt);

        // Version control
        CreateTag("GIT", "Git", TagCategory.SkillIt);
        CreateTag("GITHUB", "GitHub", TagCategory.SkillIt);
        CreateTag("GITLAB", "GitLab", TagCategory.SkillIt);
        CreateTag("BITBUCKET", "Bitbucket", TagCategory.SkillIt);

        // AI / Machine Learning
        CreateTag("AI", "Artificial Intelligence", TagCategory.SkillIt);
        CreateTag("MACHINE_LEARNING", "Machine Learning", TagCategory.SkillIt);
        CreateTag("DEEP_LEARNING", "Deep Learning", TagCategory.SkillIt);
        CreateTag("GENERATIVE_AI", "Generative AI", TagCategory.SkillIt);
        CreateTag("LLM", "Large Language Models", TagCategory.SkillIt);
        CreateTag("NLP", "Natural Language Processing", TagCategory.SkillIt);
        CreateTag("COMPUTER_VISION", "Computer Vision", TagCategory.SkillIt);
        CreateTag("OPENAI", "OpenAI", TagCategory.SkillIt);
        CreateTag("AZURE_OPENAI", "Azure OpenAI", TagCategory.SkillIt);
        CreateTag("ANTHROPIC", "Anthropic", TagCategory.SkillIt);
        CreateTag("GEMINI", "Google Gemini", TagCategory.SkillIt);
        CreateTag("CLAUDE", "Claude", TagCategory.SkillIt);
        CreateTag("LANGCHAIN", "LangChain", TagCategory.SkillIt);
        CreateTag("LANGGRAPH", "LangGraph", TagCategory.SkillIt);
        CreateTag("LLAMA_INDEX", "LlamaIndex", TagCategory.SkillIt);
        CreateTag("HUGGING_FACE", "Hugging Face", TagCategory.SkillIt);
        CreateTag("PYTORCH", "PyTorch", TagCategory.SkillIt);
        CreateTag("TENSORFLOW", "TensorFlow", TagCategory.SkillIt);
        CreateTag("SCIKIT_LEARN", "scikit-learn", TagCategory.SkillIt);
        CreateTag("PANDAS", "Pandas", TagCategory.SkillIt);
        CreateTag("NUMPY", "NumPy", TagCategory.SkillIt);
        CreateTag("MLFLOW", "MLflow", TagCategory.SkillIt);
        CreateTag("RAG", "Retrieval-Augmented Generation", TagCategory.SkillIt);
        CreateTag("VECTOR_DATABASE", "Vector Database", TagCategory.SkillIt);
        CreateTag("PROMPT_ENGINEERING", "Prompt Engineering", TagCategory.SkillIt);

        // Messaging / Event-driven
        CreateTag("KAFKA", "Apache Kafka", TagCategory.SkillIt);
        CreateTag("RABBITMQ", "RabbitMQ", TagCategory.SkillIt);
        CreateTag("AZURE_SERVICE_BUS", "Azure Service Bus", TagCategory.SkillIt);
        CreateTag("AWS_SQS", "Amazon SQS", TagCategory.SkillIt);
        CreateTag("AWS_SNS", "Amazon SNS", TagCategory.SkillIt);
        CreateTag("GOOGLE_PUBSUB", "Google Pub/Sub", TagCategory.SkillIt);
        CreateTag("NATS", "NATS", TagCategory.SkillIt);
        CreateTag("MASS_TRANSIT", "MassTransit", TagCategory.SkillIt);

        // Architecture / Backend
        CreateTag("REST_API", "REST API", TagCategory.SkillIt);
        CreateTag("GRAPHQL", "GraphQL", TagCategory.SkillIt);
        CreateTag("GRPC", "gRPC", TagCategory.SkillIt);
        CreateTag("MICROSERVICES", "Microservices", TagCategory.SkillIt);
        CreateTag("MODULAR_MONOLITH", "Modular Monolith", TagCategory.SkillIt);
        CreateTag("CLEAN_ARCHITECTURE", "Clean Architecture", TagCategory.SkillIt);
        CreateTag("HEXAGONAL_ARCHITECTURE", "Hexagonal Architecture", TagCategory.SkillIt);
        CreateTag("CQRS", "CQRS", TagCategory.SkillIt);
        CreateTag("DDD", "Domain-Driven Design", TagCategory.SkillIt);
        CreateTag("EVENT_SOURCING", "Event Sourcing", TagCategory.SkillIt);
        CreateTag("EVENT_DRIVEN_ARCHITECTURE", "Event-Driven Architecture", TagCategory.SkillIt);

        // Testing
        CreateTag("UNIT_TESTING", "Unit Testing", TagCategory.SkillIt);
        CreateTag("INTEGRATION_TESTING", "Integration Testing", TagCategory.SkillIt);
        CreateTag("TESTCONTAINERS", "Testcontainers", TagCategory.SkillIt);
        CreateTag("XUNIT", "xUnit", TagCategory.SkillIt);
        CreateTag("NUNIT", "NUnit", TagCategory.SkillIt);
        CreateTag("JUNIT", "JUnit", TagCategory.SkillIt);
        CreateTag("MOCKITO", "Mockito", TagCategory.SkillIt);
        CreateTag("PLAYWRIGHT", "Playwright", TagCategory.SkillIt);
        CreateTag("SELENIUM", "Selenium", TagCategory.SkillIt);
        CreateTag("CYPRESS", "Cypress", TagCategory.SkillIt);
        CreateTag("K6", "k6", TagCategory.SkillIt);

        // Observability
        CreateTag("PROMETHEUS", "Prometheus", TagCategory.SkillIt);
        CreateTag("GRAFANA", "Grafana", TagCategory.SkillIt);
        CreateTag("ELK", "ELK Stack", TagCategory.SkillIt);
        CreateTag("ELASTIC_APM", "Elastic APM", TagCategory.SkillIt);
        CreateTag("OPENTELEMETRY", "OpenTelemetry", TagCategory.SkillIt);
        CreateTag("JAEGER", "Jaeger", TagCategory.SkillIt);
        CreateTag("ZIPKIN", "Zipkin", TagCategory.SkillIt);

        // Security / Identity
        CreateTag("OAUTH2", "OAuth 2.0", TagCategory.SkillIt);
        CreateTag("OPENID_CONNECT", "OpenID Connect", TagCategory.SkillIt);
        CreateTag("JWT", "JWT", TagCategory.SkillIt);
        CreateTag("KEYCLOAK", "Keycloak", TagCategory.SkillIt);
        CreateTag("AUTH0", "Auth0", TagCategory.SkillIt);
        CreateTag("IDENTITY_SERVER", "IdentityServer", TagCategory.SkillIt);
        CreateTag("OKTA", "Okta", TagCategory.SkillIt);

        // Mobile
        CreateTag("ANDROID", "Android", TagCategory.SkillIt);
        CreateTag("IOS", "iOS", TagCategory.SkillIt);
        CreateTag("REACT_NATIVE", "React Native", TagCategory.SkillIt);
        CreateTag("FLUTTER", "Flutter", TagCategory.SkillIt);

        // Linux / Infrastructure
        CreateTag("LINUX", "Linux", TagCategory.SkillIt);
        CreateTag("UBUNTU", "Ubuntu", TagCategory.SkillIt);
        CreateTag("DEBIAN", "Debian", TagCategory.SkillIt);
        CreateTag("RED_HAT", "Red Hat Enterprise Linux", TagCategory.SkillIt);
        CreateTag("NGINX", "NGINX", TagCategory.SkillIt);
        CreateTag("APACHE", "Apache HTTP Server", TagCategory.SkillIt);

        // Data / Big Data
        CreateTag("APACHE_SPARK", "Apache Spark", TagCategory.SkillIt);
        CreateTag("HADOOP", "Hadoop", TagCategory.SkillIt);
        CreateTag("AIRFLOW", "Apache Airflow", TagCategory.SkillIt);
        CreateTag("DATABRICKS", "Databricks", TagCategory.SkillIt);
        CreateTag("SNOWFLAKE", "Snowflake", TagCategory.SkillIt);
        CreateTag("DBT", "dbt", TagCategory.SkillIt);

        // Tools
        CreateTag("JIRA", "Jira", TagCategory.SkillIt);
        CreateTag("CONFLUENCE", "Confluence", TagCategory.SkillIt);
        CreateTag("SONARQUBE", "SonarQube", TagCategory.SkillIt);
        CreateTag("SENTRY", "Sentry", TagCategory.SkillIt);
        CreateTag("POSTMAN", "Postman", TagCategory.SkillIt);

    }

    private void CreateProductionSkillTags()
    {
        CreateTag("WELDING_MIG_MAG", "MIG/MAG welding", TagCategory.ProductionSkill);
        CreateTag("WELDING_TIG", "TIG welding", TagCategory.ProductionSkill);
        CreateTag("WELDING_MMA", "MMA welding", TagCategory.ProductionSkill);
        CreateTag("CNC_LATHE", "CNC lathe", TagCategory.ProductionSkill);
        CreateTag("CNC_MILLING", "CNC milling", TagCategory.ProductionSkill);
        CreateTag("CNC_PROGRAMMING", "CNC programming", TagCategory.ProductionSkill);
        CreateTag("FORKLIFT", "Forklift operation", TagCategory.ProductionSkill);
        CreateTag("CRANE", "Crane operation", TagCategory.ProductionSkill);
        CreateTag("PALLET_TRUCK", "Pallet truck", TagCategory.ProductionSkill);
        CreateTag("ASSEMBLY", "Assembly", TagCategory.ProductionSkill);
        CreateTag("PRODUCTION_LINE", "Production line", TagCategory.ProductionSkill);
        CreateTag("QUALITY_CONTROL", "Quality control", TagCategory.ProductionSkill);
        CreateTag("MACHINE_OPERATION", "Machine operation", TagCategory.ProductionSkill);
        CreateTag(
            "READING_TECHNICAL_DRAWINGS",
            "Technical drawing",
            TagCategory.ProductionSkill);

        CreateTag(
            "MEASURING_INSTRUMENTS",
            "Measuring instruments",
            TagCategory.ProductionSkill);

        CreateTag("PACKING", "Packing", TagCategory.ProductionSkill);
        CreateTag("WAREHOUSE", "Warehouse operations", TagCategory.ProductionSkill);
        CreateTag("PICKING", "Order picking", TagCategory.ProductionSkill);
    }

    private void CreateDrivingLicenseTags()
    {
        CreateTag("AM", "Driving license AM", TagCategory.DrivingLicense);
        CreateTag("A", "Driving license A", TagCategory.DrivingLicense);
        CreateTag("A1", "Driving license A1", TagCategory.DrivingLicense);
        CreateTag("A2", "Driving license A2", TagCategory.DrivingLicense);
        CreateTag("B", "Driving license B", TagCategory.DrivingLicense);
        CreateTag("BE", "Driving license BE", TagCategory.DrivingLicense);
        CreateTag("C", "Driving license C", TagCategory.DrivingLicense);
        CreateTag("CE", "Driving license CE", TagCategory.DrivingLicense);
        CreateTag("D", "Driving license D", TagCategory.DrivingLicense);
        CreateTag("DE", "Driving license DE", TagCategory.DrivingLicense);
        CreateTag("T", "Driving license T", TagCategory.DrivingLicense);
    }

    private void CreateTag(
        string code,
        string name,
        TagCategory category)
    {
        session.Store(
            new Tag(
                Guid.NewGuid(),
                category,
                code,
                name));
    }
}