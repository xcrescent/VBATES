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

                //string atKeyword = "at";
                string[] words = recognizedText.Split(' ');

                // Find keywords like "from", "to", and "at"
                int fromIndex = Array.IndexOf(words, "from");
                int toIndex = Array.LastIndexOf(words, "to");
                //int atIndex = recognizedText.IndexOf(atKeyword);

                if (fromIndex != -1 && toIndex != -1)
                {
                    // Extract source, destination, and time
                    string source = words[fromIndex+1];
                    string destination = words[toIndex+1];
                    //string time = recognizedText.Substring(atIndex + atKeyword.Length).Trim();
                    try
                    {
                        string connectionString = "Server = localhost; Database = TransportEnquirySystem; Trusted_Connection = True;";
                        using (SqlConnection connection = new SqlConnection(connectionString))
                        {
                            connection.Open();


                            string query = "SELECT * FROM Trains_xx " +
                                           "JOIN Schedules_xx ON Trains_xx.TrainID = Schedules_xx.TrainID " +
                                           "WHERE Trains_xx.SourceStationID IN (SELECT StationID FROM Stations_xx WHERE StationName = @Source) " +
                                           "AND Trains_xx.DestinationStationID IN (SELECT StationID FROM Stations_xx WHERE StationName = @Destination)";

                            using (SqlCommand command = new SqlCommand(query, connection))
                            {
                                command.Parameters.AddWithValue("@Source", source);
                                command.Parameters.AddWithValue("@Destination", destination);
//                                command.Parameters.AddWithValue("@Time", time);

                                using (SqlDataReader reader = command.ExecuteReader())
                                {
                                    if (reader.HasRows)
                                    {
                                        while (reader.Read())
                                        {
                                            Console.WriteLine($"Train ID: {reader["TrainID"]}, Train Name: {reader["TrainName"]}");
                                            // Add code to display or use the retrieved information
                                        }
                                    }
                                    else
                                    {
                                        Console.WriteLine("No matching trains found.");
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error querying the database: " + ex.Message);
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
                    using (SqlCommand cmd = new SqlCommand(
                    "IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Stations_xx') " +
                    "CREATE TABLE Stations_xx (StationID INT PRIMARY KEY, StationName VARCHAR(100))", connection))
                    {
                        cmd.ExecuteNonQuery();
                    }
                    using (SqlCommand cmd = new SqlCommand(
                    "IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Trains_xx') " +
                    "CREATE TABLE Trains_xx (TrainID INT PRIMARY KEY, TrainName VARCHAR(100), SourceStationID INT, DestinationStationID INT, FOREIGN KEY (SourceStationID) REFERENCES Stations_xx(StationID), FOREIGN KEY (DestinationStationID) REFERENCES Stations_xx(StationID))", connection))
                    {
                        cmd.ExecuteNonQuery();
                    }
                    using (SqlCommand cmd = new SqlCommand(
                                        "IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Schedules_xx') " +
                                        "CREATE TABLE Schedules_xx (ScheduleID INT PRIMARY KEY, TrainID INT, DepartureTime TIME, ArrivalTime TIME,FOREIGN KEY (TrainID) REFERENCES Trains_xx(TrainID))", connection))
                    {
                        cmd.ExecuteNonQuery();
                    }

                    using (SqlCommand cmd = new SqlCommand("INSERT INTO Stations_xx (StationID, StationName) VALUES (4, 'Mumbai'), (5, 'Delhi')", connection))
                    {
                       cmd.ExecuteNonQuery();
                    }
                    using (SqlCommand cmd = new SqlCommand("INSERT INTO Trains_xx (TrainID, TrainName, SourceStationID, DestinationStationID) VALUES (101, 'Train X', 4, 5), (102, 'Train Y', 5, 4)", connection))
                    {
                        cmd.ExecuteNonQuery();
                    }
                    using (SqlCommand cmd = new SqlCommand("    INSERT INTO Schedules_xx (ScheduleID, TrainID, DepartureTime, ArrivalTime) VALUES (1001, 101, '08:00:00', '12:00:00'), (1002, 102, '10:00:00', '14:00:00')", connection))
                    {
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
            string query = EnquiryTextBox.Text;
            ExecuteSQLButton_Click(sender,e);
            string voiceResponse = "Recognized: " + query;
            
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
