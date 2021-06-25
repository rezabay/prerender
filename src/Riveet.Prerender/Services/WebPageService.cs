using Microsoft.EntityFrameworkCore;
using Riveet.Prerender.Contexts;
using Riveet.Prerender.Entities;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Riveet.Prerender.Services
{
    public class WebPageService
    {
        private readonly WebsiteContext _dbContext;

        public WebPageService(WebsiteContext dbContext)
        {
            _dbContext = dbContext;
        }

        public Task<string> Get(string url)
        {
            return _dbContext.Website
                             .AsNoTracking()
                             .Where(x => x.Url == url)
                             .Select(x => x.Html)
                             .FirstOrDefaultAsync();
        }

        public async Task<bool> IsPageUpdated(string url, TimeSpan cacheTimeout)
        {
            var page = await _dbContext.Website
                                       .AsNoTracking()
                                       .Where(x => x.Url == url)
                                       .Select(x => new { x.Url, x.Updated })
                                       .SingleOrDefaultAsync();

            return page != null && DateTime.UtcNow - page.Updated < cacheTimeout;
        }

        public async Task Set(string url, string html)
        {
            var anyPage = await _dbContext.Website
                                          .AsNoTracking()
                                          .AnyAsync(x => x.Url == url);
            if (!anyPage)
            {
                var page = new WebPage
                {
                    Url = url,
                    Html = html,
                    Updated = DateTime.UtcNow
                };
                await _dbContext.AddAsync(page);
                await _dbContext.SaveChangesAsync();
            } 
            else
            {
                await _dbContext.Database.ExecuteSqlInterpolatedAsync(
                    $@"UPDATE ""Website"" SET ""Html"" = {html}, ""Updated""= {DateTime.UtcNow} WHERE ""Url"" = {url}"
                );
            }
        }
    }
}
