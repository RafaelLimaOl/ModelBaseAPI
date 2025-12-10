var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.ModelBaseAPI>("modelbaseapi");

builder.Build().Run();
