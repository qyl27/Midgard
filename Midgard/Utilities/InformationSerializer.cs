﻿using System;
using System.Collections.Generic;
using System.Text;
using Midgard.DbModels;
using Midgard.Enumerates;
using Midgard.SharedModels;
using Newtonsoft.Json;

namespace Midgard.Utilities
{
    public class InformationSerializer
    {
        public static UserInformationModel UserSerializer(User user)
        {
            return new()
            {
                Id = user.Id.ToString("N"),
                Properties = new List<PropertyModel>
                {
                    new()
                    {
                        Name = "preferredLanguage",
                        Value = "zh_CN"
                    }
                }
            };
        }

        public static ProfileInformationModel ProfileSerializer(Profile profile, 
            Skin skin = null, Skin cape = null, bool sign = false)
        {
            var result = new ProfileInformationModel
            {
                Id = profile.Id.ToString("N"),
                Name = profile.Name
            };

            if (skin != null || cape != null)
            {
                var texture = TextureSerializer(profile, skin, cape);
                var json = JsonConvert.SerializeObject(texture);
                var bytes = Encoding.UTF8.GetBytes(json);
                var base64 = Convert.ToBase64String(bytes);

                result.Properties = new List<PropertyModel>
                {
                    new()
                    {
                        Name = "textures",
                        Value = base64,
                        Signature = sign ? Signature.Sign(json) : null
                    }
                };
            }
            
            return result;
        }
        
        public static TextureInformation TextureSerializer(Profile profile, Skin skin = null, Skin cape = null)
        {
            var result = new TextureInformation
            {
                TimeStamp = Time.GetTimeStamp(DateTime.Now),
                ProfileId = profile.Id.ToString("N"),
                ProfileName = profile.Name,
                Textures = new Dictionary<string, SkinInformation>()
            };

            if (skin != null)
            {
                result.Textures.Add("SKIN", new SkinInformation
                {
                    Url = skin.Url, Metadata = new Dictionary<string, string>()
                    {
                        { "model", skin.Model == SkinModel.Default ? "default" : "slim" }
                    }
                });
            }
            
            if (cape != null)
            {
                result.Textures.Add("CAPE", new SkinInformation
                {
                    Url = cape.Url
                });
            }

            return result;
        }
    }
}