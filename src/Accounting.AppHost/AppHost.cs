var builder = DistributedApplication.CreateBuilder(args);

var sql = builder.AddSqlServer("sql")
    .WithDataVolume()
    .WithLifetime(ContainerLifetime.Persistent);

var sqlData = sql.AddDatabase("sqldata", "AccountingDb");

var apiService = builder.AddProject<Projects.Accounting_ApiService>("apiservice")
    .WithHttpHealthCheck("/health")
    .WithReference(sqlData)
    .WaitFor(sqlData);

builder.AddProject<Projects.Accounting_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/health")
    .WithReference(apiService)
    .WaitFor(apiService);


builder.Build().Run();
