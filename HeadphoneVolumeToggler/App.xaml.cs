using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Background;
using Windows.UI.Notifications;
using Windows.UI.Notifications.Management;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace HeadphoneVolumeToggler
{
	/// <summary>
	/// Provides application-specific behavior to supplement the default Application class.
	/// </summary>
	sealed partial class App : Application
	{
		public int UnpluggedVolume { get; set; }
		public int PluggedVolume { get; set; }

		/// <summary>
		/// Initializes the singleton application object.  This is the first line of authored code
		/// executed, and as such is the logical equivalent of main() or WinMain().
		/// </summary>
		public App()
		{
			InitializeComponent();
			Suspending += OnSuspending;
		}

		/// <summary>
		/// Invoked when the application is launched normally by the end user.  Other entry points
		/// will be used such as when the application is launched to open a specific file.
		/// </summary>
		/// <param name="e">Details about the launch request and process.</param>
		protected override void OnLaunched(LaunchActivatedEventArgs e)
		{
#if DEBUG
			if (Debugger.IsAttached)
			{
				DebugSettings.EnableFrameRateCounter = true;
			}
#endif
			var rootFrame = Window.Current.Content as Frame;

			// Do not repeat app initialization when the Window already has content,
			// just ensure that the window is active
			if (rootFrame == null)
			{
				// Create a Frame to act as the navigation context and navigate to the first page
				rootFrame = new Frame();

				rootFrame.NavigationFailed += OnNavigationFailed;

				if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
				{
					//TODO: Load state from previously suspended application
				}

				// Place the frame in the current Window
				Window.Current.Content = rootFrame;
			}

			if (e.PrelaunchActivated == false)
			{
				if (rootFrame.Content == null)
				{
					// When the navigation stack isn't restored navigate to the first page,
					// configuring the new page by passing required information as a navigation
					// parameter
					rootFrame.Navigate(typeof(MainPage), e.Arguments);
				}

				// Ensure the current window is active
				Window.Current.Activate();

				InitListener();
			}
		}

		private async void InitListener()
		{
			var listener = UserNotificationListener.Current;

			var notificationStatus = await listener.RequestAccessAsync();
			switch (notificationStatus)
			{
				case UserNotificationListenerAccessStatus.Allowed:

					// Yay! Proceed as normal
					break;

				case UserNotificationListenerAccessStatus.Denied:

					// todo: Show UI explaining that listener features will not work until user allows access.
					break;

				case UserNotificationListenerAccessStatus.Unspecified:

					// todo: Show UI that allows the user to bring up the prompt again
					break;

				default:
					throw new ArgumentOutOfRangeException();
			}

			var backgroundStatus = await BackgroundExecutionManager.RequestAccessAsync();
			switch (backgroundStatus)
			{
				case BackgroundAccessStatus.AlwaysAllowed:
				case BackgroundAccessStatus.AllowedSubjectToSystemPolicy:
				case BackgroundAccessStatus.AllowedWithAlwaysOnRealTimeConnectivity:
				case BackgroundAccessStatus.AllowedMayUseActiveRealTimeConnectivity:

					// Yay! Proceed as normal
					break;

				case BackgroundAccessStatus.Denied:
				case BackgroundAccessStatus.DeniedBySystemPolicy:
				case BackgroundAccessStatus.DeniedByUser:
					// todo: Show UI explaining that listener features will not work until user allows access.
					break;

				case BackgroundAccessStatus.Unspecified:

					// todo: Show UI that allows the user to bring up the prompt again
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}

			const string backgroundTaskName = "UserNotificationChanged";

			// If background task isn't registered yet
			if (!BackgroundTaskRegistration.AllTasks.Any(i => i.Value.Name.Equals(backgroundTaskName)))
			{
				// Specify the background task
				var builder = new BackgroundTaskBuilder
				{
					Name = backgroundTaskName
				};

				// Set the trigger for Listener, listening to Toast Notifications
				builder.SetTrigger(new UserNotificationChangedTrigger(NotificationKinds.Toast));

				// Register the task
				builder.Register();
			}
		}

		protected override async void OnBackgroundActivated(BackgroundActivatedEventArgs args)
		{
			var deferral = args.TaskInstance.GetDeferral();

			if (args.TaskInstance.Task.Name == "UserNotificationChanged")
			{
				await OnNotificationChange();
			}

			deferral.Complete();
		}

		private async Task OnNotificationChange()
		{
			var listener = UserNotificationListener.Current;

			// Get all the current notifications from the platform
			var notifications = await listener.GetNotificationsAsync(NotificationKinds.Toast);

			var notification = notifications.FindMaxValue(notif => notif.CreationTime);
			if (notification == null)
				return;

			if (notification.AppInfo.DisplayInfo.DisplayName == "Realtek HD Audio Manager")
			{
				// this loop should run only once
				foreach (var notificationBinding in notification.Notification.Visual.Bindings)
				{
					var texts = notificationBinding.GetTextElements();
					if (texts == null || texts.Count < 2)
						return;

					var text = texts[1].Text;
					if (text.Contains("unplugged"))
					{
						await SwitchVolume(false);
					}
					else if (text.Contains("plugged"))
					{
						await SwitchVolume(true);
					}
				}
			}
		}

		public async Task SwitchVolume(bool plugged)
		{
			await new AhkFileManager(plugged ? PluggedVolume : UnpluggedVolume).Run();
		}

		/// <summary>
		/// Invoked when Navigation to a certain page fails
		/// </summary>
		/// <param name="sender">The Frame which failed navigation</param>
		/// <param name="e">Details about the navigation failure</param>
		private void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
		{
			throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
		}

		/// <summary>
		/// Invoked when application execution is being suspended.  Application state is saved
		/// without knowing whether the application will be terminated or resumed with the contents
		/// of memory still intact.
		/// </summary>
		/// <param name="sender">The source of the suspend request.</param>
		/// <param name="e">Details about the suspend request.</param>
		private void OnSuspending(object sender, SuspendingEventArgs e)
		{
			var deferral = e.SuspendingOperation.GetDeferral();
			//TODO: Save application state and stop any background activity
			deferral.Complete();
		}
	}
}