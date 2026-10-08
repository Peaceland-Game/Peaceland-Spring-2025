using Peaceland;

namespace Peaceland.Notebook
{
    public static class NotebookStatDisplayNames
    {
        public static string GetShortLabel(PeacelandStatId statId)
        {
            switch (statId)
            {
                case PeacelandStatId.InsightNaivety:
                    return "I/N";
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
                case PeacelandStatId.InsightNaivety:
                    return "Insight -> Naivety";
                case PeacelandStatId.KindnessCruelty:
                    return "Kindness -> Cruelty";
                default:
                    return statId.ToString();
            }
        }
    }
}
