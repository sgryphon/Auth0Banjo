using System.IO;
using Newtonsoft.Json.Linq;

namespace Banjo.CLI.Model
{
    public class Auth0ResourceTemplate
    {
        public ResourceType Type { get; set; }
        public FileInfo Location { get; set; }
        public string Filename => Location?.Name;
        public JToken Template { get; set; }
        public Overrides Overrides { get; set; }
        public bool OverridesApplied { get; set; } = false;
        public bool ReplacementsApplied { get; set; } = false;
        public bool Preprocessed { get; set; } = false;
        public bool ApiCallsProcessed { get; set; } = false;
        public bool ContinueProcessing { get; set; } = true;
    }
}