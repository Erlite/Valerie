namespace Valerie.Models
{
    public partial class DDGModel
    {
        public string Text { get; set; }
        public string FirstURL { get; set; }
    }

    public partial class DDGModel
    {
        public string Image { get; set; }
        public string Heading { get; set; }
        public string Abstract { get; set; }
        public string AbstractURL { get; set; }
        public DDGModel[] RelatedTopics { get; set; }
    }
}