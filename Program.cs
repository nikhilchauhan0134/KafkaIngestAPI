using DataCaptureApi.Data;
using DataCaptureApi.Repositories;
using Microsoft.EntityFrameworkCore;
using Confluent.Kafka; // <-- Required for Kafka

var builder = WebApplication.CreateBuilder(args);

var dbPath = Path.Combine(builder.Environment.ContentRootPath, "store.db");
builder.Services.AddControllers();

// Existing SQLite setup
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite($"Data Source={dbPath}"));
builder.Services.AddScoped<IItemRepository, ItemRepository>();

// --- NEW: KAFKA PRODUCER SETUP ---
var producerConfig = new ProducerConfig
{
    // Reads the environment variable from your docker-compose.yml, falls back to localhost
    BootstrapServers = builder.Configuration["KafkaConfig:BootstrapServers"] ?? "localhost:9092",
    
    // High-throughput optimizations
    CompressionType = CompressionType.Lz4,
    LingerMs = 10,
    BatchSize = 100000,
    Acks = Acks.Leader
};

// Register the producer as a Singleton so the connection stays open across all HTTP requests
builder.Services.AddSingleton<IProducer<Null, string>>(x => 
    new ProducerBuilder<Null, string>(producerConfig).Build());
// ---------------------------------

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
}

app.MapControllers();
app.Run();

public partial class Program { }