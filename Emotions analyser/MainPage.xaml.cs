using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;


using System.Threading.Tasks;
using Windows.Media.Capture;
using Windows.UI.Xaml.Media.Imaging;
using Windows.Media.MediaProperties;
using Windows.Storage.Streams;

using Microsoft.ProjectOxford.Emotion;
using Microsoft.ProjectOxford.Emotion.Contract;


// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Emotions_analyser
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        MediaCapture mediaCapture;
        bool isPreviewing = false;
        Emotion[] emotionResult;

        public System.Collections.ObjectModel.ObservableCollection<Emotion> emo =
                    new System.Collections.ObjectModel.ObservableCollection<Emotion>();

        public MainPage()
        {
            this.InitializeComponent();
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            MediaCaptureInitializationSettings set = new MediaCaptureInitializationSettings();
            set.StreamingCaptureMode = StreamingCaptureMode.Video;
            mediaCapture = new MediaCapture();
            await mediaCapture.InitializeAsync(set);
        }

        private async void btnStartPreview_Click(object sender, RoutedEventArgs e)
        {
            if (isPreviewing == false)
            {
                previewElement.Source = mediaCapture;
                await mediaCapture.StartPreviewAsync();
                isPreviewing = true;
            }
            previewElement.Visibility = Visibility.Visible;
        }

        private async void btnTakePhoto_Click(object sender, RoutedEventArgs e)
        {
            btnTakePhoto.IsEnabled = false;
            btnStartPreview.IsEnabled = false;

            InMemoryRandomAccessStream stream = new InMemoryRandomAccessStream();
            await mediaCapture.CapturePhotoToStreamAsync(ImageEncodingProperties.CreateJpeg(), stream);

            stream.Seek(0);
            BitmapImage bitmap = new BitmapImage();
            bitmap.SetSource(stream);
            captureImage.Source = bitmap;

            stream.Seek(0);
            Stream st = stream.AsStream();

            if (isPreviewing == true) await mediaCapture.StopPreviewAsync();
            isPreviewing = false;
            previewElement.Visibility = Visibility.Collapsed;

            progring.IsActive = true;

                try
                {
                    EmotionServiceClient emotionServiceClient =
                            new EmotionServiceClient("12345678901234567890123456789012");
        // replace 12345678901234567890123456789012 with your key taken from https://www.projectoxford.ai/Subscription/
                    emotionResult = await emotionServiceClient.RecognizeAsync(st);
                }
                catch { }

            progring.IsActive = false;

            if ((emotionResult != null) && (emotionResult.Length > 0))
            {
                emo.Clear();
                emo.Add(emotionResult[0]);
                this.DataContext = emo.ElementAt(0);
            }
            btnStartPreview.IsEnabled = true;
            btnTakePhoto.IsEnabled = true;
        }
    }
}
