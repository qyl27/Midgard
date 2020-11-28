using System.Collections.Generic;

namespace Midgard.ViewModels.Api.Shared
{
    public class TranslatableMessage
    {
        public string Message { get; set; }
        
        public List<string> Arguments { get; set; }
    }
}