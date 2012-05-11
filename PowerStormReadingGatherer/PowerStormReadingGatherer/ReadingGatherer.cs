using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.IO;
using System.Diagnostics;
using System.Threading;
using MySql.Data.MySqlClient;
using PowerStormReadingGatherer;
namespace MonoTest
{
	/// <summary>
	/// Program. Runs the ReadingGatherer.
	/// </summary>
	/// <exception cref='Exception'>
	/// Represents errors that occur during application execution.
	/// </exception>
	/// <exception cref='FileNotFoundException'>
	/// Is thrown when a file path argument specifies a file that does not exist.
	/// </exception>
    class Program
    {
		const int METER_COUNT = 4;
        DateTime now;
        DateTime timeOfLastUpdate;
		SqlCommand query;
        SqlDataReader reader;
		String winUserId;
		String winPass;
		SqlConnection dbEnvision;
		MySqlConnection dbPower;
		MySqlConnection localhost;
		
		//stores all of the dorm information
		public List<DormInfo> dormList;

		/// <summary>
		/// Initializes a new instance of the <see cref="MonoTest.Program"/> class.
		/// Begins periodically checking for new readings
		/// </summary>
        Program()
        {			
			//Reads in the "lastDormReading.csv"
			ReadCSV();
			
			//Get windows login credentials for authenticating with SQL server
			PromptForAuthentication();
			
			DataCleaner cleaner = new DataCleaner(dbPower);
			
			//Begin checking to see if updates are necessary every five minutes
			while(true) 
			{
				//Fetch newest electricity_readings
				FetchReadings();
				
				//Stores the index of the last electricity_reading we read from.
				SaveCSV();
				
				cleaner.CleanData();
				
				//Wait for five minutes, before we check for new readings
				Thread.Sleep(Constants.READER_GATHERER_SLEEP);
			}
        }

		/// <summary>
		/// Newest query, that returns all of the newest readings since the last read "Index"
		/// </summary>
		/// <returns>
		/// The get envision trendlog readings.
		/// </returns>
		/// <param name='trendlog_id'>
		/// Which table to read from
		/// </param>
		/// <param name='index_from'>
		/// Last index read
		/// </param>
		String SqlGetEnvisionTrendlogReadings(string trendlog_id, int index_from)
		{
			//We only want readings of type 2 (since type 1 will have a different reading value)
			String q = 	@"SELECT * FROM " + trendlog_id + 
						" WHERE [Index] > " + index_from.ToString() + 
						" AND [ValueType] = 2;";
			return q;
		}

		/// <summary>
		/// Inserts an electricity reading into the powerstorm MySql database with the given attributes
		/// </summary>
		/// <param name='meterId'>
		/// Meter identifier.
		/// </param>
		/// <param name='timeOfReading'>
		/// Time of reading.
		/// </param>
		/// <param name='reading'>
		/// Reading.
		/// </param>
		void SqlInsertReading(int meterId, DateTime timeOfReading, double reading) {
			String q = 	"INSERT INTO electricity_readings (meter_id, date_time, power) VALUES ("
						+ meterId
						+ ", CAST('" + timeOfReading.ToString("yyyy-MM-dd HH:mm:ss") + "' AS DATETIME), "
						+ reading + ");";
			
			new MySqlCommand(q, dbPower).ExecuteNonQuery();
			
			Console.WriteLine(meterId + ",   " + timeOfReading + ",   " + reading);
		}
		
		/// <summary>
		///  Prompts the user for windows login credentials for authenticating with SQL server
		/// </summary>
		void PromptForAuthentication() 
		{
			bool sentinel = true;
			
			while (sentinel)
			{
				//These login details are used to log onto the network
				winUserId = "pyoho11";
				winPass = "2Bway2cool";
				
                dbEnvision = new SqlConnection(@"Server=10.21.40.38\alerton;
                        Database=ENVISION;
						User ID=Admin\" + winUserId + @";
                        Password=" + winPass + @";
						Integrated Security=SSPI;");
				
				try 
				{
					dbEnvision.Open();
					sentinel = false;
					dbEnvision.Close();
				}
				catch(Exception e)
				{
					sentinel = true;
				}
			}		
		}
		
		/// <summary>
		/// Establishes a connection to the Envision database
		/// </summary>
		void ConnectEnvision() 
		{
            dbEnvision = new SqlConnection(@"Server=10.21.40.38\alerton;
                    Database=ENVISION;
					User ID=Admin\" + winUserId + @";
                    Password=" + winPass + @";
					Integrated Security=SSPI;");
			dbEnvision.Open();
		}

		/// <summary>
		/// Establishes a connection to the Powerstorm database
		/// </summary>
		void ConnectPowerStorm() 
		{
			dbPower = new MySqlConnection(@"
					Server=10.15.2.1;
					Port=5875;
                    Database=powerstorm_data;
                    User ID=rails_powerstorm;
                    Password=0wtwFK-a2xn;");
			dbPower.Open();
		}

        //Local Machine connection
     /*   void ConnectPowerStorm()
        {
            dbPower = new MySqlConnection(@"
       Server=localhost;
 	   Database=powerstorm_data;
         Uid=root;
            password=;");

            dbPower.Open();

        }*/

		/// <summary>
		/// Retrieves each new reading from the Envision database and inserts it into the Powerstorm database
		/// </summary>
		void FetchReadings()
        {
            try
            {
				// Establish connections to the required databases
				ConnectPowerStorm();
				ConnectEnvision();

				for(int i = 0; i < dormList.Count; i++)
				{
					//new SqlCommand to be executed on the Envision database
					query = new SqlCommand(SqlGetEnvisionTrendlogReadings(dormList[i].trendLogString, dormList[i].indexFrom), dbEnvision);
					
					//execute reader, so that we can get the newest electricity readings
					reader = query.ExecuteReader();
					
					//stores the power amount from the previous reading (-5 minutes before)
					int previousPower = 0;
					
					//stores the power amount from the current reading (0 minutes before)
					int latestPower = 0;
					
					//Cycle through all of the new readings
					while(reader.Read ())
					{
						
						Console.WriteLine ("Time of sample: " + reader["TimeOfSample"].ToString() + " Meter#: " + (i+1).ToString());
						
						//Only consider the reading if it was an interval of 5 minutes.
						if(((DateTime)reader.GetValue(Constants.ENVISION_DATABASE_TIMEOFSAMPLE)).Minute % 5 == 0)
						{
							
							//if, we didn't have a previous reading, store that as the previousPower
							if(previousPower == 0)
							{
								previousPower = Convert.ToInt32(reader.GetValue(Constants.ENVISION_DATABASE_SAMPLEVALUE));
								
							//else, store that as the latestPower 
							}
							else
							{
								latestPower = Convert.ToInt32(reader.GetValue(Constants.ENVISION_DATABASE_SAMPLEVALUE));
								
								//Since we have both readings, lets store the data
								SqlInsertReading(i+1, (DateTime)reader.GetValue(Constants.ENVISION_DATABASE_TIMEOFSAMPLE), (latestPower-previousPower));
								
								Console.WriteLine ("Meter id: " + (i+1).ToString() + " " + reader["TimeOfSample"].ToString() + " " + (latestPower-previousPower).ToString());          
								
								//set the previousPower to the latestPower
								previousPower = latestPower;
								
								//reset the latestPower
								latestPower = 0;
								
								//store the index of the value we read from (store the previous one so that we have a place to start)
								dormList[i].indexFrom = (int)reader["Index"] - 1;
							}
			
						}
					}
					
					reader.Close();
				}
				
				//close the connection to the databases
				dbPower.Close();
				dbEnvision.Close(); 

			}
            catch (Exception e)
            {
                Console.WriteLine("SQL Error: " + e.Message);
            }

        }
		
		/// <summary>
		/// add a new dormInfo to the dormList
		/// </summary>
		/// <param name='indexOfCSV'>
		/// Index of the CSV file.
		/// </param>
		/// <param name='name'>
		/// Name of the dorm.
		/// </param>
		/// <param name='trendLogString'>
		/// Trend log string.
		/// </param>
		/// <param name='indexFrom'>
		/// </param>
		public void AddItem(int indexOfCSV, string name, string trendLogString, int indexFrom)
    	{
            dormList.Add(new DormInfo(indexOfCSV, name, trendLogString, indexFrom));
    	}
		
		/// <summary>
		/// Dorm info class. Stores information about the dorm to be stored in CSV.
		/// </summary>
		public class DormInfo
		{
			//Line number of which the following data is stored in the "lastDormReading.csv"
			public int indexOfCSV;
			
			//Name of the dorm
			public string name;
			
			//String that helps us connect to the appropriate database
			public string trendLogString;
			
			//The index of the last electricity reading
			public int indexFrom;
			
			/// <summary>
			/// Initializes a new instance of the <see cref="MonoTest.Program.DormInfo"/> class.
			/// </summary>
			/// <param name='indexOfCSV'>
			/// Index of CSV.
			/// </param>
			/// <param name='name'>
			/// Name.
			/// </param>
			/// <param name='trendLogString'>
			/// Trend log string.
			/// </param>
			/// <param name='indexFrom'>
			/// Index from.
			/// </param>
			public DormInfo(int indexOfCSV, string name, string trendLogString, int indexFrom)
			{
				this.indexOfCSV = indexOfCSV;
				this.name = name;
				this.trendLogString = trendLogString;
				this.indexFrom = indexFrom;
			}
			
			
			/// <summary>
			/// This will save the newly updated dormInfo to the "lastDormReading.csv"
			/// </summary>
			public void SaveData()
			{
				using (System.IO.StreamWriter file = new System.IO.StreamWriter(@"lastDormReading.csv", true))
            	{
                	file.WriteLine(name + "," + trendLogString + "," + indexFrom.ToString());
            	}  
			}
			
		}
		
		/// <summary>
		/// Reads the CSV file.
		/// </summary>
		/// <exception cref='Exception'>
		/// Represents errors that occur during application execution.
		/// </exception>
		/// <exception cref='FileNotFoundException'>
		/// Is thrown when a file path argument specifies a file that does not exist.
		/// </exception>
		public void ReadCSV()
		{
			string fileName = "lastDormReading.csv";
				dormList = new List<DormInfo>();
				
				if ((fileName != null) && (!fileName.Equals(string.Empty)) && (File.Exists(fileName)))
        		{
            		try
            		{
                		List<string> rows = new List<string>();
                		StreamReader reader = new StreamReader(fileName, System.Text.Encoding.ASCII);
                		string record = reader.ReadLine();
                		int row_num = 0;
					
						// take all rows from the .csv file and store it
                		while (record != null)  
                		{
						
							// parse the rows and prepare each element for storage
                        	rows.Add(record);     
                    		record = reader.ReadLine();
                    		row_num++;
                		}

						int indexOfCSV = 0;
						
                		foreach (string s in rows)
                		{
                    		string[] convertedRow = s.Split(new char[] { ',' });
                    		int num_elements = convertedRow.Length;
						
							//only add it if there are 4 elements (in case there is something wrong with the row)
                    		if (num_elements == 3)              
							{
								AddItem (indexOfCSV, convertedRow[0], convertedRow[1], Convert.ToInt32(convertedRow[2]));
                    		}
							indexOfCSV++;
                		}
                        reader.Close();
            		}
            		catch //We had trouble reading in the file
            		{
                		throw new Exception("Error in ReadFromCsv: IO error."); //We had trouble reading from the file
            		}

        		}
        		else //file name was either "", null, or DNE
        		{
        		    throw new FileNotFoundException("Error in ReadFromCsv: the file path could not be found.");
        		} 
		}
		
		/// <summary>
		/// Saves the CSV file.
		/// </summary>
		public void SaveCSV()
		{
			string path = @"lastDormReading.csv";
        	try 
        	{
				//Delete the old file
				if(File.Exists(path))
				{
            		File.Delete(path);
				}
				
				//Create the new file
            	using (StreamWriter sw = File.CreateText(path)) {}
				
        	} 
        	catch (Exception e) 
        	{
            	Console.WriteLine("The process failed: {0}", e.ToString());
        	}
			
			//Save all data to the .csv file
			for(int i = 0; i < dormList.Count; i++)
			{
				dormList[i].SaveData();
			}
		}

        static void Main(string[] args)
        {
            new Program();
        }
    }
}
