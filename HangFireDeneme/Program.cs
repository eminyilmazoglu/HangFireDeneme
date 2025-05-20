using Hangfire;
using Hangfire.MemoryStorage;
using HangFireDeneme.Utils;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Hangfire servisini ekle (Hafizada Tut...)
builder.Services.AddHangfire(config =>
    config.UseMemoryStorage());

// Bu sekilde tanimlandıginda level siralamasi olmaz...
// builder.Services.AddHangfireServer();

builder.Services.AddHangfireServer(options =>
{
    // Buradaki sira önemlidir! Önce 'critical', sonra 'high', sonra digerleri dinlenir.
    options.Queues = new[] {
        JobQueues.Critical,
        JobQueues.High,
        JobQueues.Normal,
        JobQueues.Low
    };
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Hangfire dashboard'u aktif et
app.UseHangfireDashboard(); // /hangfire endpoint'i

// Basit bir iş tanımı (1 dakikada bir çalışır)
RecurringJob.AddOrUpdate("hello-job", () => Console.WriteLine("*** Merhaba! Bu job her dakika çalışır."), Cron.Minutely); // Basit tanimlama hic bir zaman success olmayacak bir Level tanimi yok!

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
