namespace dmr_api.Models
{
    public class GlueType
    {
        public int ID { get; set; }
        public string Code { get; set; }
        public string Title { get; set; }
        public int Level { get; set; }
        public double Minutes { get; set; }
        public double RPM { get; set; }
        public string Method { get; set; }

        public int? ParentID { get; set; }
    }
}