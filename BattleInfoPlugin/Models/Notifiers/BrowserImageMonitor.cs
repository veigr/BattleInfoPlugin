using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Reactive.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using BattleInfoPlugin.Win32;
using mshtml;
using SHDocVw;
using IServiceProvider = BattleInfoPlugin.Win32.IServiceProvider;
using WebBrowser = System.Windows.Controls.WebBrowser;
using BattleInfoPlugin.Properties;
using Grabacr07.KanColleWrapper;
using BattleInfoPlugin.Models.Notifiers._Internal;

namespace BattleInfoPlugin.Models.Notifiers
{
    internal class BrowserImageMonitor
    {
        private WebBrowser kanColleBrowser;

        private bool isConfirmPursuitNotified;

        private bool isInCombat;

        public event Action ConfirmPursuit;

        public BrowserImageMonitor()
        {
            Observable.Interval(TimeSpan.FromMilliseconds(1000))
                .ObserveOn(SynchronizationContext.Current)
                .Subscribe(_ => this.CheckImage());

            var proxy = KanColleClient.Current.Proxy;

            proxy.api_start2
                .ObserveOn(SynchronizationContext.Current)
                .Subscribe(_ => this.FindKanColleBrowser());

            proxy.ApiSessionSource.Subscribe(_ => this.isConfirmPursuitNotified = false);

            proxy.ApiSessionSource
                .Where(x => x.Request.PathAndQuery == "/kcsapi/api_req_practice/battle")
                .Subscribe(_ => this.isInCombat = true);
            proxy.api_req_map_start
                .Subscribe(_ => this.isInCombat = true);
            proxy.api_port
                .Subscribe(_ => this.isInCombat = false);
        }

        private void FindKanColleBrowser()
        {
            // 強引！
            var host = Application.Current.MainWindow?.FindName("kanColleHost");
            this.kanColleBrowser = host?.GetType().GetProperty("WebBrowser")?.GetValue(host) as WebBrowser;
        }

        private void CheckImage()
        {
            if (!this.isInCombat) return;
            if (this.isConfirmPursuitNotified) return;

            var image = this.kanColleBrowser?.GetImage();
            if (image == null) return;

            // 雑比較
            var browserImage = image.Resize().GetBitmapBytes();
            var confirmPursuitImage = Resources.ConfirmPursuit.Resize().GetBitmapBytes();
            var diff = browserImage.Zip(confirmPursuitImage, (a, b) => Math.Abs(a - b)).Average();
            System.Diagnostics.Debug.WriteLine(diff);
            if (diff < 0.9)
            {
                // ボタンマウスオーバー状態とかでも大体0.5くらいまでに収まる
                // 戦闘終了の瞬間は 1.2 位の事も
                this.ConfirmPursuit?.Invoke();
                this.isConfirmPursuitNotified = true;
            }

            image.Dispose();
        }
    }
}

namespace BattleInfoPlugin.Models.Notifiers._Internal
{
    internal static class Extensions
    {
        public static Bitmap GetImage(this WebBrowser browser)
        {
            var document = browser?.Document as HTMLDocument;
            if (document == null) return null;

            if (document.url.Contains(".swf?"))
            {
                var viewObject = document.getElementsByTagName("embed").item(0, 0) as IViewObject;
                if (viewObject == null) return null;

                var width = ((HTMLEmbed)viewObject).clientWidth;
                var height = ((HTMLEmbed)viewObject).clientHeight;
                return GetScreenshot(viewObject, width, height);
            }

            var gameFrame = document.getElementById("game_frame")?.document as HTMLDocument;
            if (gameFrame == null) return null;

            var frames = document.frames;
            for (var i = 0; i < frames.length; i++)
            {
                var item = frames.item(i);
                var provider = item as IServiceProvider;
                if (provider == null) continue;

                object ppvObject;
                provider.QueryService(typeof(IWebBrowserApp).GUID, typeof(IWebBrowser2).GUID, out ppvObject);
                var webBrowser = ppvObject as IWebBrowser2;
                var iframeDocument = webBrowser?.Document as HTMLDocument;
                var swf = iframeDocument?.getElementById("externalswf");
                if (swf == null) continue;

                IViewObject viewObject = null;
                var width = 0;
                var height = 0;
                Func<dynamic, bool> findSwf = target =>
                {
                    if (target == null) return false;
                    viewObject = target as IViewObject;
                    if (viewObject == null) return false;
                    width = int.Parse(target.width);
                    height = int.Parse(target.height);
                    return true;
                };
                if (findSwf(swf as HTMLEmbed) || findSwf(swf as HTMLObjectElement))
                    return GetScreenshot(viewObject, width, height);
            }
            return null;
        }

        private static Bitmap GetScreenshot(IViewObject viewObject, int width, int height)
        {
            var image = new Bitmap(width, height, PixelFormat.Format24bppRgb);
            var rect = new RECT { left = 0, top = 0, width = width, height = height, };
            var tdevice = new DVTARGETDEVICE { tdSize = 0, };

            using (var graphics = Graphics.FromImage(image))
            {
                var hdc = graphics.GetHdc();
                viewObject.Draw(1, 0, IntPtr.Zero, tdevice, IntPtr.Zero, hdc, rect, null, IntPtr.Zero, IntPtr.Zero);
                graphics.ReleaseHdc(hdc);
            }
            return image;
        }

        public static Bitmap Resize(this Bitmap bmp)
        {
            var result = new Bitmap(80, 48);
            using (var g = Graphics.FromImage(result))
            {
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.Low;
                g.DrawImage(bmp, 0, 0, result.Width, result.Height);
            }
            return result;
        }

        public static byte[] GetBitmapBytes(this Bitmap bmp)
        {
            var rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
            var data = bmp.LockBits(rect, ImageLockMode.ReadOnly, bmp.PixelFormat);
            try
            {
                var ptr = data.Scan0;
                var stride = Math.Abs(data.Stride);
                var bytes = new byte[stride * bmp.Height];
                Marshal.Copy(ptr, bytes, 0, bytes.Length);
                return bytes;
            }
            finally
            {
                bmp.UnlockBits(data);
            }
        }
    }
}
