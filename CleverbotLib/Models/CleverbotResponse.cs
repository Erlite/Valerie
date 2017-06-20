using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace Rick.CleverbotLib.Models
{
    public class CleverbotResponse
    {
        public string CleverbotState { get; protected set; }
        public string InteractionCount { get; protected set; }
        public string Input { get; protected set; }
        public string InputOther { get; protected set; }
        public string InputLabel { get; protected set; }
        public string PredictedInput { get; protected set; }
        public string Accuracy { get; protected set; }
        public string OutputLabel { get; protected set; }
        public string Output { get; protected set; }
        public string ConversationId { get; protected set; }
        public string ErrorLine { get; protected set; }
        public string DatabaseVersion { get; protected set; }
        public string SoftwareVersion { get; protected set; }
        public string TimeTaken { get; protected set; }
        public string RandomNumber { get; protected set; }
        public string TimeSecond { get; protected set; }
        public string TimeMinute { get; protected set; }
        public string TimeHour { get; protected set; }
        public string TimeDayOfWeek { get; protected set; }
        public string TimeDay { get; protected set; }
        public string TimeMonth { get; protected set; }
        public string TimeYear { get; protected set; }
        public string Reaction { get; protected set; }
        public string ReactionTone { get; protected set; }
        public string Emotion { get; protected set; }
        public string EmotionTone { get; protected set; }
        public string CleverAccuracy { get; protected set; }
        public string CleverOutput { get; protected set; }
        public string CleverMatch { get; protected set; }
        public string CSRES30 { get; protected set; }
        public string TimeElapsed { get; protected set; }
        public string FilteredInput { get; protected set; }
        public string FilteredInputOther { get; protected set; }
        public string ReactionDegree { get; protected set; }
        public string EmotionDegree { get; protected set; }
        public string ReactionValues { get; protected set; }
        public string EmotionValues { get; protected set; }
        public string Callback { get; protected set; }
        public List<Interaction> Interactions { get; protected set; } = new List<Interaction>();

        public CleverbotResponse(JToken json)
        {
            CleverbotState = json.SelectToken("cs")?.ToString();
            InteractionCount = json.SelectToken("interaction_count")?.ToString();
            Input = json.SelectToken("input")?.ToString();
            InputOther = json.SelectToken("input_other")?.ToString();
            InputLabel = json.SelectToken("input_label")?.ToString();
            PredictedInput = json.SelectToken("predicted_input")?.ToString();
            Accuracy = json.SelectToken("accuracy")?.ToString();
            OutputLabel = json.SelectToken("output_label")?.ToString();
            Output = json.SelectToken("output")?.ToString();
            ConversationId = json.SelectToken("conversation_id")?.ToString();
            ErrorLine = json.SelectToken("errorline")?.ToString();
            DatabaseVersion = json.SelectToken("database_version")?.ToString();
            SoftwareVersion = json.SelectToken("software_version")?.ToString();
            TimeTaken = json.SelectToken("time_taken")?.ToString();
            RandomNumber = json.SelectToken("random_number")?.ToString();
            TimeSecond = json.SelectToken("time_second")?.ToString();
            TimeMinute = json.SelectToken("time_minute")?.ToString();
            TimeHour = json.SelectToken("time_hour")?.ToString();
            TimeDayOfWeek = json.SelectToken("time_day_of_week")?.ToString();
            TimeDay = json.SelectToken("time_day").ToString();
            TimeMonth = json.SelectToken("time_month")?.ToString();
            TimeYear = json.SelectToken("time_year")?.ToString();
            Reaction = json.SelectToken("reaction")?.ToString();
            ReactionTone = json.SelectToken("reaction_tone")?.ToString();
            Emotion = json.SelectToken("emotion")?.ToString();
            EmotionTone = json.SelectToken("emotion_tone")?.ToString();
            CleverAccuracy = json.SelectToken("clever_accuracy")?.ToString();
            CleverOutput = json.SelectToken("clever_output")?.ToString();
            CleverMatch = json.SelectToken("clever_match")?.ToString();
            CSRES30 = json.SelectToken("CSRES30")?.ToString();
            TimeElapsed = json.SelectToken("time_elapsed")?.ToString();
            FilteredInput = json.SelectToken("filtered_input")?.ToString();
            FilteredInputOther = json.SelectToken("filtered_input_other")?.ToString();
            ReactionDegree = json.SelectToken("reaction_degree")?.ToString();
            EmotionDegree = json.SelectToken("emotion_degree")?.ToString();
            ReactionValues = json.SelectToken("reaction_values")?.ToString();
            EmotionValues = json.SelectToken("emotion_values")?.ToString();
            Callback = json.SelectToken("callback")?.ToString();

            int cur = 1;
            while (json.SelectToken($"interaction_{cur}") != null && json.SelectToken($"interaction_{cur}").ToString() != "")
            {
                Interactions.Add(new Interaction(json.SelectToken($"interaction_{cur}").ToString(), json.SelectToken($"interaction_{cur}_other").ToString()));
                cur++;
            }

        }
    }
}