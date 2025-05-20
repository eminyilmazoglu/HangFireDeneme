using Hangfire;
using Hangfire.Common;
using Hangfire.States;
using HangFireDeneme.Utils;
using Microsoft.AspNetCore.Mvc;

namespace HangFireDeneme.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class HangfireJobController : ControllerBase
    {
        // Fire-and-forget job (anında çalışır)
        [HttpPost("fire-and-forget")]
        public IActionResult FireAndForget()
        {
            //BackgroundJob.Enqueue(() => Console.WriteLine("🔥 Fire-and-forget job tetiklendi!"));
            var client = new BackgroundJobClient();
            client.Create(() => Console.WriteLine("🔥 Fire-and-forget job tetiklendi!"), new EnqueuedState("low"));
            return Ok("Job gönderildi.");
        }

        // Delayed job (örnek: 10 saniye sonra çalışır)
        [HttpPost("delayed")]
        public IActionResult Delayed()
        {
            //BackgroundJob.Schedule(() => Console.WriteLine("⏳ Bu job 10 saniye sonra çalıştı."), TimeSpan.FromSeconds(10));
            var client = new BackgroundJobClient();
            var job = Job.FromExpression(() => Console.WriteLine("🔥 Zamanlanmış job"));
            var jobId = client.Create(job, new ScheduledState(TimeSpan.FromSeconds(10)));

            // Kuyruk bilgisini ayrıca ekle
            client.ChangeState(jobId, new EnqueuedState("high")); // Level belirle...

            return Ok("10 saniye gecikmeli job gönderildi.");
        }

        // Recurring job (her dakika çalışır, günceller)
        [HttpPost("recurring")]
        public IActionResult Recurring()
        {
            RecurringJob.AddOrUpdate("recurring-hello", () => Console.WriteLine("🔁 Bu job her dakika çalışır."), Cron.Minutely, timeZone: TimeZoneInfo.Local, queue: "critical");
            return Ok("Recurring job tanımlandı.");
        }

        [HttpPost("send/{level}")]
        public IActionResult SendJob(string level)
        {
            var message = $"Görev '{level}' kuyruğuna gönderildi. Zaman: {DateTime.Now}";

            var client = new BackgroundJobClient();

            switch (level.ToLower())
            {
                case "critical":
                    client.Create(() => Console.WriteLine("🚨 [CRITICAL] " + message), new EnqueuedState("critical"));
                    //client.Create<IMyJobService>(x => x.DoWork(), new EnqueuedState("high"));
                    break;
                case "high":
                    client.Create(() => Console.WriteLine("🔥 [HIGH] " + message), new EnqueuedState("high"));
                    break;
                case "normal":
                    client.Create(() => Console.WriteLine("🔄 [NORMAL] " + message), new EnqueuedState("normal"));
                    break;
                case "low":
                    client.Create(() => Console.WriteLine("🐢 [LOW] " + message), new EnqueuedState("low"));
                    break;
                default:
                    return BadRequest("Geçersiz level. Kullan: critical | high | normal | low");
            }

            return Ok(message);
        }
    }
}
