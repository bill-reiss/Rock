using Microsoft.SemanticKernel;
using System.ComponentModel;
using System.Net.Http.Json;

namespace api_ai_rag_intent.Plugins
{
    public class HomeACPlugin
    {
        static int _acTemp = 72;
        [KernelFunction]
        [Description("Changes the air conditioning (AC) temperature for the home")]
        public string SetACTemperature([Description("The new temperature for the air conditioning in degrees")] int newTemperature)
        {
            _acTemp = newTemperature;
            return "Air conditioning now set to " + _acTemp;
        }

        [KernelFunction]
        [Description("Provides current temperature setting of the air conditioning for the home")]
        public int GetCurrentACTemperature()
        {
            return _acTemp; 
        }
    }
}
