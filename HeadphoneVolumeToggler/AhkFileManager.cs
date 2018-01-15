using System;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.System;

namespace HeadphoneVolumeToggler
{
	public class AhkFileManager
	{
		private readonly int _volume;

		public AhkFileManager(int volume)
		{
			_volume = volume;
		}

		public async Task Run()
		{
			var folder = ApplicationData.Current.LocalFolder;
			var file = await folder.CreateFileAsync("HVT.ahk", CreationCollisionOption.ReplaceExisting);
			await FileIO.WriteTextAsync(file, "SoundSet, " + _volume);
			await Launcher.LaunchFileAsync(file);
			// await file.DeleteAsync();
		}
	}
}