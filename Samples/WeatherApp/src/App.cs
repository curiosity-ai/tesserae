using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using H5.Core;
using Tesserae;
using static H5.Core.dom;
using static Tesserae.UI;

namespace WeatherApp
{
    internal static class App
    {
        private static void Main()
        {
            document.body.style.overflow = "hidden";
            Theme.Light();

            var cityObservable = new SettableObservable<string>("");
            var searchBox = SearchBox("Enter city name...").SearchAsYouType();

            searchBox.OnSearch((_, val) => cityObservable.Value = val);

            var resultArea = Defer(cityObservable, city => Task.FromResult(RenderWeather(city)));

            var page = VStack().S().Padding(32.px()).Children(
                TextBlock("Weather Forecast").XLarge().SemiBold(),
                HStack().MarginTop(16.px()).Children(searchBox.W(1).Grow()),
                resultArea.W(1).Grow().MarginTop(32.px())
            );

            document.body.appendChild(page.Render());
        }

        private static IComponent RenderWeather(string city)
        {
            if (string.IsNullOrWhiteSpace(city))
            {
                return CenteredCard(TextBlock("Please enter a city name to see the forecast."));
            }

            // Mock weather data
            var temp = new Random(city.GetHashCode()).Next(-10, 35);
            var condition = GetMockCondition(city);

            return VStack().Children(
                Card(VStack().AlignItemsCenter().Children(
                    TextBlock(city).XXLarge().SemiBold(),
                    Icon(condition.icon, size: TextSize.XXLarge).MarginTop(16.px()),
                    TextBlock($"{temp}°C").XXLarge().MarginTop(8.px()),
                    TextBlock(condition.text).Secondary().Large()
                )).Padding(32.px()),
                TextBlock("5-Day Forecast").Large().SemiBold().MarginTop(32.px()),
                HStack().MarginTop(16.px()).Wrap().Children(
                    Enumerable.Range(1, 5).Select(i => RenderForecastItem(city, i)).ToArray()
                )
            );
        }

        private static IComponent RenderForecastItem(string city, int dayOffset)
        {
            var seed = city.GetHashCode() + dayOffset;
            var temp = new Random(seed).Next(-10, 35);
            var condition = GetMockCondition(city + dayOffset);
            var date = DateTime.Today.AddDays(dayOffset);

            return Card(VStack().AlignItemsCenter().Children(
                TextBlock(date.ToString("ddd, MMM d")).SemiBold(),
                Icon(condition.icon, size: TextSize.Large).MarginTop(8.px()),
                TextBlock($"{temp}°C").SemiBold().MarginTop(8.px())
            )).Width(120.px()).MarginRight(16.px()).MarginBottom(16.px());
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

        private static IComponent CenteredCard(IComponent content)
        {
            return HStack().JustifyContent(ItemJustify.Center).MarginTop(100.px()).Children(Card(content).Padding(32.px()));
        }
    }
}
