using System.Runtime.Serialization;

namespace ssapj.YetAnotherCommander
{
    public class Config
    {
        [DataMember(Name = "scanFolder")]
        public string ScanFolder { get; set; }

        [DataMember(Name = "fileMatchPattern")]
        public string FileMatchPattern { get; set; }

        [DataMember(Name = "btwFilePath")]
        public string BtwFilePath { get; set; }

        [DataMember(Name = "processRefreshInterval")]
        public uint ProcessRefreshInterval { get; set; }
    }
}
