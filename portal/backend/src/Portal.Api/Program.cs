using Azure;
using Azure.AI.FormRecognizer.DocumentAnalysis;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Portal.Domain;
using Portal.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<PortalDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("PortalDatabase")));

// Register Azure Document Intelligence client if configured
var docIntelEndpoint = builder.Configuration["AzureDocumentIntelligence:Endpoint"];
var docIntelKey = builder.Configuration["AzureDocumentIntelligence:ApiKey"];
if (!string.IsNullOrEmpty(docIntelEndpoint) && !string.IsNullOrEmpty(docIntelKey))
{
    builder.Services.AddSingleton(new DocumentAnalysisClient(
        new Uri(docIntelEndpoint),
        new AzureKeyCredential(docIntelKey)));
}

builder.Services.AddHttpClient<IPortalContractService, PortalContractService>(client =>
{
    client.BaseAddress = new Uri(
        builder.Configuration["CoreApi:BaseUrl"] ?? "https://localhost:5005");
});

// Fix: Restrict CORS to configured frontend origins only
var allowedOrigins = builder.Configuration
    .GetSection("Cors:AllowedOrigins")
    .Get<string[]>() ?? new[] { "http://localhost:5173" };

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services
    .AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", options =>
    {
        options.Authority = builder.Configuration["Auth:Authority"];
        options.Audience = builder.Configuration["Auth:Audience"];
        options.RequireHttpsMetadata = !builder.Environment.IsDevelopment();
    });

builder.Services.AddAuthorization();

var app = builder.Build();

// Fix: Gate Swagger behind development environment check
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

// Fix: Enforce [Authorize] on all endpoints
app.MapPost("/portal/contracts/submit",
    [Authorize] async (PortalContractUploadRequest request, IPortalContractService service) =>
    {
        var result = await service.SubmitAndAnalyzeAsync(request);
        return Results.Ok(result);
    }).WithName("SubmitContract").WithOpenApi();

app.MapGet("/portal/contracts",
    [Authorize] async (IPortalContractService service) =>
    {
        var list = await service.ListContractsAsync();
        return Results.Ok(list);
    }).WithName("ListContracts").WithOpenApi();

app.MapGet("/portal/contracts/{contractId}",
    [Authorize] async (string contractId, IPortalContractService service) =>
    {
        var result = await service.GetAnalysisAsync(contractId);
        return result is null ? Results.NotFound() : Results.Ok(result);
    }).WithName("GetContractAnalysis").WithOpenApi();

app.Run();
