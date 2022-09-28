using System;
using System.ComponentModel;
using System.Text.RegularExpressions;
using Dalamud;

namespace GatherBuddy.FishTimer.Parser;

public partial class FishingParser
{
    private readonly struct Regexes
    {
        public Regex  Cast           { get; private init; }
        public string Undiscovered   { get; private init; }
        public Regex  AreaDiscovered { get; private init; }
        public Regex  Mooch          { get; private init; }

        public static Regexes FromLanguage(ClientLanguage lang)
        {
            return lang switch
            {
                ClientLanguage.ChineseSimplified => ChineseSimplified.Value,
                _                       => throw new InvalidEnumArgumentException(),
            };
        }

        // @formatter:off


        private static readonly Lazy<Regexes> ChineseSimplified = new(() => new Regexes
        {
            Cast = new Regex(@"在(?<FishingSpot>.+)甩出了鱼线开始钓鱼。", RegexOptions.Compiled),
            AreaDiscovered = new Regex(@"将新钓场「(?<FishingSpot>.+)」记录到了钓鱼笔记中！", RegexOptions.Compiled),
            Mooch = new Regex(@"开始利用上钩的.+尝试以小钓大。", RegexOptions.Compiled),
            Undiscovered = "未发现的钓场",
        });
        // @formatter:on
    }
}
