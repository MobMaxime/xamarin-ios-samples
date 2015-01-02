
using System;
using System.Drawing;

using MonoTouch.Foundation;
using MonoTouch.UIKit;
using System.Threading.Tasks;
using Xamarin.Media;
using PhotoCropBind;
using MonoTouch.CoreGraphics;

namespace PhotoCrop
{
	public partial class PhotoCropViewController : UIViewController
	{
		public UIImageView imgProfilePic;
		UIImagePickerController imagePicker;
		PECropViewController peVC;
		readonly MediaPicker mediaPicker = new MediaPicker ();
		readonly TaskScheduler uiScheduler = TaskScheduler.FromCurrentSynchronizationContext ();
		private MediaPickerController mediaPickerController;

		public PhotoCropViewController () : base ("PhotoCropViewController", null)
		{
		}

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();
			this.Title = "Photo Crop";

			UILabel titleLbl = new UILabel ();
			titleLbl.Frame = new RectangleF (0, 90, UIScreen.MainScreen.Bounds.Width, 30);
			titleLbl.Text = "Click below to choose Image.";
			titleLbl.TextAlignment = UITextAlignment.Center;
			View.Add (titleLbl);

			imgProfilePic = new UIImageView ();
			imgProfilePic.Frame = new RectangleF (20, 140, UIScreen.MainScreen.Bounds.Width - 40, UIScreen.MainScreen.Bounds.Height - 160);
			imgProfilePic.Image = UIImage.FromFile ("user_grey.png");
			imgProfilePic.UserInteractionEnabled = true;
			imgProfilePic.ContentMode = UIViewContentMode.ScaleAspectFit;
			View.Add (imgProfilePic);

			UIActionSheet actionSheet = new UIActionSheet ("Choose Image Source");
			actionSheet.AddButton ("Choose From Gallery");
			actionSheet.AddButton ("Take Photo");
			actionSheet.AddButton ("Cancel");
			actionSheet.CancelButtonIndex = 2;

			UIButton choosePhotoButton = new UIButton ();
			choosePhotoButton = UIButton.FromType (UIButtonType.Custom);
			choosePhotoButton.BackgroundColor = UIColor.Clear;
			choosePhotoButton.Frame = new RectangleF (20, 140, UIScreen.MainScreen.Bounds.Width - 40, UIScreen.MainScreen.Bounds.Height - 160);
			View.Add (choosePhotoButton);

			choosePhotoButton.TouchUpInside += (sender, e) => {
				actionSheet = new UIActionSheet ();
				actionSheet.AddButton ("Take Photo");
				actionSheet.AddButton ("Choose Existing Photo");
				actionSheet.AddButton ("Cancel");
				actionSheet.CancelButtonIndex = 2;
				imagePicker = new UIImagePickerController ();

				actionSheet.Clicked += delegate(object a, UIButtonEventArgs b) {
					switch (b.ButtonIndex) {
					case 0:
						if (!mediaPicker.IsCameraAvailable) {
							ShowUnsupported ();
							return;
						}
						mediaPickerController = mediaPicker.GetTakePhotoUI (new StoreCameraMediaOptions {
							Name = string.Format ("{0}.jpg", System.DateTime.Now.Ticks.ToString ()),
							Directory = "PhotoCrop.iOS",
						});
						this.PresentViewController (mediaPickerController, true, null);
						mediaPickerController.GetResultAsync ().ContinueWith (t => {
							this.DismissViewController (true, () => {
								if (t.IsCanceled || t.IsFaulted)
									return;
								MediaFile media = t.Result;
								ShowPhoto (media);
							});
						}, uiScheduler);
						break;
					case 1:
						mediaPickerController = mediaPicker.GetPickPhotoUI ();
						this.PresentViewController (mediaPickerController, true, null);
						mediaPickerController.GetResultAsync ().ContinueWith (t => {
							this.DismissViewController (true, () => {
								if (t.IsCanceled || t.IsFaulted)
									return;
								MediaFile media = t.Result;
								ShowPhoto (media);
							});
						}, uiScheduler);
						break;
					default:
						break;
					}
				};
				actionSheet.ShowInView (View);
				imagePicker.Canceled += Handle_Canceled;
			};
		}

		private void ShowPhoto (MediaFile media)
		{
			UINavigationController navigationVC = new UINavigationController ();
			peVC = new PECropViewController ();
			peVC.Delegate = new PhotoSelectedDelegate (this);
			peVC.Image = UIImage.FromFile (media.Path);
			navigationVC.PushViewController (peVC, true);
			this.PresentViewController (navigationVC, true, null);
		}

		public class PhotoSelectedDelegate : PECropViewControllerDelegate
		{
			private PhotoCropViewController _parent;

			public PhotoSelectedDelegate (PhotoCropViewController parent)
			{
				_parent = parent;
			}

			[Export ("CropViewControllerDidCancel:")] 
			public override void CropViewControllerDidCancel (PECropViewController controller)
			{
				controller.DismissViewController (true, null);
			}

			[Export ("DidFinishCroppingImage:")] 
			public override void DidFinishCroppingImage (PECropViewController controller, UIImage croppedImage)
			{
				controller.DismissViewController (true, null);
			}

			[Export ("DidFinishCroppingImage:")] 
			public override void DidFinishCroppingImage (PECropViewController controller, UIImage croppedImage, CGAffineTransform transform, RectangleF cropRect)
			{
				_parent.imgProfilePic.Image = croppedImage;
				controller.DismissViewController (true, null);
			}
		}

		private void ShowUnsupported ()
		{
			new UIAlertView ("Device unsupported", "Your device does not support this feature", null, "OK", null).Show ();
		}

		void Handle_Canceled (object sender, EventArgs e)
		{
			imagePicker.DismissViewController (true, null);
		}
	}
}