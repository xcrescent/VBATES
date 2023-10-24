using Microsoft.CognitiveServices.Speech;
using System.Data.SqlClient;
using System;
using System.Windows.Forms;
using System.Linq;

namespace VBATES
{
    public partial class Form1 : Form
    {

        private SpeechRecognizer recognizer;

        public Form1()
        {
            InitializeComponent();
            InitializeSpeechRecognition();
        }

        private void InitializeSpeechRecognition()
        {
            var config = SpeechConfig.FromSubscription("ef675da96ee740639a0cac547c370f5d", "southeastasia");
            recognizer = new SpeechRecognizer(config);
            Console.WriteLine("Say something...");
            recognizer.Recognized += (s, e) =>
            {
                if (e.Result.Reason == ResultReason.RecognizedSpeech)
                {
                    string query = e.Result.Text;
                    //StoreInDatabase(query);
                    string voiceResponse = "Recognized: " + query;
                    
                    Recognizer_Recognized(s, e);
                    Speak(voiceResponse);
                }
            };

            recognizer.StartContinuousRecognitionAsync().Wait();
        }
        private void Recognizer_Recognized(object sender, SpeechRecognitionEventArgs e)
        {
            if (e.Result.Reason == ResultReason.RecognizedSpeech)
            {
                string recognizedText = e.Result.Text;

                // Define some keywords
                string fromKeyword = "from";
                string toKeyword = "to";
                string atKeyword = "at";

                // Find the positions of keywords
                int fromIndex = recognizedText.IndexOf(fromKeyword);
                int toIndex = recognizedText.LastIndexOf(toKeyword);
                int atIndex = recognizedText.IndexOf(atKeyword);

                if (fromIndex != -1 && toIndex != -1 && atIndex != -1)
                {
                    // Extract source, destination, and time
                    string source = recognizedText.Substring(fromIndex + fromKeyword.Length, toIndex - fromIndex - fromKeyword.Length).Trim();
                    string destination = recognizedText.Substring(toIndex + toKeyword.Length, atIndex - toIndex - toKeyword.Length).Trim();
                    string time = recognizedText.Substring(atIndex + atKeyword.Length).Trim();

                    // Now you have the source, destination, and time
                    Console.WriteLine($"Source: {source}, Destination: {destination}, Time: {time}");
                    UpdateResponseLabel(text: $"Source: {source}, Destination: {destination}, Time: {time}");
                }
                else
                {
                    // Handle case where keywords are not found
                    Console.WriteLine("Command not understood. Please include 'from', 'to', and 'at' in your command.");
                    UpdateResponseLabel(text: "Command not understood. Please include 'from', 'to', and 'at' in your command.");
                }
            }
        }

        private void Speak(string text)
        {
            SpeechConfig speechConfig = SpeechConfig.FromSubscription("ef675da96ee740639a0cac547c370f5d", "southeastasia");
            SpeechSynthesizer synthesizer = new SpeechSynthesizer(speechConfig);
            synthesizer.SpeakTextAsync(text).Wait();
        }

        private void StoreInDatabase(string query)
        {
            using (SqlConnection conn = new SqlConnection("Server=localhost;Database=TransportEnquirySystem;Trusted_Connection=True;"))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(
                    "IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Enquiries') " +
                    "CREATE TABLE Enquiries (Id INT PRIMARY KEY IDENTITY(1,1), Query NVARCHAR(MAX), Response NVARCHAR(MAX))", conn))
                {
                    cmd.ExecuteNonQuery();
                }

                using (SqlCommand cmd = new SqlCommand("INSERT INTO Enquiries (Query) VALUES (@Query)", conn))
                {
                    cmd.Parameters.AddWithValue("@Query", query);
                    cmd.ExecuteNonQuery();
                }
            }

        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            recognizer.StopContinuousRecognitionAsync().Wait();
        }

        private void EnquiryButton_Click(object sender, EventArgs e)
        {
            string query = EnquiryTextBox.Text;
            Console.WriteLine(query);   
            StoreInDatabase(query);
            Console.WriteLine(query);
            string voiceResponse = "Recognized: " + query;
            ResponseLabel.Text = voiceResponse;
            Speak(voiceResponse);
        }


        private void UpdateResponseLabel(string text)
        {
            if (ResponseLabel.InvokeRequired)
            {
                ResponseLabel.Invoke(new Action(() => { ResponseLabel.Text = text; }));
            }
            else
            {
                ResponseLabel.Text = text;
            }
        }


    }
}
