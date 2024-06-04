using Microsoft.SemanticKernel;
using System.ComponentModel;
using System.Net.Http.Json;

namespace api_ai_rag_intent.Plugins
{
    public class HomeLightsPlugin
    {
        static bool _areLightsOn = false;
        [KernelFunction]
        [Description("Handles requests for turning home lights on and off")]
        public string TurnLightsOnAndOff([Description("the action, whether the lights should be on or off")] bool newStatus)
        {
            _areLightsOn = newStatus;
            return "Lights are now " + (newStatus ? "on" : "off");
        }

        [KernelFunction]
        [Description("Provides current status of whether the home lights are on")]
        public bool AreLightsOn()
        {
            return _areLightsOn; 
        }
    }
}
