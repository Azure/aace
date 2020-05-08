using System;
using System.Collections.Generic;
using System.Text;

namespace Luna.Clients.Controller
{
    public interface IController
    {
        public string GetName();
        public string GetUrlTemplate();
        public string GetMethod();
        public string GetPath(string productName, string deploymentName);
        public string GetBaseUrl();
    }
}
