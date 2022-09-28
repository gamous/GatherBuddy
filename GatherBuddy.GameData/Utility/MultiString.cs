using System;
using Dalamud;
using Dalamud.Data;
using Lumina.Text;

namespace GatherBuddy.Utility;

public readonly struct MultiString
{
    public static string ParseSeStringLumina(SeString? luminaString)
        => luminaString == null ? string.Empty : Dalamud.Game.Text.SeStringHandling.SeString.Parse(luminaString.RawData).TextValue;

    public readonly string ChineseSimplified;
    public readonly string English;
    public readonly string German;
    public readonly string French;
    public readonly string Japanese;

    public string this[ClientLanguage lang]
        => Name(lang);

    public override string ToString()
        => Name(ClientLanguage.ChineseSimplified);

    public string ToWholeString()
        => $"{ChineseSimplified}";

    public MultiString(string zh)
    {
        ChineseSimplified = zh;
        //English  = en;
        //German   = de;
        //French   = fr;
        //Japanese = jp;
    }


    public static MultiString FromPlaceName(DataManager gameData, uint id)
    {
        var zh = ParseSeStringLumina(gameData.GetExcelSheet<Lumina.Excel.GeneratedSheets.PlaceName>(ClientLanguage.ChineseSimplified)!.GetRow(id)?.Name);
        //var en = ParseSeStringLumina(gameData.GetExcelSheet<Lumina.Excel.GeneratedSheets.PlaceName>(ClientLanguage.English)!.GetRow(id)?.Name);
        //var de = ParseSeStringLumina(gameData.GetExcelSheet<Lumina.Excel.GeneratedSheets.PlaceName>(ClientLanguage.German)!.GetRow(id)?.Name);
        //var fr = ParseSeStringLumina(gameData.GetExcelSheet<Lumina.Excel.GeneratedSheets.PlaceName>(ClientLanguage.French)!.GetRow(id)?.Name);
        //var jp = ParseSeStringLumina(gameData.GetExcelSheet<Lumina.Excel.GeneratedSheets.PlaceName>(ClientLanguage.Japanese)!.GetRow(id)?.Name);
        return new MultiString(zh);
    }

    public static MultiString FromItem(DataManager gameData, uint id)
    {
        var zh = ParseSeStringLumina(gameData.GetExcelSheet<Lumina.Excel.GeneratedSheets.Item>(ClientLanguage.ChineseSimplified)!.GetRow(id)?.Name);
        //var en = ParseSeStringLumina(gameData.GetExcelSheet<Lumina.Excel.GeneratedSheets.Item>(ClientLanguage.English)!.GetRow(id)?.Name);
        //var de = ParseSeStringLumina(gameData.GetExcelSheet<Lumina.Excel.GeneratedSheets.Item>(ClientLanguage.German)!.GetRow(id)?.Name);
        //var fr = ParseSeStringLumina(gameData.GetExcelSheet<Lumina.Excel.GeneratedSheets.Item>(ClientLanguage.French)!.GetRow(id)?.Name);
        //var jp = ParseSeStringLumina(gameData.GetExcelSheet<Lumina.Excel.GeneratedSheets.Item>(ClientLanguage.Japanese)!.GetRow(id)?.Name);
        return new MultiString(zh);
    }

    private string Name(ClientLanguage lang)
        => lang switch
        {
            ClientLanguage.ChineseSimplified => ChineseSimplified,
            //ClientLanguage.English  => English,
            //ClientLanguage.German   => German,
            //ClientLanguage.Japanese => Japanese,
            //ClientLanguage.French   => French,
            _                       => throw new ArgumentException(),
        };

    public static readonly MultiString Empty = new( string.Empty);
}
