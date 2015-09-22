using System;
using Nancy;
using Nancy.Security;

namespace IdSrvDemo
{
    public class HomeModule : NancyModule
    {
        public HomeModule()
        {
            this.RequiresAuthentication();
            Get["/"] = _ => "Hi";


        }
    }
}

