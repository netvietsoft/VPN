namespace NextAiVPN.Setup
{
    public class InstallProgressReport
    {
        public int Percentage { get; set; }
        public string StatusMessage { get; set; } = string.Empty;
        public string CurrentFile { get; set; } = string.Empty;
    }
}
