using static Tesserae.UI;
using static Tesserae.Tests.Samples.SamplesHelper;
using static Transpose.Core.dom;

namespace Tesserae.Tests.Samples
{
    [SampleDetails(Group = SampleGroup.Media, Order = 60, Icon = UIcons.Film, Description = "The browser's video player, driven from C#")]
    public class VideoPlayerSample : IComponent, ISample
    {
        private readonly IComponent _content;

        public VideoPlayerSample()
        {
            const string video = "assets/media/sample-video.webm";

            var position = TextBlock("0.0 s").Small().Secondary();
            var state    = TextBlock("paused").Small().Secondary();

            var player = VideoPlayer(video, "video/webm")
               .OnTimeUpdate(t => position.Text = t.ToString("0.0") + " s")
               .OnPlay(() => state.Text = "playing")
               .OnPause(() => state.Text = "paused")
               .OnEnded(() => state.Text = "ended")
               .W(480);

            _content = SectionStack().Secondary()
               .SampleTitle(typeof(VideoPlayerSample), UIcons.Film, "A video with the browser's native controls")
               .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                    TextBlock("VideoPlayer wraps the browser's own video element, so every format the browser plays works, the controls are the ones users know, and the picture sizes like any other component. What it adds is a typed handle on playback: read and set the position, play and pause, and listen to what happens."))).SetTitle("Overview")))
               .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                    SampleSubTitle("Playback from code"),
                    TextBlock("The buttons call Play(), Pause() and CurrentTime; the readouts come from OnTimeUpdate, OnPlay, OnPause and OnEnded. A transcript that follows the picture is built from the same two members."),
                    player,
                    HStack().Gap(8.px()).AlignItemsCenter().Children(
                        Button("Play").SetIcon(UIcons.Play).OnClick(() => player.Play()),
                        Button("Pause").SetIcon(UIcons.Pause).OnClick(() => player.Pause()),
                        Button("Jump to 3 s").SetIcon(UIcons.ArrowRight).OnClick(() => player.CurrentTime = 3),
                        Button("Mute").SetIcon(UIcons.VolumeMute).OnClick(() => player.IsMuted = !player.IsMuted),
                        position, state)
                )).SetTitle("Usage")))
               .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                    SampleSubTitle("A muted loop"),
                    TextBlock("AutoPlay() only works for a video without sound, so it is paired with Muted(); PlaysInline() keeps it in the page on a phone, and Controls(false) leaves just the picture."),
                    VideoPlayer(video, "video/webm").AutoPlay().Muted().Loop().PlaysInline().Controls(false).W(240),
                    SampleSubTitle("No download"),
                    TextBlock("NoDownload() takes the download item out of the controls and blocks the context menu. It stops the casual save only: the browser still fetches the file."),
                    VideoPlayer(video, "video/webm").NoDownload().W(240),
                    SampleSubTitle("Fit"),
                    TextBlock("Contain() (the default), Cover() and Fill() decide what happens when the player's box and the picture have different shapes."),
                    HStack().Gap(8.px()).Children(
                        VideoPlayer(video, "video/webm").Contain().W(160).H(160).Controls(false),
                        VideoPlayer(video, "video/webm").Cover().W(160).H(160).Controls(false),
                        VideoPlayer(video, "video/webm").Fill().W(160).H(160).Controls(false))
                )).SetTitle("Options")))
               .SeeAlso(typeof(AudioPlayerSample), typeof(CarouselSample), typeof(AvatarSample));
        }

        public HTMLElement Render() => _content.Render();
    }
}
