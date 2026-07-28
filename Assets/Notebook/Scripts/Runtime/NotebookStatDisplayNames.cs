using Peaceland;

namespace Peaceland.Notebook
{
    public static class NotebookStatDisplayNames
    {
        public static string GetShortLabel(PeacelandStatId statId)
        {
            switch (statId)
            {
                case PeacelandStatId.SelfishAltruistic:
                    return "S/A";
                case PeacelandStatId.InsightNaivety:
                    return "I/N";
                case PeacelandStatId.NationalismRebellion:
                    return "N/RA";
                case PeacelandStatId.KindnessCruelty:
                    return "K/C";
                default:
                    return statId.ToString();
            }
        }

        public static string GetTitle(PeacelandStatId statId)
        {
            switch (statId)
            {
                case PeacelandStatId.SelfishAltruistic:
                    return "Selfish -> Altruistic";
                case PeacelandStatId.InsightNaivety:
                    return "Insight -> Naivety";
                case PeacelandStatId.NationalismRebellion:
                    return "Nationalism -> Rebellion";
                case PeacelandStatId.KindnessCruelty:
                    return "Kindness -> Cruelty";
                default:
                    return statId.ToString();
            }
        }
    }
}
