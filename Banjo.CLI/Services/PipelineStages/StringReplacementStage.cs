using System.Linq;
using System.Threading.Tasks;
using Banjo.CLI.Model;
using Newtonsoft.Json.Linq;

namespace Banjo.CLI.Services.PipelineStages
{
    public class StringReplacementStage : IPipelineStage<Auth0ResourceTemplate>
    {
        public string Name { get; } = "Replace Tokens";

        public StringReplacementStage()
        {
        }

        public async Task<Auth0ResourceTemplate> Process(Auth0ResourceTemplate t)
        {
            var overridesVerbatimReplacements = t.Overrides?.VerbatimReplacements;
            if ((overridesVerbatimReplacements?.Count ?? 0) == 0)
            {
                //no overrides or no replacements defined in it, so bail out.
                t.ReplacementsApplied = true;
                return t;
            }

            var tokens = t.Template.SelectTokens("$..*").OfType<JValue>().Where(x => x.Type == JTokenType.String);

            foreach (var token in tokens)
            {
                if (!(token.Value is string v) || string.IsNullOrWhiteSpace(v)) continue;
                
                foreach (var (key, value) in overridesVerbatimReplacements)
                {
                    token.Value = (token.Value as string)?.Replace(key, value);
                }
            }

            t.ReplacementsApplied = true;
            return t;
        }
    }
}