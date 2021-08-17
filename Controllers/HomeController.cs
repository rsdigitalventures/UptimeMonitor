using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Threading.Tasks;
using UptimeMonitor.Data;
using UptimeMonitor.Models;
using UptimeMonitor.Services;

namespace UptimeMonitor.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public ApplicationDbContext ApplicationDbContext { get; }
        public IDataService DataService { get; }

        public HomeController(ILogger<HomeController> logger,
            ApplicationDbContext applicationDbContext,
            IDataService dataService)
        {
            _logger = logger;
            ApplicationDbContext = applicationDbContext;
            DataService = dataService;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> DeleteWebsite(long id)
        {
            await DataService.DeleteWebsiteById(id);
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Index(Website website)
        {
            ApplicationDbContext.Add(website);
            await ApplicationDbContext.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Install()
        {
            await ApplicationDbContext.Database.EnsureCreatedAsync();
            return RedirectToAction(nameof(Index));
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
