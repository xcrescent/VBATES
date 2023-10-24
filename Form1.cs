using Microsoft.CognitiveServices.Speech;
using System.Data.SqlClient;
using System;
using System.Windows.Forms;
using System.Linq;
using System.Data;
using static System.Net.Mime.MediaTypeNames;

namespace VBATES
{
    public partial class Form1 : Form
    {

        private SpeechRecognizer recognizer;

        public Form1()
        {
            InitializeComponent();
            
        }
        private bool isListening = false;

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
            isListening = true;
        }
       
    

    private void AddSampleRow(int trainID, string trainName, string source, string destination, string departureTime, string arrivalTime)
    {
        DataGridViewRow row = new DataGridViewRow();
        row.CreateCells(dataGridView1);

        row.Cells[0].Value = trainID;
        row.Cells[1].Value = trainName;
        row.Cells[2].Value = source;
        row.Cells[3].Value = destination;
        row.Cells[4].Value = departureTime;
        row.Cells[5].Value = arrivalTime;
            if (dataGridView1.InvokeRequired)
            {
                dataGridView1.Invoke(new Action(() => { dataGridView1.Rows.Add(row); }));
            }
            else
            {
                dataGridView1.Rows.Add(row);
                   
            }
    }

        private void Recognizer_Recognized(object sender, SpeechRecognitionEventArgs e)
        {
            if (e.Result.Reason == ResultReason.RecognizedSpeech)
            {
                string recognizedText = e.Result.Text;
                UpdateResponseLabel(recognizedText);

                //string atKeyword = "at";
                string[] words = recognizedText.Split(' ');

                // Find keywords like "from", "to", and "at"
                int fromIndex = Array.IndexOf(words, "from");
                int toIndex = Array.LastIndexOf(words, "to");
                //int atIndex = recognizedText.IndexOf(atKeyword);

                if (fromIndex != -1 && toIndex != -1)
                {
                    // Extract source, destination, and time
                    string source = words[fromIndex + 1].Trim().Split('.')[0];
                    string destination = words[toIndex + 1].Replace(".", "").Replace("?","");
                    if (fromIndex < toIndex)
                    {
                        source = words[fromIndex + 1];
                        destination = words[toIndex + 1].Replace(".", "").Replace("?", "");
                    }
                    else
                    {
                        source = words[toIndex + 1];
                        destination = words[fromIndex + 1].Replace(".", "").Replace("?", "");
                    }
                    if (source != "" || destination != "")
                    {
                        // Initialize DataGridView columns
                        //if (dataGridView1.InvokeRequired)
                        //{
                        //    dataGridView1.Invoke(new Action(() =>
                        //    {
                        //        dataGridView1.Columns.Clear();
                        //        dataGridView1.Rows.Clear();
                        //        dataGridView1.Columns.Add("TrainID", "Train ID");
                        //        dataGridView1.Columns.Add("TrainName", "Train Name");
                        //        dataGridView1.Columns.Add("SourceStation", "Source Station");
                        //        dataGridView1.Columns.Add("DestinationStation", "Destination Station");
                        //        dataGridView1.Columns.Add("DepartureTime", "Departure Time");
                        //        dataGridView1.Columns.Add("ArrivalTime", "Arrival Time");
                        //    }));
                        //
                        //    // Add sample rows to the DataGridView
                        //    AddSampleRow(101, "Train X", "Delhi", "Mumbai", "08:00 AM", "12:00 PM");
                        //    AddSampleRow(102, "Train Y", "Mumbai", "Delhi", "10:00 AM", "02:00 PM");
                        //
                        //    UpdateResponseLabel(text: recognizedText);
                        //    return;
                        //}
                        try
                        {
                            using (SqlConnection conn = new SqlConnection("Server=localhost;Database=TransportEnquirySystem;Trusted_Connection=True;"))
                            {
                                conn.Open();

                                string query = "SELECT * FROM TransportInfo WHERE SourceStation = @SourceStation AND DestinationStation = @DestinationStation";


                                using (SqlCommand cmd = new SqlCommand(query, conn))
                                {
                                    cmd.Parameters.AddWithValue("@SourceStation", source);
                                    cmd.Parameters.AddWithValue("@DestinationStation", destination);
                                    Console.WriteLine("Executing SQL commands... "+ source + " " + destination);
                                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                                    DataTable dataTable = new DataTable();
                                    adapter.Fill(dataTable);
                                    foreach (DataRow row in dataTable.Rows)
                                    {
                                        foreach (var item in row.ItemArray)
                                        {
                                            Console.Write(item + "\t");
                                        }
                                        Console.WriteLine();
                                    }
                                    if (dataGridView1.InvokeRequired)
                                    {
                                        dataGridView1.Invoke(new Action(() => { dataGridView1.DataSource = dataTable; }));
                                    }
                                    else
                                    {
                                        dataGridView1.DataSource = dataTable;
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("Error executing SQL commands: " + ex.Message);
                            MessageBox.Show("Error fetching data: " + ex.Message);
                        }

                        // Now you have the source, destination, and time
                        Console.WriteLine($"Source: {source}, Destination: {destination}");
                        UpdateResponseLabel(text: $"Source: {source}, Destination: {destination}");
                    }
                    else
                    {
                        
                        // Handle case where keywords are not found
                        Console.WriteLine("Command not understood. Please include 'from', 'to', and 'at' in your command.");
                        UpdateResponseLabel(text: "Command not understood. Please include 'from', 'to', and 'at' in your command.");
                    }
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
        private void ExecuteSQLButton_Click(object sender, EventArgs e)
        {
            string connectionString = "Server=localhost;Database=TransportEnquirySystem;Trusted_Connection=True;";

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string sourceStation = "Mumbai";
                    string destinationStation = "Delhi";
                    string departureTime = "10:00 AM";
                    string arrivalTime = "02:00 PM";
                    string trainName = "Train Y";
                    using (SqlCommand cmd = new SqlCommand(
                    "IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'TransportInfo') " +
                    "CREATE TABLE TransportInfo ( ID INT PRIMARY KEY IDENTITY(1,1), SourceStation VARCHAR(100), DestinationStation VARCHAR(100), DepartureTime TIME, ArrivalTime TIME, TrainName VARCHAR(100))", connection))
                    {
                        cmd.ExecuteNonQuery();
                    }

                    using (SqlCommand cmd = new SqlCommand("INSERT INTO TransportInfo (SourceStation, DestinationStation, DepartureTime, ArrivalTime, TrainName) VALUES (@SourceStation, @DestinationStation, @DepartureTime, @ArrivalTime, @TrainName)", connection))
                    {
                        cmd.Parameters.AddWithValue("@SourceStation", sourceStation);
                        cmd.Parameters.AddWithValue("@DestinationStation", destinationStation);
                        cmd.Parameters.AddWithValue("@DepartureTime", departureTime);
                        cmd.Parameters.AddWithValue("@ArrivalTime", arrivalTime);
                        cmd.Parameters.AddWithValue("@TrainName", trainName);

                        cmd.ExecuteNonQuery();
                    }
                    
                    Console.WriteLine("SQL commands executed successfully.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error executing SQL commands: " + ex.Message);
            }
        }


        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            recognizer.StopContinuousRecognitionAsync().Wait();
        }

        private void EnquiryButton_Click(object sender, EventArgs e)
        {

            //ExecuteSQLButton_Click(sender,e);
            if (isListening)
            {
                StopListening();
                UpdateEnquiryButton("Start Voice Search");
            }
            else
            {
                InitializeSpeechRecognition();
                UpdateEnquiryButton("Stop Voice Search");
            }
            //            string voiceResponse = "Recognized: " + query;

        }
        // Function to stop listening
        private void StopListening()
        {
            if (recognizer != null && isListening)
            {
                recognizer.StopContinuousRecognitionAsync().Wait();
                isListening = false;
                Console.WriteLine("Listening stopped.");
            }
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

            private void UpdateEnquiryButton(string text)
        {
                if (EnquiryButton.InvokeRequired)
            {
                    EnquiryButton.Invoke(new Action(() => { EnquiryButton.Text = text; }));
                }
                else
            {
                    EnquiryButton.Text = text;
                }
            }   


    }
}
