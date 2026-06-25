using Microsoft.EntityFrameworkCore;
using Portal.Domain;
using Portal.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<PortalDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("PortalDatabase")));

builder.Services.AddHttpClient<IPortalContractService, PortalContractService>(client =>
{
    client.BaseAddress = new Uri(
        builder.Configuration["CoreApi:BaseUrl"] ?? "https://localhost:5005");
});

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
    });
});

builder.Services
    .AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", options =>
    {
        options.Authority = builder.Configuration["Auth:Authority"];
        options.Audience = builder.Configuration["Auth:Audience"];
        options.RequireHttpsMetadata = false;
    });

builder.Services.AddAuthorization();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

app.MapPost("/portal/contracts/submit",
    async (PortalContractUploadRequest request, IPortalContractService service) =>
    {
        var result = await service.SubmitAndAnalyzeAsync(request);
        return Results.Ok(result);
    }).WithName("SubmitContract").WithOpenApi();

app.MapGet("/portal/contracts",
    async (IPortalContractService service) =>
    {
        var list = await service.ListContractsAsync();
        return Results.Ok(list);
    }).WithName("ListContracts").WithOpenApi();

app.MapGet("/portal/contracts/{contractId}",
    async (string contractId, IPortalContractService service) =>
    {
        var result = await service.GetAnalysisAsync(contractId);
        return result is null ? Results.NotFound() : Results.Ok(result);
    }).WithName("GetContractAnalysis").WithOpenApi();

app.Run();
