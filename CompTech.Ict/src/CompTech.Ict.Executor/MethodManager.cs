using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using System.IO;


namespace CompTech.Ict.Executor
{
    public class MethodManager
    {
        public static IConfiguration Configuration { get; set; }

        public string PathForMethod(string nameOfMethod)
        {
            var ConfigurationMethods = new ConfigurationBuilder()
                            .SetBasePath(Directory.GetCurrentDirectory())
                            .AddJsonFile("configurationMethods.json");

            Configuration = ConfigurationMethods.Build();

            return Configuration[nameOfMethod].ToString();
        }
    }
}
