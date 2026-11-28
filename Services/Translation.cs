using Application.Data;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace Application.Services;

public class Translation
{
    private readonly AppDbContext _db;

    public Translation(AppDbContext db)
    {
        _db = db;
    }

    public string? GetValue(string code)
    {
        var culture = CultureInfo.CurrentUICulture;

        while (true)
        {
            var value = _db.AppResources
                .AsNoTracking()
                .Where(x => x.Code == code &&
                            x.Culture == culture.Name)
                .Select(x => x.Value)
                .FirstOrDefault();

            if (value != null)
                return value;

            if (culture.Equals(CultureInfo.InvariantCulture))
                return null;

            culture = culture.Parent;
        }
    }

    public Dictionary<string, string> GetValues(
        bool includeParentCultures)
    {
        var culture = CultureInfo.CurrentUICulture;

        var values = new Dictionary<string, string>(
            StringComparer.OrdinalIgnoreCase);

        while (true)
        {
            var translations = _db.AppResources
                .AsNoTracking()
                .Where(x => x.Culture == culture.Name)
                .Select(x => new { x.Code, x.Value })
                .ToList();

            foreach (var translation in translations)
            {
                values.TryAdd(
                    translation.Code,
                    translation.Value);
            }

            if (!includeParentCultures ||
                culture.Equals(CultureInfo.InvariantCulture))
                break;

            culture = culture.Parent;
        }

        return values;
    }
}