namespace Midgard.ViewModels.Yggdrasil.Shared
{
    public class ErrorViewModel
    {
        public string Error { get; set; }

        public string ErrorMessage { get; set; }

        public string Cause { get; set; }

        public ErrorViewModel(string error, string errorMsg, string cause = null)
        {
            Error = error;
            ErrorMessage = errorMsg;

            if (cause != null)
            {
                Cause = cause;
            }
        }
    }
}