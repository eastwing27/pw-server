using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace pwServer
{
    public static class Extenders
    {
        public static int GetUserId (this ISession Session)
        {
            int id;

            if (int.TryParse(Session.GetString("id"), out id))
                return id;

            return -1;
        }
    }
}
