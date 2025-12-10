var builder = DistributedApplication.CreateBuilder(args);

var grafana = builder.AddContainer("grafana", "grafana/grafana")
    .WithVolume("../grafana/config", "/etc/grafana")
    .WithVolume("../grafana/dashboards", "var/lib/grafana/dashboards")
    .WithEndpoint(3000, 3000, "http", "grafana-http");

builder.AddContainer("prometheus", "prom/prometheus")
    .WithVolume("../prometheus", "/etc/prometheus")
    .WithEndpoint(9090, 9090);

builder.AddProject<Projects.ModelBaseAPI>("modelbaseapi").WithEnvironment("GRAFANA_URL", grafana.GetEndpoint("grafana-http"));

builder.AddProject<Projects.WorkerService>("workerservice");

builder.Build().Run();