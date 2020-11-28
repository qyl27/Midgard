using Newtonsoft.Json;

namespace Midgard.ViewModels.Yggdrasil.Index
{
    public class MetaViewModel
    {
        public string ServerName { get; set; }

        public string ServerOwner { get; set; }

        public string ServerOwnerContact { get; set; }

        public string ImplementationName { get; set; }

        public string ImplementationVersion { get; set; }

        public string ImplementationAuthor { get; set; }
        
        [JsonProperty("feature.non_email_login")]
        public bool FeatureNonEmailLogin { get; set; }

        public LinksViewModel Links { get; set; }
    }
}