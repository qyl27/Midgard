namespace Midgard.ViewModels.Api.Shared
{
    public class BoolViewModel
    {
        public bool Result { get; set; }

        public BoolViewModel(bool boolIn)
        {
            Result = boolIn;
        }
    }
}