using Microsoft.Extensions.Localization;
using System.Globalization;

namespace Application.Services;

public class AppStringLocalizer : IStringLocalizer
{
    private readonly Translation _translation;

    public AppStringLocalizer(Translation translation)
    {
        _translation = translation;
    }

    public LocalizedString this[string name]
    {
        get
        {
            var value = _translation.GetValue(name);

            return new LocalizedString(
                name,
                value ?? name,
                value == null);
        }
    }

    public LocalizedString this[string name, params object[] arguments]
    {
        get
        {
            var localized = this[name];

            var values = arguments.Select(argument =>
            {
                if (argument is not string code)
                    return argument;

                var translation = this[code];

                return translation.ResourceNotFound
                    ? argument
                    : translation.Value;
            }).ToArray();

            return new LocalizedString(
                name,
                string.Format(
                    CultureInfo.CurrentCulture,
                    localized.Value,
                    values),
                localized.ResourceNotFound);
        }
    }

    public IEnumerable<LocalizedString> GetAllStrings(
        bool includeParentCultures)
    {
        return _translation
            .GetValues(includeParentCultures)
            .Select(x => new LocalizedString(
                x.Key,
                x.Value,
                false));
    }
}
