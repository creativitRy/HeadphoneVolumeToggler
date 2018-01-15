using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace HeadphoneVolumeToggler
{
	/// <summary>
	/// An empty page that can be used on its own or navigated to within a Frame.
	/// </summary>
	public sealed partial class MainPage : Page
	{
		public MainPage()
		{
			InitializeComponent();
		}

		/*private async void button_Click(object sender, RoutedEventArgs e)
		{
			var mediaElement = new MediaElement();
			var synth = new Windows.Media.SpeechSynthesis.SpeechSynthesizer();
			var stream = await synth.SynthesizeTextToStreamAsync("I am saying something. Wow!!");
			mediaElement.SetSource(stream, stream.ContentType);
			mediaElement.Play();
		}*/

		private void slUnplugged_ValueChanged(object sender,
			RangeBaseValueChangedEventArgs e)
		{
			var application = (App) Application.Current;
			application.UnpluggedVolume = e.NewValue / 100.0;
		}

		private void slPlugged_ValueChanged(object sender,
			RangeBaseValueChangedEventArgs e)
		{
			var application = (App) Application.Current;
			application.PluggedVolume = e.NewValue / 100.0;
		}
	}
}