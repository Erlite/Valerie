namespace Rick.CleverbotLib.Models
{
    public class Interaction
    {
        public string Input { get; protected set; }
        public string Output { get; protected set; }

        public Interaction(string input, string output)
        {
            Input = input;
            Output = output;
        }
    }
}
