using System;
using CollectionViewBinding;
using MonoTouch.UIKit;
using System.Drawing;
using MobilePatterns.Data;
using MonoTouch.Foundation;
using System.Threading;
using SDWebImage;

namespace MobilePatterns.Cells
{
    public class PatternCell : PSCollectionViewCell
    {
        UIImageView _imageView;
        UIActivityIndicatorView _activity;
        Uri _requestUri;

        public PatternCell()
        {
            _imageView = new UIImageView(RectangleF.Empty);
            _imageView.ContentMode = UIViewContentMode.ScaleToFill;
            this.AddSubview(_imageView);
            
            this.Layer.ShadowColor = UIColor.FromRGB(41, 41, 41).CGColor;
            this.Layer.ShadowOffset = new SizeF(0, 0);
            this.Layer.ShadowOpacity = 0.6f;
            this.Layer.ShadowRadius = 4f;

            BackgroundColor = UIColor.White;
        }
        
        public override void PrepareForReuse ()
        {
            base.PrepareForReuse ();
            SDWebImageManager.SharedManager.CancelForDelegate(this);
            _requestUri = null;
            if (_imageView.Image != null)
            {
                //_imageView.Image.Dispose();
                _imageView.Image = null;
            }
        }
        
        public override void LayoutSubviews ()
        {
            base.LayoutSubviews ();
            this.Layer.ShadowPath = UIBezierPath.FromRect(this.Bounds).CGPath;
            
            _imageView.Frame = new RectangleF(0, 0, Bounds.Width, Bounds.Height);
        }
        
        public override float HeightForViewWithObject (NSObject obj, float columnWidth)
        {
            var width = Bounds.Width;
            var scale = 960f / (640f / width);
            return scale;
        }
        
        public void FillViewWithObject(string sc)
        {
            AddSpinner();
            SizeF size;
            if (MobilePatterns.Utils.Util.IsRetina)
                size = new SizeF(296, 444);
            else
                size = new SizeF(148, 222);

            var url = "http://www.dillonbuchanan.com/appreciateui/downloader.php?id=" + sc + "&w=" + size.Width + "&h=" + size.Height;
            SDWebImageManager.SharedManager.CancelForDelegate(this);
            SDWebImageManager.SharedManager.Download(new NSUrl(url), this, SDWebImageOptions.SDWebImageCacheMemoryOnly, (i, c) => {
                FillWithLocal(i);
            }, (e) => { 
                Console.WriteLine("Crap.");
            });
        }

        private void AddSpinner()
        {
            RemoveSpinner();

            _activity = new UIActivityIndicatorView(UIActivityIndicatorViewStyle.Gray);
            _activity.Frame = new RectangleF(0, 0, 48, 48);
            _activity.Center = this.Center;
            _activity.AutoresizingMask = UIViewAutoresizing.All;
            this.AddSubview(_activity);
            _activity.StartAnimating();
        }

        public void FillWithLocal(UIImage img)
        {
            RemoveSpinner();
            _imageView.Image = img;
            _imageView.Alpha = 0;
            UIView.BeginAnimations("imageFade");
            UIView.SetAnimationCurve(UIViewAnimationCurve.EaseInOut);
            UIView.SetAnimationDuration(0.3);
            _imageView.Alpha = 1;
            UIView.CommitAnimations();
        }


        UIImage currentLoad;
        public void FillWithLocalNotResized(UIImage img)
        {
            AddSpinner();

            currentLoad = img;
            var size = new SizeF(148, 222);

            ThreadPool.QueueUserWorkItem(delegate {
                var ciimage = MonoTouch.CoreImage.CIImage.FromCGImage (img.CGImage);
                var transform = MonoTouch.CoreGraphics.CGAffineTransform.MakeScale (size.Width / img.Size.Width * 2, size.Height / img.Size.Height * 2);
                var affineTransform = new MonoTouch.CoreImage.CIAffineTransform () { 
                    Image = ciimage,
                    Transform = transform
                };
                var output = affineTransform.OutputImage;
                var context = MonoTouch.CoreImage.CIContext.FromOptions (null);
                var a = context.CreateCGImage (output, output.Extent);

                _imageView.BeginInvokeOnMainThread(() => {
                    if (img == currentLoad)
                    {
                        FillWithLocal(UIImage.FromImage(a));
                    }
                });

            });
        }

        public override void FillViewWithObject (MonoTouch.Foundation.NSObject obj)
        {
        }

        private void RemoveSpinner()
        {
            if (_activity != null)
            {
                _activity.StopAnimating();
                _activity.RemoveFromSuperview();
                _activity = null;
            }
        } 
    }

}
