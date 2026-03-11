using OrbRushServer.Hubs;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSignalR();

builder.Services.AddCors(options =>
{
	options.AddDefaultPolicy(policy =>
	{
		policy.AllowAnyHeader()
			  .AllowAnyMethod()
			  .AllowCredentials()
			  .SetIsOriginAllowed(_ => true);
	});
});

var app = builder.Build();

app.UseCors();

app.MapGet("/", () => "OrbRush SignalR Server is running.");
app.MapHub<GameHub>("/gamehub");

app.Run();
