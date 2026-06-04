using NurFlac.Extensions;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddNurFlac(builder.Configuration);

var host = builder.Build();
host.Run();
