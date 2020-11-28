using System;
using System.Collections.Generic;
using Midgard.SharedModels;

namespace Midgard.ViewModels.Yggdrasil.AuthServer
{
    public class AuthenticateViewModel
    {
        public Guid AccessToken { get; set; } 

        public string ClientToken { get; set; }

        public List<ProfileInformationModel> AvailableProfiles { get; set; }

        public ProfileInformationModel SelectedProfile { get; set; }

        public UserInformationModel User { get; set; }
    }
}