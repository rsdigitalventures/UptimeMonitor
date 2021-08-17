using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UptimeMonitor.Data;
using UptimeMonitor.Models;

namespace UptimeMonitor.Services
{
    public interface IDataService
    {
        Task<IEnumerable<Website>> GetAllWebsites();
        Task<IEnumerable<Website>> GetEnabledWebsites();
        Task RecordUptime(string url, bool online);
        Task<Website> GetWebsiteByUrl(string url);
        Task<IEnumerable<UptimeRecord>> GetUptimeRecordsByUrl(string url);
        Task<IEnumerable<UptimeRecord>> GetAllUptimeRecords();
        Task DeleteWebsiteById(long id);
    }

    public class DataService : IDataService
    {
        public DataService(ApplicationDbContext databaseContext)
        {
            DatabaseContext = databaseContext;
        }

        public ApplicationDbContext DatabaseContext { get; }

        public async Task DeleteWebsiteById(long id)
        {
            var website = await DatabaseContext.Websites.FindAsync(id);
            var uptimeRecords = DatabaseContext.UptimeRecords.Where(e => e.WebsiteId == id);
            DatabaseContext.RemoveRange(uptimeRecords);
            await DatabaseContext.SaveChangesAsync();
            DatabaseContext.Remove(website);
            await DatabaseContext.SaveChangesAsync();
        }

        public async Task<IEnumerable<UptimeRecord>> GetAllUptimeRecords()
        {
            return await DatabaseContext.UptimeRecords
              .AsNoTracking()
              .Include(e => e.Website)
              .ToListAsync();
        }

        public async Task<IEnumerable<Website>> GetAllWebsites()
        {
            return await DatabaseContext.Websites.AsNoTracking().ToListAsync();
        }

        public async Task<IEnumerable<Website>> GetEnabledWebsites()
        {
            return await DatabaseContext.Websites.AsNoTracking().Where(e => e.Enabled).ToListAsync();
        }

        public async Task<IEnumerable<UptimeRecord>> GetUptimeRecordsByUrl(string url)
        {
            return await DatabaseContext.UptimeRecords
                .AsNoTracking()
                .Include(e => e.Website)
                .Where(e => e.Website.Url.ToLower() == url.ToLower())
                .ToListAsync();
        }

        public async Task<Website> GetWebsiteByUrl(string url)
        {
            return await DatabaseContext.Websites
               .AsNoTracking()
               .FirstOrDefaultAsync(e => e.Url.ToLower() == url.ToLower());
        }

        public async Task RecordUptime(string url, bool online)
        {
            var website = await GetWebsiteByUrl(url);

            var uptimeRecord = new UptimeRecord
            {
                Date = DateTime.UtcNow,
                Online = online,
                WebsiteId = website.Id
            };

            DatabaseContext.Add(uptimeRecord);
            await DatabaseContext.SaveChangesAsync();
        }
    }
}
