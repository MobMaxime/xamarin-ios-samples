using System;
using System.Collections.Generic;
using System.Linq;

using MonoTouch.Foundation;
using MonoTouch.UIKit;

namespace PhotoCrop
{

	[Register ("AppDelegate")]
	public partial class AppDelegate : UIApplicationDelegate
	{
		// class-level declarations
		UIWindow window;
		UINavigationController navigationController;

		public override bool FinishedLaunching (UIApplication app, NSDictionary options)
		{
			window = new UIWindow (UIScreen.MainScreen.Bounds);
			navigationController = new UINavigationController ();
			navigationController.PushViewController (new PhotoCropViewController (), true);
			window.RootViewController = navigationController;
			window.MakeKeyAndVisible ();
			
			return true;
		}
	}
}

