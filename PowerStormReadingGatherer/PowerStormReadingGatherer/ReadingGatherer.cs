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
			//Read from the file which persistantly stores the time of last update
            //timeOfLastUpdate = DateTime.Parse(File.ReadAllText(@"myFile.txt"));
			
			//Reads in the "lastDormReading.csv"
			ReadCSV();
			
			//Get windows login credentials for authenticating with SQL server
			PromptForAuthentication();
		
			//Begin checking to see if updates are necessary every five minutes
			while(true) 
			{
				
				FetchReadings();
				SaveCSV();
			
				//Save all the CSV files after we query each of the dorms
				//Console.WriteLine ("Saving CSV File!");
			//	Thread.Sleep (10000);
				Thread.Sleep(Constants.READER_GATHERER_SLEEP);
			}
        }
		
		/// <summary>
		/// Returns a SQL Query string that will retrieve each reading from the given meter within the given timeframe
		/// </summary>
		/// <returns>
		/// The envision readings.
		/// </returns>
		/// <param name='meterID'>
		/// Meter ID.
		/// </param>
		/// <param name='maxHoursToRetrieve'>
		/// Max hours to retrieve.
		/// </param>
		/// <param name='from'>
		/// Specifies the date to read from in the database.
		/// </param>
		/// <param name='until'>
		/// Specifies the date to read until in the database
		/// </param>
		String SqlGetEnvisionReadings(int meterID, int maxHoursToRetrieve, DateTime from, DateTime until)
		{
		    String q = @"SELECT TOP " + maxHoursToRetrieve + " * FROM (";
		    for (int i = 1; true; ++i)
		    {
		        q += " (SELECT"
		            + " DATEADD(minute, " + (i - 1)*15 + ", time_stamp) AS TimeOfReading, QtrHr" + i
		            + " AS Reading";
		        q += " FROM tblELHC_000000000" + meterID + ")";
		
		        if (i >= 4) { break; }
		
		        q += " UNION ";
		    }
		
		    q += ") AS Readings"
		        + " WHERE TimeOfReading > CAST('" + from.ToString("yyyy-MM-dd HH:mm:ss") + "' AS DATETIME)"
		        + " AND TimeOfReading <= CAST('" + until.ToString("yyyy-MM-dd HH:mm:ss") + "' AS DATETIME)"
		        + " ORDER BY TimeOfReading;";
		    return q;
		}
		
//		// Returns a SQL Query string that will retrieve each reading from the meters for Warren (2), Mac, and Boppel
//		String SqlGetEnvisionTrendlogReadings(string trendlog_id, int maxHoursToRetrive, DateTime from, DateTime until)
//		{
//			String q = @"SELECT TOP " + maxHoursToRetrive + " * FROM (";
//		    for (int i = 1; true; ++i)
//		    {
//		        q += " (SELECT"
//		            + " DATEADD(minute, " + (i - 1)*5 + ", TimeOfSample) AS TimeOfReading, QtrHr" + i
//		            + " AS Reading";
//		        q += " FROM " + trendlog_id /*tblELHC_000000000" + meterID */ + ")";
//		
//		        if (i >= 20) { break; }
//		
//		        q += " UNION ";
//		    }
//		
//		    q += ") AS Readings"
//		        + " WHERE TimeOfReading > CAST('" + from.ToString("yyyy-MM-dd HH:mm:ss") + "' AS DATETIME)"
//		        + " AND TimeOfReading <= CAST('" + until.ToString("yyyy-MM-dd HH:mm:ss") + "' AS DATETIME)"
//		        + " ORDER BY TimeOfReading;";
//		    return q;
//		}
		

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
		/// This is used for testing instead of SqlInsertReadings
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
		void SqlInsertlocalhost(int meterId, DateTime timeOfReading, double reading) 
		{
			String q = 	"INSERT INTO electricity_readings (meter_id, date_time, power) VALUES ("
						+ meterId
						+ ", CAST('" + timeOfReading.ToString("yyyy-MM-dd HH:mm:ss") + "' AS DATETIME), "
						+ reading + ");";
			
			new MySqlCommand(q, localhost).ExecuteNonQuery();
			
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
				//Console.Write("Username: ");
				//winUserId = Console.ReadLine();
				winUserId = "pyoho11";
				
				//Console.Write("Password: ");
				//winPass = Console.ReadLine();
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
		
		/// <summary>
		/// Establishes a connection to local host for testing
		/// </summary>
		void Connectlocalhost() 
		{
			localhost = new MySqlConnection(@"
					Server=localhost;
					Database=powerstorm_data;
					Uid=root;
					password=;");
			localhost.Open();
		}
		
		/// <summary>
		/// Retrieves each new reading from the Envision database and inserts it into the Powerstorm database
		/// </summary>
		void FetchReadings()
        {
            try
            {
				// Establish connections to the required databases
				ConnectPowerStorm();
				//Use Connectlocalhost for testing
				//Connectlocalhost();
				ConnectEnvision();
				
				// 'Now' is set to now because we are using current data
				//now = DateTime.Now;  //.Subtract(new TimeSpan(365, 0, 0, 0));
				
				/*
				// Iterates through each meter index and queries each new reading associated with it
				for (int i = 1; i <= METER_COUNT; ++i) {
					// Retrieves each new reading associated with the meter index from the Envision database
	                query = new SqlCommand(SqlGetEnvisionReadings(i, 1000000000, timeOfLastUpdate, now), dbEnvision);
	                reader = query.ExecuteReader();
					
					// Inserts each new reading into the powerstorm database
	                while (reader.Read()) {
						SqlInsertReading(i, (DateTime)reader.GetValue(0), (double)reader.GetValue(1));
	                }
					
					reader.Close();
				}
				*/
				
				for(int i = 0; i < dormList.Count; i++)
				{
					//new SqlCommand
					query = new SqlCommand(SqlGetEnvisionTrendlogReadings(dormList[i].trendLogString, dormList[i].indexFrom), dbEnvision);
					
					//execute reader
					reader = query.ExecuteReader();
					
					//stores the power amount from the previous reading (-5 minutes before)
					int previousPower = 0;
					
					//stores the power amount from the current reading (0 minutes before)
					int latestPower = 0;
					
					//Cycle through all of the new readings
					while(reader.Read ())
					{
						
						Console.WriteLine ("Time of sample: " + reader["TimeOfSample"].ToString() + " Meter#: " + (i+1).ToString());
						// 0 -> Index
						// 1 -> TimeOfSample
						// 2 -> Sequence
						// 3 -> ValueType
						// 4 -> SampleValue
						
						//Only consider the reading if it was an interval of 5 minutes.
						if(((DateTime)reader.GetValue(Constants.POWERSTORM_DATABASE_DATETIME)).Minute % 5 == 0)
						{
							
							//if, we didn't have a previous reading, store that as the previousPower
							if(previousPower == 0)
							{
								previousPower = Convert.ToInt32 (reader.GetValue (Constants.POWERSTORM_DATABASE_POWER));
								
							//else, store that as the latestPower 
							}
							else
							{
								latestPower = Convert.ToInt32 (reader.GetValue (Constants.POWERSTORM_DATABASE_POWER));
								
								//Since we have both readings, lets store the data
								//SqlInsertlocalhost(i+1, (DateTime)reader.GetValue (1), (latestPower-previousPower));
								SqlInsertReading(i+1, (DateTime)reader.GetValue (Constants.POWERSTORM_DATABASE_DATETIME), (latestPower-previousPower));
								
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
					//condense the readings
					//insert readings into powerstorm database
					//SqlInsertReading(i, (DateTime)reader.GetValue(0), (double)reader.GetValue(1));
					//SqlInsertlocalhost(i, (DateTime)reader.GetValue(0), (double)reader.GetValue(1));
				}
				
//				// Get Boppel info
//				
//				// Retrieves each new reading associated with the meter index from the Envision database
//                query = new SqlCommand(SqlGetEnvisionTrendlogReadings("tblTrendlog_0002300_0000000002", 1000000000, timeOfLastUpdate, now), dbEnvision);
//                reader = query.ExecuteReader();
//				
//				// Inserts each new reading into the powerstorm database
//                while (reader.Read()) {
//					SqlInsertReading(1, (DateTime)reader.GetValue(0), (double)reader.GetValue(1));
//                }
//				
//				reader.Close();
//				
//				// Get Mac info
//				
//				// Retrieves each new reading associated with the meter index from the Envision database
//                query = new SqlCommand(SqlGetEnvisionTrendlogReadings("tblTrendlog_0006000_0000000000", 1000000000, timeOfLastUpdate, now), dbEnvision);
//                reader = query.ExecuteReader();
//				
//				// Inserts each new reading into the powerstorm database
//                while (reader.Read()) {
//					SqlInsertReading(2, (DateTime)reader.GetValue(0), (double)reader.GetValue(1));
//                }
//				
//				reader.Close();
//				
//				// Get Warren 1 info
//				
//				// Retrieves each new reading associated with the meter index from the Envision database
//                query = new SqlCommand(SqlGetEnvisionTrendlogReadings("tblTrendlog_0005000_0000000000", 1000000000, timeOfLastUpdate, now), dbEnvision);
//                reader = query.ExecuteReader();
//				
//				// Inserts each new reading into the powerstorm database
//                while (reader.Read()) {
//					SqlInsertReading(3, (DateTime)reader.GetValue(0), (double)reader.GetValue(1));
//                }
//				
//				reader.Close();
//				
//				// Get Warren 2 info
//				
//				// Retrieves each new reading associated with the meter index from the Envision database
//                query = new SqlCommand(SqlGetEnvisionTrendlogReadings("tblTrendlog_0015000_0000000000", 1000000000, timeOfLastUpdate, now), dbEnvision);
//                reader = query.ExecuteReader();
//				
//				// Inserts each new reading into the powerstorm database
//                while (reader.Read()) {
//					SqlInsertReading(4, (DateTime)reader.GetValue(0), (double)reader.GetValue(1));
//                }
//				
//				reader.Close();

				dbPower.Close();
				//localhost.Close();
				dbEnvision.Close(); 
					
				//timeOfLastUpdate = now;
	
	            //StreamWriter sw = new StreamWriter(@"myFile.txt");
	            //sw.WriteLine(now.ToString());
	            //sw.Close();
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
			
			public int indexOfCSV;
			public string name;
			public string trendLogString;
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

                	//	List<string[]> rowObjects = new List<string[]>();
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
				if(File.Exists (path))
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
			//Append all data to the .csv file
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
