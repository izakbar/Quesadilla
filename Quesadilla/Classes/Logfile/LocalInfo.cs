namespace Quesadilla.Classes.Logfile
{
    public class LocalInfo
    {
        public LocalInfo()
        {
            this.CharName = string.Empty;
            this.InitialSystem = -1;
            this.CurrentSystem = string.Empty;
        }

        public string CharName { get; set; }

        public int InitialSystem { get; set; }

        public string CurrentSystem { get; set; }
    }
}
