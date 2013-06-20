using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Shapes;
using Windows.UI.Input;
using Windows.Devices.Input;

// 空白ページのアイテム テンプレートについては、http://go.microsoft.com/fwlink/?LinkId=234238 を参照してください

namespace MultiTouchChecker
{
    /// <summary>
    /// それ自体で使用できる空白ページまたはフレーム内に移動できる空白ページ。
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
        }

        private Dictionary<uint, Line> lineHList = new Dictionary<uint, Line>();
        private Dictionary<uint, Line> lineVList = new Dictionary<uint, Line>();
        private Dictionary<uint, Ellipse> ellipseList = new Dictionary<uint, Ellipse>();
        private Dictionary<uint, TextBlock> textBlockList = new Dictionary<uint, TextBlock>();
        /// <summary>
        /// このページがフレームに表示されるときに呼び出されます。
        /// </summary>
        /// <param name="e">このページにどのように到達したかを説明するイベント データ。Parameter 
        /// プロパティは、通常、ページを構成するために使用します。</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            String message = "";
            message += "Window:{Width:" + Window.Current.Bounds.Width+",";
            message += "Hight:" + Window.Current.Bounds.Height + "},";
            message += "Device:[";

            MaxContactsTB.Text = "";
            MaxContactsTB.Text += "Window";
            MaxContactsTB.Text += "\tWidth:" + Window.Current.Bounds.Width + "\n";
            MaxContactsTB.Text += "\tHight:" + Window.Current.Bounds.Height + "\n";
            int n = PointerDevice.GetPointerDevices().Count;
            for (int i = 0; i < n; i++)
            {
                MaxContactsTB.Text += "Device : " + i + "\n";
                MaxContactsTB.Text += "\tType : " + PointerDevice.GetPointerDevices()[i].PointerDeviceType + "\n";
                MaxContactsTB.Text += "\tMaxContacts : " + PointerDevice.GetPointerDevices()[i].MaxContacts;

                message += "{Type:" + PointerDevice.GetPointerDevices()[i].PointerDeviceType + ",";
                message += "MaxContacts:" + PointerDevice.GetPointerDevices()[i].MaxContacts + "}";
                if(i < n-1){
                    message += ",";
                    MaxContactsTB.Text += "\n";
                }
            }
            message += "]";
            UDPSender(message);

            messageTB.Width = Window.Current.Bounds.Width - 404;
            messageTB.Height = 50;

        }

        private void myPointerEntered(object sender, PointerRoutedEventArgs e)
        {
            var pt = e.GetCurrentPoint(canvas);
            
            double w = Window.Current.Bounds.Width;
            double h = Window.Current.Bounds.Height;
            Line lineH = new Line();
            lineH.Width = w;
            lineH.Height = h;
            lineH.Stroke = (pt.PointerDevice.PointerDeviceType.ToString() == "Mouse") ? LineBlue.Stroke : LineRed.Stroke;
            canvas.Children.Add(lineH);
            lineHList[pt.PointerId] = lineH;

            Line lineV = new Line();
            lineV.Width = w;
            lineV.Height = h;
            lineV.Stroke = (pt.PointerDevice.PointerDeviceType.ToString() == "Mouse") ? LineBlue.Stroke : LineRed.Stroke;
            canvas.Children.Add(lineV);
            lineVList[pt.PointerId] = lineV;

            Ellipse ellipse = new Ellipse();
            ellipse.Fill = (pt.PointerDevice.PointerDeviceType.ToString() == "Mouse") ? LineBlue.Fill : LineRed.Fill;
            ellipse.Width = 60;
            ellipse.Height = 60;
            canvas.Children.Add(ellipse);
            ellipseList[pt.PointerId] = ellipse;

            TextBlock textBlock = new TextBlock();
            textBlock.FontSize = 14;
            canvas.Children.Add(textBlock);
            textBlockList[pt.PointerId] = textBlock;

            Thickness th = ellipse.Margin;
            th.Left = -100;
            th.Top = -100;
            ellipse.Margin = th;
            textBlock.Margin = th;
        }

        private void myPointerMoved(object sender, PointerRoutedEventArgs e)
        {
            double w = Window.Current.Bounds.Width;
            double h = Window.Current.Bounds.Height;
            var pt = e.GetCurrentPoint(canvas);

            if (lineHList[pt.PointerId] == null)
            {
                myPointerEntered(sender, e);
            }

            Line lineH = lineHList[pt.PointerId];
            Line lineV = lineVList[pt.PointerId];
            Ellipse ellipse = ellipseList[pt.PointerId];
            TextBlock textBlock = textBlockList[pt.PointerId];

            if (lineH != null)
            {
                lineH.X1 = 0;
                lineH.X2 = w;
                lineH.Y1 = lineH.Y2 = pt.Position.Y;
            }
            if (lineV != null)
            {
                lineV.X1 = lineV.X2 = pt.Position.X;
                lineV.Y1 = 0;
                lineV.Y2 = h;
            }
            if (ellipse != null)
            {
                Thickness th = ellipse.Margin;
                th.Left = pt.Position.X - ellipse.Width * 0.5;
                th.Top = pt.Position.Y - ellipse.Height * 0.5;
                ellipse.Margin = th;

                th.Left = pt.Position.X;
                th.Top = pt.Position.Y - 90;
                textBlock.Margin = th;
                textBlock.Text = "ID:" + pt.PointerId.ToString() + "\nX:" + pt.Position.X + "\nY:" + pt.Position.Y;
            }

            String message = "Moved:{";
            message += "Type:" + pt.PointerDevice.PointerDeviceType + ",";
            message += "Id:" + pt.PointerId + ",";
            message += "X:" + pt.Position.X + ",";
            message += "Y:" + pt.Position.Y + "}";
            UDPSender(message);

        }

        private void UDPSender(String message)
        {
            messageTB.Text = message;
        }

        private void myPointerReleased(object sender, PointerRoutedEventArgs e)
        {
            pointerClose(e, "Released");
        }
        private void myPointerExit(object sender, PointerRoutedEventArgs e)
        {
            pointerClose(e, "Exit");
        }
        private void myPointerCanceled(object sender, PointerRoutedEventArgs e)
        {
            pointerClose(e, "Canceled");
        }
        private void myPointerCaptureLost(object sender, PointerRoutedEventArgs e)
        {
            pointerClose(e, "CaptureLost");
        }
        private void pointerClose(PointerRoutedEventArgs e, String eventName)
        {
            var pt = e.GetCurrentPoint(canvas);
            if (lineHList[pt.PointerId] != null)
            {
                canvas.Children.Remove(lineHList[pt.PointerId]);
                canvas.Children.Remove(lineVList[pt.PointerId]);
                canvas.Children.Remove(ellipseList[pt.PointerId]);
                canvas.Children.Remove(textBlockList[pt.PointerId]);

                lineHList[pt.PointerId] = null;
                lineVList[pt.PointerId] = null;
                ellipseList[pt.PointerId] = null;
                textBlockList[pt.PointerId] = null;

                String message = eventName + ":{";
                message += "Type:" + pt.PointerDevice.PointerDeviceType + ",";
                message += "Id:" + pt.PointerId + ",";
                message += "X:" + pt.Position.X + ",";
                message += "Y:" + pt.Position.Y + "}";
                UDPSender(message);
            }
        }

        private void canvas_Loaded(object sender, RoutedEventArgs e)
        {
            double w = Window.Current.Bounds.Width;
            double h = Window.Current.Bounds.Height;
            canvas.Width = Window.Current.Bounds.Width;
            canvas.Height = Window.Current.Bounds.Height;

            PointerEntered += myPointerEntered;

            PointerMoved += myPointerMoved;
            PointerReleased += myPointerReleased;
            PointerExited += myPointerExit;
            PointerCanceled += myPointerCanceled;
            PointerCaptureLost += myPointerCaptureLost;

            Thickness th = messageTB.Margin;
            th.Left = 32;
            th.Top = canvas.Height - 46;
            messageTB.Margin = th;

        }

    }
}
