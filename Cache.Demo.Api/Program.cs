using Cache.Demo.Api.MessageHandlers;
using Cache.Demo.Api.Options;
using Cache.Demo.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<HttpETagCacheHandler>();             // uses ConcurrentDictionary
builder.Services.AddScoped<HttpCacheHeadersMessageHandler>();   // using IMemoryCache
builder.Services
    .AddHttpClient<IEtagApiClient, EtagApiClient>()
    .AddHttpMessageHandler<HttpCacheHeadersMessageHandler>();

builder.Services.AddMemoryCache();
builder.Services.AddHttpContextAccessor();
builder.Services.Configure<ETagClientOptions>(builder.Configuration.GetSection(ETagClientOptions.SectionName));
builder.Services.Configure<CachingOptions>(builder.Configuration.GetSection(CachingOptions.SectionName));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
