using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BarbershopEntities
{
    public class AppSettings
    {
        public ConnectionStrings connectionStrings { get; set; }
        public Jwt Jwt { get; set; }


    }

    public class ConnectionStrings
    {
        public string Barbershop { get; set; }
    }

    public class Jwt
    { 
       public string SecretKey { get; set; }
        public string Issuer { get; set; }
        public string Audience { get; set; }
        public int ExpireMinutes { get; set; }
    }
}

