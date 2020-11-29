using Midgard.SharedModels;

namespace Midgard.ViewModels.Yggdrasil.AuthServer
{
    public class RefreshViewModel
    {
        public string AccessToken { get; set; } 

        public string ClientToken { get; set; }

        public ProfileInformationModel SelectedProfile { get; set; }

        public UserInformationModel User { get; set; }
    }
}