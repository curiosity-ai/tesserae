using System;
using System.Linq;
using static H5.Core.dom;
using static Tesserae.Tests.Samples.SamplesHelper;
using static Tesserae.UI;

namespace Tesserae.Tests.Samples
{
    [SampleDetails(Group = "Components", Order = 10, Icon = UIcons.TableLayout)]
    public class GridPickerSample : IComponent, ISample
    {
        private readonly IComponent _content;
        public GridPickerSample()
        {
            var picker = GridPicker(
                columnNames: new[] { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday" },
                rowNames: new[] { "Morning", "Afternoon", "Night" },
                states: 3,
                initialStates: new[]
                {
                    new[] { 0, 0, 0, 0, 0, 0, 0 },
                    new[] { 0, 0, 0, 0, 0, 0, 0 },
                    new[] { 0, 0, 0, 0, 0, 0, 0 }
                },
                formatState: (btn, state, previousState) =>
                {
                    string text = "";
                    switch (state)
                    {
                        case 0: text = "☠"; break;
                        case 1: text = "🐢"; break;
                        case 2: text = "🐇"; break;
                    }

                    if (previousState >= 0 && previousState != state)
                    {
                        switch (previousState)
                        {
                            case 0: text = $"☠ -> {text}"; break;
                            case 1: text = $"🐢 -> {text}"; break;
                            case 2: text = $"🐇 -> {text}"; break;
                        }
                    }
                    btn.SetText(text);
                });

            var hourPicker = GridPicker(
                rowNames: new[] { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday" },
                columnNames: Enumerable.Range(0, 24).Select(n => $"{n:00}").ToArray(),
                states: 4,
                initialStates: Enumerable.Range(0, 7).Select(_ => new int[24]).ToArray(),
                formatState: (btn, state, previousState) =>
                {
                    string color = "";
                    switch (state)
                    {
                        case 0: color = "#c7c5c5"; break;
                        case 1: color = "#a3cfa5"; break;
                        case 2: color = "#76cc79"; break;
                        case 3: color = "#1fcc24"; break;
                    }
                    btn.Background(color);
                },
                columns: new[] { 128.px(), 24.px() },
                rowHeight: 24.px());

            _content = SectionStack()
               .Title(SampleHeader(nameof(GridPickerSample)))
               .Section(Stack().Children(
                    SampleTitle("Overview"),
                    TextBlock("The GridPicker component provides an interactive grid where each cell can cycle through a predefined number of states. It's highly customizable through its state formatting logic."),
                    TextBlock("Common use cases include scheduling, availability maps, or any scenario where you need to visualize and edit state across two dimensions.")))
               .Section(Stack().Children(
                    SampleTitle("Best Practices"),
                    TextBlock("Use GridPickers for dense state selection where labels for each cell would be too cluttered. Provide a clear legend or visual cues for what each state represents. Ensure the row and column headers are descriptive. If the grid is large, consider how it will behave on smaller screens. Leverage the state formatting to provide rich feedback, such as changing colors, icons, or text based on the current state.")))
               .Section(Stack().Children(
                    SampleTitle("Usage"),
                    SampleSubTitle("Simple Schedule Example"),
                    TextBlock("Click on cells to cycle through states: Dead (☠), Slow (🐢), and Fast (🐇)."),
                    picker,
                    SampleSubTitle("Heatmap/Availability Example"),
                    TextBlock("Assigning different background colors based on state levels (0 to 3)."),
                    hourPicker,
                    SampleSubTitle("Dynamic/Calculated Grid"),
                    TextBlock("Using GridPicker for a complex logic visualization (Game of Life)."),
                    GetGameOfLifeSample()));
        }

        private static Stack GetGameOfLifeSample()
        {
            int  height   = 32;
            int  width    = 32;
            bool isPaused = false;

            var grid = GridPicker(
                rowNames: Enumerable.Range(0,    height).Select(n => $"{n:00}").ToArray(),
                columnNames: Enumerable.Range(0, width).Select(n => $"{n:00}").ToArray(),
                states: 2,
                initialStates: Enumerable.Range(0, width).Select(_ => new int[height]).ToArray(),
                formatState: (btn, state, previousState) =>
                {
                    string color = state == 0 ? Theme.Default.Background : Theme.Default.Foreground;
                    btn.Background(color);
                },
                columns: new[] { 128.px(), 24.px() },
                rowHeight: 24.px());

            grid.WhenMounted(() =>
            {
                var t = window.setInterval((_) => { if (grid.IsMounted()) Grow(); }, 200);
                grid.WhenRemoved(() => window.clearInterval(t));
            });

            var btnReset = Button("Reset").SetIcon(UIcons.Bomb).OnClick(() =>
            {
                var state = grid.GetState();
                foreach (var a in state) for (int i = 0; i < a.Length; i++) a[i] = 0;
                grid.SetState(state);
            });

            var btnPause = Button("Pause").SetIcon(UIcons.Pause);
            btnPause.OnClick(() =>
            {
                isPaused = !isPaused;
                btnPause.SetIcon(isPaused ? UIcons.Play : UIcons.Pause);
                btnPause.SetText(isPaused ? "Resume" : "Pause");
            });

            void Grow()
            {
                if (grid.IsDragging || isPaused) return;
                var previous = grid.GetState();
                var cells    = grid.GetState();
                for (int i = 0; i < height; i++)
                {
                    for (int j = 0; j < width; j++)
                    {
                        int alive = GetNeighbors(previous, i, j);
                        if (cells[i][j] == 1) cells[i][j] = (alive < 2 || alive > 3) ? 0 : 1;
                        else if (alive == 3) cells[i][j] = 1;
                    }
                }
                grid.SetState(cells);
            }

            int GetNeighbors(int[][] cells, int x, int y)
            {
                int count = 0;
                for (int i = x - 1; i <= x + 1; i++)
                {
                    for (int j = y - 1; j <= y + 1; j++)
                    {
                        if (i < 0 || j < 0 || i >= height || j >= width || (i == x && j == y)) continue;
                        if (cells[i][j] == 1) count++;
                    }
                }
                return count;
            }

            return VStack().WS().Children(HStack().WS().Children(btnPause, btnReset), grid.WS());
        }

        public HTMLElement Render() => _content.Render();
    }
}
