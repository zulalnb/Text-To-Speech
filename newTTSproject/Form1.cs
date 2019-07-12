using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using NAudio.Mixer;
using NAudio.Wave;

namespace newTTSproject
{
    public partial class Form1 : Form
    {
        HaberService1.AkıllıHaberSoapClient client = new HaberService1.AkıllıHaberSoapClient();
        string title, article, audio;

        private void Form1_Load(object sender, EventArgs e)
        {
            timer1.Interval = 1000;
            timer1.Start();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            var response = client.GetMakale(title, article);
            DataSet rs = response;
            foreach (DataRow item in rs.Tables[0].Rows)
            {
                title = item.ItemArray[3].ToString();
                article = item.ItemArray[4].ToString();
                audio = item.ItemArray[6].ToString();

                if (audio == null)
                {
                    string msg = article;
                    if (msg.Length < 255)
                    {
                        ses(msg);
                    }
                    else
                    {
                        divide(msg);
                    }
                }
            }

        }

        void divide(string text)
        {
            var phrases = new List<string>();
            var words = text.Split(' ');

            var phrase = words[0];
            var index = 1;
            var k = 0;

            while (index < words.Length)
            {
                var word = words[index];
                var newLength = phrase.Length + 1 + word.Length;

                if (newLength < 255)
                {
                    phrase += " " + word;
                }
                else
                {
                    phrases.Add(phrase);                 
                    phrase = word;
                }
                index++;
            }
            phrases.Add(phrase);
            var cumleler = phrases.ToArray<string>();
            multises(cumleler);
        }

        void Combine(string[] inputFiles, Stream output)
        {
            foreach (string file in inputFiles)
            {
                Mp3FileReader reader = new Mp3FileReader(file);
                if ((output.Position == 0) && (reader.Id3v2Tag != null))
                {
                    output.Write(reader.Id3v2Tag.RawData, 0, reader.Id3v2Tag.RawData.Length);
                }
                Mp3Frame frame;
                while ((frame = reader.ReadNextFrame()) != null)
                {
                    output.Write(frame.RawData, 0, frame.RawData.Length);
                }
            }

        }

        void multises(string[] cumles)
        {
            var sesler = new List<string>();
            
            

            using (var clientweb = new WebClient())
            {
                foreach (var item in cumles)
                {
                    var content = clientweb.DownloadData(new TextToSpeech(item).location);

                    using (var stream = new MemoryStream(content))
                    {
                        
                            FileStream fs = new FileStream(title + Array.IndexOf(cumles,item) + ".mp3", FileMode.Create);
                            stream.WriteTo(fs);
                            fs.Close();
                            sesler.Add(title + Array.IndexOf(cumles, item) + ".mp3");
                      
                    }
                }

            }
            FileStream fsmerge = new FileStream(title + ".mp3", FileMode.Create);
            var voice = sesler.ToArray<string>();
            Combine(voice, fsmerge);
            File.Copy(title + ".mp3 ", @"C:\audiofiles\" + title + ".mp3");
            audio = "http://88.247.150.76/" + title + ".mp3 ";
            sesler = null;
            voice = null;

           
        }

        void ses(string txt)
        {

            using (var client = new WebClient())
            {

                var content = client.DownloadData(new TextToSpeech(txt).location);

                using (var stream = new MemoryStream(content))
                {
                    FileStream fs = new FileStream(title + ".mp3", FileMode.Create);
                    stream.WriteTo(fs);
                    fs.Close();
                    File.Copy(title + ".mp3 ", @"C:\audiofiles\" + title + ".mp3");
                    audio = "http://88.247.150.76/" + title + ".mp3 ";
                }
            }
        }

        public Form1()
        {
            InitializeComponent();
            
        }
        public class TextToSpeech
        {
            const string baseurl = "https://tts.voicetech.yandex.net/generate?text=";

            string format = "mp3";
            string lang = "tr-TR";
            string speaker = "zahar";
            double speed = 0.85;
            string emotion = "good";
            string key = "358b6df1-6fe8-43bf-8caf-4d38d144ccd7";

            public string location;

            public TextToSpeech(string Text)
            {
                location = $"{baseurl}{Text}{GetAttribute()}";

            }
            private string GetAttribute()
            {
                return $"&format={format}&lang={lang}&speaker={speaker}&speed={speed.ToString().Replace(',', '.')}&emotion={emotion}&key={key}";
            }
        }
    }
}
