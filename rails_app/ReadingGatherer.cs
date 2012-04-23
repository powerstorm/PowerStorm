using System;
using System.Collections.Generic;
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
		
		// Initializes the program and begins periodically checking for new readings
        Program()
        {
			//Read from the file which persistantly stores the time of last update
            timeOfLastUpdate = DateTime.Parse(File.ReadAllText(@"myFile.txt"));
			
			//Get windows login credintials for authenticating with SQL server
			PromptForAuthentication();
		
			//Begin checking to see if updates are necessary every five minutes
			while(true) {
				FetchReadings();
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
		// Inserts an electricity reading into the powerstorm MySql database with the given attributes
		void SqlInsertReading(int meterId, DateTime timeOfReading, double reading) {
			String q = "INSERT INTO electricity_readings (meter_id, date_time, power) VALUES ("
				+ meterId
				+ ", CAST('" + timeOfReading.ToString("yyyy-MM-dd HH:mm:ss") + "' AS DATETIME), "
				+ reading + ");";
			
			new MySqlCommand(q, dbPower).ExecuteNonQuery();
			
			//Console.WriteLine(meterId + ",   " + timeOfReading + ",   " + reading);
		}
		// Prompts the user for windows login credintials for authenticating with SQL server
		void PromptForAuthentication() {
			bool sentinel = true;
			
			while (sentinel)
			{
				Console.Write("Username: ");
				winUserId = Console.ReadLine();
				
				Console.Write("Password: ");
				winPass = Console.ReadLine();
				
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
		// Retrieves each new reading from the Envision database and inserts it into the Powerstorm database
		void FetchReadings()
        {
            try
            {
				// Establish connections to the required databases
				ConnectPowerStorm();
				ConnectEnvision();
				
				// 'Now' is set to two years ago because we are using old data
				now = DateTime.Now.Subtract(new TimeSpan(365, 0, 0, 0));
				
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
				
				dbPower.Close();
				dbEnvision.Close();
				
				timeOfLastUpdate = now;
	
	            StreamWriter sw = new StreamWriter(@"myFile.txt");
	            sw.WriteLine(now.ToString());
	            sw.Close();
			}
            catch (Exception e)
            {
                Console.WriteLine("SQL Error: " + e.Message);
            }

        }

        static void Main(string[] args)
        {
            new Program();
        }
    }
}