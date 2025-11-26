var builder = DistributedApplication.CreateBuilder(args);

var api = builder.AddProject<Projects.FamilyBudgeting_API>("api");

builder.Build().Run();