using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMS.Models
{
    public class MongoDbSettings
    {
        public const string SectionName = "MongoDbSettings";
        
        public required string ConnectionString { get; set; }
        public required string DatabaseName { get; set; }
    }
}
