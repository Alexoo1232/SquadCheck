namespace SquadCheck.Models
{
    /// <summary>
    /// Holds information for the error page.
    /// </summary>
    public class ErrorViewModel
    {
        public string? RequestId { get; set; }

        public bool ShowRequestId
        {
            get
            {
                return !string.IsNullOrEmpty(RequestId);
            }
        }
    }
}
