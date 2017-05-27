namespace Rick.Models
{
    public class TagsModel
    {
        public string TagName { get; set; }
        public string TagResponse { get; set; }
        public string CreationDate { get; set; }
        public ulong OwnerId { get; set; }
        public int TagUses { get; set; }
    }
}
