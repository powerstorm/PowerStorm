using System;
using System.Data;
using MySql.Data.MySqlClient;
using System.Net.Mail;
using System.Collections.Generic;

namespace PowerStormReadingGatherer
{
	/// <summary>
	/// Cleans the errors in the meter data and notifies the administrator via email.
	/// </summary>
	public class DataCleaner
	{
		private int numberOfErrors = 0;
		private MySqlConnection dbPowerstorm; 
		public DataCleaner (MySqlConnection dbPower)
		{
			dbPowerstorm = dbPower;
			
		}
		
		public void CleanData()
        {
            //BEGINS BY GRABBING ALL UNMARKED VALDITIY ROWS
            MySqlCommand command = new MySqlCommand("SELECT * FROM powerstorm_data.electricity_readings WHERE validity IS NULL OR validity = ''",dbPowerstorm); 
            MySqlDataReader read = command.ExecuteReader();
			
   
			List<Int32> rowMeterId = new List<Int32>();
			List<DateTime> rowDate = new List<DateTime>();
			while (read.Read())
			{
				rowMeterId.Add(Int32.Parse(read[2].ToString()));
				rowDate.Add(DateTime.Parse(read[1].ToString()));
			}
			
			read.Close();
            //foreach (DataRow myRow in schemaTable.Rows)
			for (int i = 0; i < rowMeterId.Count; i++)
            {
				//THEN FOR THOSE IT FINDS ALL THE ALARMED ONES
                int meterId = rowMeterId[i];//SEQUENCE = ID ?
                DateTime date = rowDate[i];
                MySqlCommand command2 = new MySqlCommand(@"SELECT id FROM electricity_readings 
WHERE power > ANY(SELECT (2 * STD(power) + AVG(power) + 2) AS UPPERLIMIT 
FROM electricity_readings WHERE meter_id = @meter 
AND date_time BETWEEN @from AND  @to 
AND (validity = 'ACCEPTABLE' OR validity = 'CORRECTED')) 
AND date_time BETWEEN @from AND  @to 
AND (validity = '' OR validity IS NULL)
AND  meter_id = @meter", dbPowerstorm);
                command2.Parameters.Add(@"@meter", MySqlDbType.Int32);//CHECK TYPES
                command2.Parameters.Add(@"@from", MySqlDbType.DateTime);
                command2.Parameters.Add(@"@to", MySqlDbType.DateTime);

                command2.Parameters["@meter"].Value = meterId;
                command2.Parameters["@to"].Value = date;
                command2.Parameters["@from"].Value = date.AddMinutes(-90);
				
				MySqlDataReader reader = command2.ExecuteReader();
				List<int> theIds = new List<int>();
				
				while (reader.Read())
				{
					theIds.Add(Convert.ToInt32(reader[0].ToString()));
				}
				reader.Close();
				MySqlCommand command3 = new MySqlCommand("",dbPowerstorm);
				command3.Parameters.Add(@"@theId", MySqlDbType.Int32);
                //THEN IT NOTES THEM AS ALARMED
				foreach (int theId in theIds)
				{
					command3.CommandText=@"UPDATE electricity_readings SET validity = 'ALARMED' WHERE id = @theId";
					command3.Parameters["@theId"].Value = theId;

					command3.ExecuteNonQuery();   
				}
				
							
				//FINDS THE OK ONES
                command2.CommandText = @"SELECT id FROM electricity_readings 
WHERE power <= ANY(SELECT (2 * STD(power) + AVG(power) + 2) AS UPPERLIMIT 
FROM electricity_readings WHERE meter_id = @meter 
AND date_time BETWEEN @from AND  @to 
AND (validity = 'ACCEPTABLE' OR validity = 'CORRECTED')) 
AND date_time BETWEEN @from AND  @to 
AND (validity = '' OR validity IS NULL)
AND  meter_id = @meter";
                MySqlDataReader reader2 = command2.ExecuteReader();
				theIds = new List<int>();
				while (reader2.Read())
				{
					theIds.Add(Convert.ToInt32(reader2[0].ToString()));
				}
				reader2.Close();
				MySqlCommand command4 = new MySqlCommand("",dbPowerstorm);
				command4.Parameters.Add(@"@theId", MySqlDbType.Int32);
				foreach (int theId in theIds)
				{
                    //MARKS THEM AS OK
					command4.CommandText=@"UPDATE electricity_readings SET validity = 'ACCEPTABLE' WHERE id = @theId";
					command4.Parameters["@theId"].Value = theId;

					command4.ExecuteNonQuery();   
				}

            }
			 
			//BEGINS CLEANING THE DATA BY GRABBING ALL ALARMED ONES 
            
            MySqlCommand command5 = new MySqlCommand("SELECT * FROM electricity_readings WHERE validity = 'ALARMED'", dbPowerstorm); 
            MySqlDataReader alarmedReader = command5.ExecuteReader();

         //   schemaTable = read.GetSchemaTable();
			List<int> idList = new List<int>();
			List<DateTime> dates = new List<DateTime>();
			List<int> meterIds = new List<int>();
			
			while(alarmedReader.Read())
			{
				meterIds.Add(Convert.ToInt32(alarmedReader[2].ToString()));
				idList.Add(Convert.ToInt32(alarmedReader[0].ToString()));
				dates.Add(Convert.ToDateTime(alarmedReader[1].ToString()));
			}
			
			//Object[] rows = new Object[alarmedReader.FieldCount];
    		//int numRows = alarmedReader.GetValues(rows);
			alarmedReader.Close();
			
			for (int i = 0; i < meterIds.Count; i++)
			{

                //Previous
                command5 = new MySqlCommand("SELECT * FROM electricity_readings WHERE date_time < @date AND meter_id = @meter AND validity = 'ACCEPTABLE' ORDER BY date_time DESC LIMIT 0,1", dbPowerstorm);
                command5.Parameters.Add(@"meter", MySqlDbType.Text);
				command5.Parameters.Add (@"date", MySqlDbType.DateTime);
                command5.Parameters["@meter"].Value = meterIds[i].ToString();
				command5.Parameters["@date"].Value = dates[i].ToString();
				
                alarmedReader = command5.ExecuteReader();

                bool hadPrev = false;
                double datePrev = 0;
                int powerPrev = 0;
                if (alarmedReader.Read())
                {
                    //  DataTable data = reader.GetSchemaTable();

                    // DataRow trow = data.Rows[0];
                    //	Object trow = theRow[0];                         
                    //   long datePrev = long.Parse(trow[1].ToString());
                    // int powerPrev = Int32.Parse(trow[4].ToString());

                    //double datePrev = Convert.ToDouble(alarmedReader[1].ToString());
                    //	string datePrv = alarmedReader[1].ToString();
                    //	DateTime dtPrev = Convert.ToDateTime(datePrv);
                    //	double datePrev = Convert.ToDouble(dtPrev);
                    datePrev = DateTime.Parse(alarmedReader[1].ToString()).Ticks;
                    //    long datePrev = long.Parse(alarmedReader[1].ToString());
                    powerPrev = Int32.Parse(alarmedReader[3].ToString());
                    hadPrev = true;
                }
				alarmedReader.Close();
				
                //Recent
                command5 = new MySqlCommand("SELECT * FROM electricity_readings WHERE date_time > @date AND meter_id = @meter AND validity = 'ACCEPTABLE' ORDER BY date_time ASC LIMIT 0,1", dbPowerstorm);
                command5.Parameters.Add(@"meter", MySqlDbType.Text);
				command5.Parameters.Add (@"date", MySqlDbType.DateTime);
                command5.Parameters["@meter"].Value = meterIds[i].ToString();
				command5.Parameters["@date"].Value = dates[i].ToString();

                alarmedReader = command5.ExecuteReader();
             //   DataTable data = reader.GetSchemaTable();
               // DataRow trow = data.Rows[0];

                double dateLast = 0;
                int powerLast = 0;
                bool hadLast = false;
                if (alarmedReader.Read())
                {

                    //	double dateLast = Convert.ToDouble(alarmedReader[1].ToString());
                    dateLast = DateTime.Parse(alarmedReader[1].ToString()).Ticks;
                    //	long dateLast = long.Parse(alarmedReader[1].ToString());
                    powerLast = Int32.Parse(alarmedReader[3].ToString());
                    hadLast = true;
                }
                    alarmedReader.Close();
               // long dateLast = long.Parse(trow[1].ToString());
                //int powerLast = Int32.Parse(trow[4].ToString()); ;

                    if (hadLast && hadPrev)
                    {
                        double currentTime = DateTime.Parse(dates[i].ToString()).Ticks;
                        int id = Int32.Parse(idList[i].ToString());

                        double newPower = powerPrev + (powerLast - powerPrev) * ((currentTime - datePrev) / (dateLast - datePrev));
                        //CHECK IT IS CALLED ID next INSERT
                        //update with newPower
                        MySqlCommand updateCommand = new MySqlCommand(@"UPDATE electricity_readings SET power = @power, validity = 'CORRECTED' WHERE id = @id", dbPowerstorm);
                        updateCommand.Parameters.Add(@"@power", MySqlDbType.Int32);
                        updateCommand.Parameters.Add(@"@id", MySqlDbType.Int32);

                        updateCommand.Parameters["@power"].Value = newPower;
                        updateCommand.Parameters["@id"].Value = id;

                        updateCommand.ExecuteNonQuery();
                    }
			 
			// updateCommand = new MySqlCommand(@"UPDATE electricity_readings SET power = @power, validity = 'CORRECTED' WHERE id = @id", dbPowerstorm);
			 	//updateCommand = new MySqlCommand(@"UPDATE electricity_readings SET power = @power, validity = 'CORRECTED' WHERE id = @id", dbPowerstorm);
				//updateCommand.ExecuteNonQuery();

            }
           
        }
		/// <summary>
		/// Cleans errors in the data
		/// </summary>
		private void ErrorSanitizer()
		{
			//find each record marked as an outlier, and run linear regression to fix the outlier
			
			//call ErrorFrequency for each error
			
			//string validation_state
			//unmarked ""
			//corrected
			//alarmed
			//acceptable
			
			//don't display graph if alarmed or ""
			
			//rails server
			//localhost:3000
			
		}
		
		/// <summary>
		/// Updates the error count
		/// </summary>
		private void ErrorFrequency()
		{
			numberOfErrors++;
		}
		
		/// <summary>
		/// Emails the notification that errors have occurred and been cleaned.
		/// </summary>
		private void EmailNotification()
		{
			MailMessage message = new MailMessage("whitworthpowerstorm@gmail.com", "whitworthpowerstorm@gmail.com", "Meter error", "Outliers found in ___");
			SmtpClient client = new SmtpClient("smtp.gmail.com");
			client.UseDefaultCredentials = true;
			try
			{
				client.Send(message);
			}
			catch (Exception ex)
			{
				Console.WriteLine("Email message could not be sent! {0}", ex.ToString());
			}
		}
	}
}

