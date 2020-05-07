using System;
using System.Collections.Generic;
using System.Text;

namespace Luna.Clients.Controller
{
    public static class ControllerHelper
    {
        private static IDictionary<string, IController> controllerMap = new Dictionary<string, IController> {
            {"predict", new PredictionController()},
        };

        public static IController GetController(string type)
        {
            if(!controllerMap.ContainsKey(type))
                throw new ArgumentException("The controller type haven't support yet.");
            return controllerMap[type];
        }
    }
}
