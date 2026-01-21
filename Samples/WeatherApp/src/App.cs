using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using H5.Core;
using Tesserae;
using Newtonsoft.Json;
using static H5.Core.dom;
using static Tesserae.UI;

namespace WeatherApp
{
    internal static class App
    {
        private const string HistoryKey = "tss-weather-history";

        private static void Main()
        {
            document.body.style.overflow = "hidden";
            Theme.Light();

            var history = LoadHistory();
            var historyObservable = new ObservableList<string>(history.ToArray());
            historyObservable.Observe(_ => SaveHistory(historyObservable.ToList()));

            var cityObservable = new SettableObservable<string>(history.FirstOrDefault() ?? "");
            var searchBox = SearchBox("Enter city name...").SearchAsYouType();

            searchBox.OnSearch((_, val) => {
                if (!string.IsNullOrWhiteSpace(val))
                {
                    cityObservable.Value = val;
                    if (!historyObservable.Contains(val))
                    {
                        historyObservable.Insert(0, val);
                        if (historyObservable.Count > 5) historyObservable.RemoveAt(5);
                    }
                }
            });

            var resultArea = Defer(cityObservable, city => Task.FromResult(RenderWeather(city)));

            var sidebar = VStack().W(250).Class("weather-sidebar").P(16).Children(
                TextBlock("Recent Searches").SemiBold().Secondary().MB(16),
                Defer(historyObservable, h => Task.FromResult<IComponent>(
                    VStack().Children(h.Select(c =>
                        Button(c).NoBackground().TextLeft().W(1).Grow().OnClick((_, __) => {
                            cityObservable.Value = c;
                            searchBox.Text = c;
                        })
                    ).Cast<IComponent>().ToArray())
                ))
            );

            var mainContent = VStack().S().Children(
                VStack().P(32).Children(
                    searchBox.W(1).Grow()
                ),
                resultArea.W(1).Grow()
            );

            var page = HStack().S().Children(
                sidebar,
                mainContent.W(1).Grow()
            );

            document.body.appendChild(page.Render());
        }

        private static IComponent RenderWeather(string city)
        {
            if (string.IsNullOrWhiteSpace(city))
            {
                return CenteredCard(TextBlock("Explore the weather around the world."));
            }

            var condition = GetMockCondition(city);
            var temp = new Random(city.GetHashCode()).Next(-10, 35);

            return VStack().S().Class("weather-bg-" + condition.text.ToLower().Replace(" ", "-")).Children(
                VStack().S().AlignCenter().JustifyCenter().Children(
                    VStack().AlignCenter().Children(
                        TextBlock(city).Mega().Bold().Class("weather-city"),
                        Icon(condition.icon, size: TextSize.Mega).MT(16).Class("weather-icon-main"),
                        TextBlock($"{temp}°C").Mega().Bold().MT(8).Class("weather-temp"),
                        TextBlock(condition.text).XXLarge().Class("weather-condition")
                    ),
                    HStack().MT(48).Wrap().JustifyCenter().Children(
                        Enumerable.Range(1, 5).Select(i => RenderForecastItem(city, i)).ToArray()
                    )
                )
            );
        }

        private static IComponent RenderForecastItem(string city, int dayOffset)
        {
            var seed = city.GetHashCode() + dayOffset;
            var temp = new Random(seed).Next(-10, 35);
            var condition = GetMockCondition(city + dayOffset);
            var date = DateTime.Today.AddDays(dayOffset);

            return Card(VStack().AlignCenter().Children(
                TextBlock(date.ToString("ddd")).SemiBold(),
                Icon(condition.icon, size: TextSize.Large).MT(8),
                TextBlock($"{temp}°").Bold().MT(8)
            )).Class("forecast-card").W(80).M(8);
        }

        private static (UIcons icon, string text) GetMockCondition(string seed)
        {
            var conditions = new[] {
                (UIcons.Sun, "Sunny"),
                (UIcons.CloudSun, "Partly Cloudy"),
                (UIcons.Cloud, "Cloudy"),
                (UIcons.CloudRain, "Rainy"),
                (UIcons.CloudShowersHeavy, "Stormy"),
                (UIcons.Snowflake, "Snowy")
            };
            return conditions[Math.Abs(seed.GetHashCode()) % conditions.Length];
        }

        private static List<string> LoadHistory()
        {
            var json = localStorage.getItem(HistoryKey);
            if (string.IsNullOrEmpty(json)) return new List<string>();
            try { return JsonConvert.DeserializeObject<List<string>>(json); }
            catch { return new List<string>(); }
        }

        private static void SaveHistory(List<string> history)
        {
            localStorage.setItem(HistoryKey, JsonConvert.SerializeObject(history));
        }

        private static IComponent CenteredCard(IComponent content)
        {
            return VStack().S().AlignCenter().JustifyCenter().Children(Card(content).P(32));
        }
    }
}
