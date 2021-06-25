using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.InteropServices;

namespace Riveet.Prerender.Entities
{
    public class WebPage
    {
        public int Id { get; set; }

        [StringLength(2000)]
        public string Url { get; set; }

        public string Html { get; set; }

        public DateTime Updated { get; set; }
    }
}
