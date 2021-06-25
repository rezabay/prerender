using Riveet.AspNetCore;
using System.Threading.Tasks;

namespace Riveet.Prerender
{
    public class Program
    {
        public static Task Main(string[] args)
        {
            var program = new RiveetProgram<Startup>(args);
            return program.RunAsync();
        }
    }
}
