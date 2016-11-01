using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.Owin;
using Owin;
using System.Net.Http;

namespace ANRDraft
{
    public class Startup
    {

        public void Configuration(IAppBuilder app)
        {
            app.MapSignalR();
            //set up the card database
            var setup = NRDBClient.Instance;
        }
    }
}