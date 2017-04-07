namespace Rick.Classes
{
    public class Response
    {
        public string Trigger;
        public string Respond;
        public ulong Creator;

        public Response(string trigger, string response, ulong ID)
        {
            this.Trigger = trigger;
            this.Respond = response;
            this.Creator = ID;
        }
    }
}
