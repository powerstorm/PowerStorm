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

namespace MonoTest
{
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
		public List<dormInfo> dormList;
		
		// Initializes the program and begins periodically checking for new readings
        Program()
        {
			//Read from the file which persistantly stores the time of last update
            //timeOfLastUpdate = DateTime.Parse(File.ReadAllText(@"myFile.txt"));
			
			//Reads in the "lastDormReading.csv"
			readCSV();
			
			//Get windows login credentials for authenticating with SQL server
			PromptForAuthentication();
		
			//Begin checking to see if updates are necessary every five minutes
			while(true) {
				
				FetchReadings();
				saveCSV();
			
				//Save all the CSV files after we query each of the dorms
				//Console.WriteLine ("Saving CSV File!");
			//	Thread.Sleep (10000);
				Thread.Sleep(1000 * 60 * 5);
			}
        }
		// Returns a SQL Query string that will retrieve each reading from the given meter within the given timeframe
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
		
		//Newest query, that returns all of the newest readings since the last read "Index"
		//Takes in the trendlog_id (which table) and the index_from (last index read)
		String SqlGetEnvisionTrendlogReadings(string trendlog_id, int index_from){
			//We only want readings of type 2 (since type 1 will have a different reading value)
			String q = @"SELECT * FROM " + trendlog_id /*tblELHC_000000000" + meterID */ + " WHERE [Index] > " + index_from.ToString() + " AND [ValueType] = 2";
			return q;
		}
		
		// Inserts an electricity reading into the powerstorm MySql database with the given attributes
		void SqlInsertReading(int meterId, DateTime timeOfReading, double reading) {
			String q = "INSERT INTO electricity_readings (meter_id, date_time, power) VALUES ("
				+ meterId
				+ ", CAST('" + timeOfReading.ToString("yyyy-MM-dd HH:mm:ss") + "' AS DATETIME), "
				+ reading + ");";
			
			new MySqlCommand(q, dbPower).ExecuteNonQuery();
			
			Console.WriteLine(meterId + ",   " + timeOfReading + ",   " + reading);
		}
		
		//This is used for testing instead of SqlInsertReadings
		void SqlInsertlocalhost(int meterId, DateTime timeOfReading, double reading) {
			String q = "INSERT INTO electricity_readings (meter_id, date_time, power) VALUES ("
				+ meterId
				+ ", CAST('" + timeOfReading.ToString("yyyy-MM-dd HH:mm:ss") + "' AS DATETIME), "
				+ reading + ");";
			
			new MySqlCommand(q, localhost).ExecuteNonQuery();
			
			Console.WriteLine(meterId + ",   " + timeOfReading + ",   " + reading);
		}
		
		// Prompts the user for windows login credentials for authenticating with SQL server
		void PromptForAuthentication() {
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
				
				try {
					dbEnvision.Open();
					sentinel = false;
					dbEnvision.Close();
				}
				catch(Exception e){
					sentinel = true;
				}
			}		
		}
		// Establishes a connection to the Envision database
		void ConnectEnvision() {
            dbEnvision = new SqlConnection(@"Server=10.21.40.38\alerton;
                    Database=ENVISION;
					User ID=Admin\" + winUserId + @";
                    Password=" + winPass + @";
					Integrated Security=SSPI;");
			dbEnvision.Open();
		}
		// Establishes a connection to the Powerstorm database
		void ConnectPowerStorm() {
			dbPower = new MySqlConnection(@"
					Server=10.15.2.1;
					Port=5875;
                    Database=powerstorm_data;
                    User ID=rails_powerstorm;
                    Password=0wtwFK-a2xn;");
			dbPower.Open();
		}
		
		void Connectlocalhost() {
			localhost = new MySqlConnection(@"
					Server=localhost;
					Database=powerstorm_data;
					Uid=root;
					password=;");
			localhost.Open();
		}
		
		// Retrieves each new reading from the Envision database and inserts it into the Powerstorm database
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
				
				for(int i = 0; i < dormList.Count; i++){
					//new SqlCommand
					query = new SqlCommand(SqlGetEnvisionTrendlogReadings(dormList[i].trendLogString, dormList[i].indexFrom), dbEnvision);
					
					//execute reader
					reader = query.ExecuteReader();
					
					//stores the power amount from the previous reading (-5 minutes before)
					int previousPower = 0;
					
					//stores the power amount from the current reading (0 minutes before)
					int latestPower = 0;
					
					//Cycle through all of the new readings
					while(reader.Read ()){
						
						Console.WriteLine ("Time of sample: " + reader["TimeOfSample"].ToString() + " Meter#: " + (i+1).ToString());
						// 0 -> Index
						// 1 -> TimeOfSample
						// 2 -> Sequence
						// 3 -> ValueType
						// 4 -> SampleValue
						
						//Only consider the reading if it was an interval of 5 minutes.
						if(((DateTime)reader.GetValue(1)).Minute % 5 == 0){
							
							//if, we didn't have a previous reading, store that as the previousPower
							if(previousPower == 0){
								previousPower = Convert.ToInt32 (reader.GetValue (4));
								
							//else, store that as the latestPower 
							}else{
								latestPower = Convert.ToInt32 (reader.GetValue (4));
								
								//Since we have both readings, lets store the data
								//SqlInsertlocalhost(i+1, (DateTime)reader.GetValue (1), (latestPower-previousPower));
								SqlInsertReading(i+1, (DateTime)reader.GetValue (1), (latestPower-previousPower));
								
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
		
		//add a new dormInfo to the dormList
		public void addItem(int indexOfCSV, string Name, string trendLogString, int indexFrom)
    	{
            dormList.Add(new dormInfo(indexOfCSV, Name, trendLogString, indexFrom));
    	}
		
		public class dormInfo{
			
			public int indexOfCSV;
			public string Name;
			public string trendLogString;
			public int indexFrom;
			
			public dormInfo(int indexOfCSV, string Name, string trendLogString, int indexFrom){
				this.indexOfCSV = indexOfCSV;
				this.Name = Name;
				this.trendLogString = trendLogString;
				this.indexFrom = indexFrom;
			}
			
			//This will save the newly updated dormInfo to the "lastDormReading.csv"
			public void saveData(){
				using (System.IO.StreamWriter file = new System.IO.StreamWriter(@"lastDormReading.csv", true))
            	{
                	file.WriteLine(Name + "," + trendLogString + "," + indexFrom.ToString());
            	}  
			}
			
		}
		
		public void readCSV(){
			string fileName = "lastDormReading.csv";
				dormList = new List<dormInfo>();
				
				if ((fileName != null) && (!fileName.Equals(string.Empty)) && (File.Exists(fileName)))
        		{
            		try
            		{
                		List<string> rows = new List<string>();
                		StreamReader reader = new StreamReader(fileName, System.Text.Encoding.ASCII);
                		string record = reader.ReadLine();
                		int row_num = 0;

                		while (record != null)  //lets take all rows from the .csv file and store it
                		{
                        	rows.Add(record);     // lets parse the rows and prepare each element for storage
                    		record = reader.ReadLine();
                    		row_num++;
                		}

                		List<string[]> rowObjects = new List<string[]>();
						int indexOfCSV = 0;
						
                		foreach (string s in rows)
                		{
                    		string[] convertedRow = s.Split(new char[] { ',' });
                    		int num_elements = convertedRow.Length;
                    		if (num_elements == 3)              //only add it if there are 4 elements (in case there is something wrong with the row)
                    		{
								addItem (indexOfCSV, convertedRow[0], convertedRow[1], Convert.ToInt32(convertedRow[2]));
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
		
		public void saveCSV(){
			string path = @"lastDormReading.csv";
        	try 
        	{
				//Delete the old file
				if(File.Exists (path)){
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
			for(int i = 0; i < dormList.Count; i++){
				dormList[i].saveData();
			}
		}

        static void Main(string[] args)
        {
            new Program();
        }
    }
}
