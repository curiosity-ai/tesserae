using System.Linq;
using static Tesserae.UI;
using static Tesserae.Tests.Samples.SamplesHelper;
using static Transpose.Core.dom;

namespace Tesserae.Tests.Samples
{
    [SampleDetails(Group = SampleGroup.Media, Order = 70, Icon = UIcons.Waveform, Description = "An audio player whose waveform is the seek bar")]
    public class AudioPlayerSample : IComponent, ISample
    {
        private readonly IComponent _content;

        public AudioPlayerSample()
        {
            const string audio = "assets/media/sample-audio.wav";

            // Twelve half-second notes: what a transcript is, a list of excerpts that each know where they start.
            var notes = new[] { "C", "E", "G", "high C", "G", "E", "D", "G", "B", "high D", "B", "G" };
            var lines = notes.Select((n, i) => TextBlock($"{i * 0.5:0.0} s  ·  {n}").Small()).ToArray();

            var current = -1;
            var player = AudioPlayer(audio, "audio/wav");

            player.OnTimeUpdate(t =>
            {
                var index = System.Math.Min(notes.Length - 1, (int)(t / 0.5));
                if (index == current) return;
                if (current >= 0) lines[current].Regular();
                lines[index].Bold();
                current = index;
            });

            var excerpts = VStack().Gap(2.px()).Children(lines.Select((l, i) =>
                (IComponent)HStack().Gap(8.px()).AlignItemsCenter().Children(
                    Button().SetIcon(UIcons.Play).Compact().NoBorder().Tooltip($"Play from {i * 0.5:0.0} s").OnClick(() => { player.CurrentTime = i * 0.5; player.Play(); }),
                    l)).ToArray());

            _content = SectionStack().Secondary()
               .SampleTitle(typeof(AudioPlayerSample), UIcons.Waveform, "A player with a waveform")
               .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                    TextBlock("AudioPlayer draws the whole recording as a waveform that is also the seek bar: click it to jump. The sound is the browser's own audio element, so every format the browser plays works and Element is there for anything not wrapped. The waveform library (wavesurfer.js) is fetched the first time a player is on the page; if it cannot draw (the script did not load, the audio cannot be decoded from another origin) the player shows the browser's native controls instead of nothing."))).SetTitle("Overview")))
               .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                    SampleSubTitle("A player and the excerpts that follow it"),
                    TextBlock("OnTimeUpdate highlights the excerpt being played and CurrentTime jumps to the one clicked: the shape of a transcript next to a recording."),
                    HStack().WS().Wrap().Gap(24.px()).AlignItems(ItemAlign.Start).Children(player.W(420), excerpts)
                )).SetTitle("Usage")))
               .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                    SampleSubTitle("Colours and height"),
                    TextBlock("The waveform follows the theme (the played part in the primary colour). WaveformColors takes any CSS colour or theme token, and WaveformHeight the height in pixels."),
                    AudioPlayer(audio, "audio/wav").WaveformHeight(48).WaveformColors("#c7b9f2", "#7b3ff2").W(420),
                    SampleSubTitle("Native controls"),
                    TextBlock("NoWaveform() shows the browser's own audio bar and fetches nothing extra."),
                    AudioPlayer(audio, "audio/wav").NoWaveform().W(420),
                    SampleSubTitle("No download"),
                    AudioPlayer(audio, "audio/wav").NoDownload().Loop().W(420)
                )).SetTitle("Options")))
               .SeeAlso(typeof(VideoPlayerSample), typeof(AvatarSample));
        }

        public HTMLElement Render() => _content.Render();
    }
}
