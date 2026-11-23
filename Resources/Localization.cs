using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Markup;

namespace PosAccountingApp.Resources;

public static partial class Localizer
{
    private sealed record Translation(string English, string Pashto, string Dari);

    private static readonly Dictionary<string, Translation> Translations = LoadTranslations();
    private static readonly HashSet<string> ResourceKeys = new(StringComparer.Ordinal);
    public static string Language { get; private set; } = "prs";
    public static bool IsEnglish => Language == "en";

    public static void Apply(string? language)
    {
        Language = language is "en" or "ps" ? language : "prs";
        var resources = Application.Current.Resources;
        resources["InterfaceDirection"] = IsEnglish ? FlowDirection.LeftToRight : FlowDirection.RightToLeft;
        resources["InterfaceLanguage"] = XmlLanguage.GetLanguage(Language switch
        {
            "en" => "en-US", "ps" => "ps-AF", _ => "prs-AF"
        });
        foreach (var key in ResourceKeys.ToArray())
            resources[key] = Translate(key);
    }

    public static string Translate(string source)
    {
        if (Translations.TryGetValue(source, out var exact))
            return GetTranslation(exact);
        var match = Decorations().Match(source);
        if (!match.Success || !Translations.TryGetValue(match.Groups["text"].Value, out var translations))
            return source;
        var suffix = match.Groups["suffix"].Value;
        var prefix = match.Groups["prefix"].Value;
        if (IsEnglish && prefix.Contains('.'))
        {
            prefix = string.Concat(prefix.Select(c => char.IsDigit(c)
                ? (char)('0' + CharUnicodeInfo.GetDecimalDigitValue(c)) : c));
        }
        return prefix + GetTranslation(translations) + suffix;
    }

    private static string GetTranslation(Translation translation) => Language switch
    {
        "en" => translation.English,
        "ps" => translation.Pashto,
        _ => translation.Dari
    };

    public static string Format(string source, params object?[] args) =>
        string.Format(CultureInfo.CurrentCulture, Translate(source), args);

    internal static void Register(string source)
    {
        if (ResourceKeys.Add(source))
            Application.Current.Resources[source] = Translate(source);
    }

    private static Dictionary<string, Translation> LoadTranslations()
    {
        using var stream = typeof(Localizer).Assembly.GetManifestResourceStream("PosAccountingApp.Translations.tsv")
            ?? throw new InvalidOperationException("The application translation catalog is missing.");
        using var reader = new StreamReader(stream);
        var entries = new Dictionary<string, Translation>(StringComparer.Ordinal);
        while (reader.ReadLine() is { } line)
        {
            if (line.Length == 0) continue;
            var columns = line.Split('\t');
            if (columns.Length is not (3 or 4))
                throw new InvalidDataException("Invalid entry in the application translation catalog.");
            var source = Unescape(columns[0]);
            entries.Add(source, new Translation(
                Unescape(columns[1]), Unescape(columns[2]), columns.Length == 4 ? Unescape(columns[3]) : source));
        }
        return entries;
    }

    private static string Unescape(string value) => value.Replace("\\n", "\n");

    [GeneratedRegex(@"\A(?<prefix>\s*(?:[\uE000-\uF8FF]\s*|[۰-۹]+\.\s+)?)(?<text>.*?)(?<suffix>(?:\s*\((?:F[0-9]+|Ctrl\+G)\)|\s*\*)?[:\s]*)\z", RegexOptions.Singleline)]
    private static partial Regex Decorations();
}

[MarkupExtensionReturnType(typeof(string))]
public sealed class LocalizeExtension(string source) : MarkupExtension
{
    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        Localizer.Register(source);
        return new DynamicResourceExtension(source).ProvideValue(serviceProvider);
    }
}

public sealed class LanguagePicker : System.Windows.Controls.ComboBox
{
    public LanguagePicker()
    {
        DisplayMemberPath = nameof(LanguageOption.Name);
        SelectedValuePath = nameof(LanguageOption.Code);
        ItemsSource = new LanguageOption[]
        {
            new("prs", "دری"),
            new("ps", "پښتو"),
            new("en", "English")
        };
        SelectedValue = Localizer.Language;
    }

    public string SelectedLanguage => SelectedValue as string ?? Localizer.Language;

    private sealed record LanguageOption(string Code, string Name);
}
