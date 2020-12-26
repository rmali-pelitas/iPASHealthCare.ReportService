using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace iPAS.ReportService.API.Models
{
    public class Report
    {
        public Guid Id { get; set; }
        public string ReportType { get; set; }
        public string Name { get; set; }

        public string WebUrl { get; set; }
        public string EmbedUrl { get; set; }
        public bool IsFromPbix { get; set; }
        public bool IsOwnedByMe { get; set; }
        public Guid DataSetId { get; set; }

    }
}
