using System;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using Xamarin.Forms;
using Plugin.SimpleAudioPlayer;
using WebSocket4Net;

namespace nyanpasu_button
{
    [DataContract]
    public class JsonItem
    {
        [DataMember(Name = "count")]
        public int Count { get; set; }
        [DataMember(Name = "mp3")]
        public string MP3 { get; set; }
    }

    // Learn more about making custom code visible in the Xamarin.Forms previewer
    // by visiting https://aka.ms/xamarinforms-previewer
    [DesignTimeVisible(false)]
    public partial class MainPage : ContentPage
    {
        private readonly WebSocket ws;

        private ISimpleAudioPlayer[] players;

        private const string HOST_NAME = "the-des-alizes.herokuapp.com";

        public MainPage()
        {
            InitializeComponent();

            this.players = Enumerable.Range(0, 10).Select(_ => CrossSimpleAudioPlayer.CreateSimpleAudioPlayer()).ToArray();
            foreach (var p in this.players)
            {
                using (var s = typeof(App).Assembly.GetManifestResourceStream("nyanpasu-button.Resources.nyanpasu.mp3"))
                {
                    p.Load(s);
                }
            }

            this.ws = new WebSocket(string.Format("wss://{0}/ws", MainPage.HOST_NAME));
            this.ws.Opened += Ws_Opened;
            this.ws.MessageReceived += Ws_MessageReceived;
            this.ws.Open();
        }

        private void Button_Clicked(object sender, EventArgs e)
        {
            try
            {
                if (this.ws == null) { return; }
                if (this.ws.State != WebSocketState.Open) { return; }

                var player = this.players.FirstOrDefault((x) => !x.IsPlaying);
                if (player != null)
                {
                    player.Play();
                }

                this.ws.Send("にゃんぱすー");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("***** send error!! *****");
                System.Diagnostics.Debug.WriteLine("***** {0} *****", ex.Message);
            }
        }

        private void Ws_Opened(object sender, EventArgs e)
        {
            Device.BeginInvokeOnMainThread(() => this.button.IsEnabled = true);
        }

        private void Ws_MessageReceived(object sender, MessageReceivedEventArgs e)
        {
            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(e.Message)))
            {
                var serializer = new DataContractJsonSerializer(typeof(JsonItem));
                var item = (JsonItem)serializer.ReadObject(stream);
                Device.BeginInvokeOnMainThread(() => this.label.Text = item.Count.ToString());
            }
        }
    }
}
