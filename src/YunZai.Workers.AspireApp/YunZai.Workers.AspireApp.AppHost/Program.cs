var builder = DistributedApplication.CreateBuilder(args);

var apiService = builder.AddProject<Projects.YunZai_Workers_AspireApp_ApiService>
    ("apiservice");

builder.AddProject<Projects.YunZai_Workers_AspireApp_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithReference(apiService)
    .WaitFor(apiService);



builder.Build().Run();
