using System;
using System.Data;
using MySql.Data.MySqlClient;
using System.Net.Mail;
using System.Collections.Generic;
using System.Net;
using PowerStormReadingGatherer;

namespace PowerStormReadingGatherer
{
	/// <summary>
	/// Cleans the errors in the meter data and notifies the administrator via email.
	/// </summary>
	public class DataCleaner
	{
		private MySqlConnection dbPowerstorm; 
		
		/// <summary>
		/// Initializes a new instance of the <see cref="PowerStormReadingGatherer.DataCleaner"/> class.
		/// </summary>
		/// <param name='dbPower'>
		/// Database connection
		/// </param>
		public DataCleaner (MySqlConnection dbPower)
		{
			dbPowerstorm = dbPower;
			
		}
		
		/// <summary>
		/// Finds outliers in the data, marks them in the database, and fixes outliers
		/// </summary>
		public void CleanData()
        {
			dbPowerstorm.Open ();
			//gets all of the rows with unmarked validity
            MySqlCommand command = new MySqlCommand("SELECT * FROM powerstorm_data.electricity_readings " + 
			                                        "WHERE validity IS NULL OR validity = '';", dbPowerstorm); 
			
            MySqlDataReader read = command.ExecuteReader();
			   
			List<Int32> rowMeterId = new List<Int32>();
			List<DateTime> rowDate = new List<DateTime>();
			while (read.Read())
			{
				rowMeterId.Add(Int32.Parse(read[Constants.POWERSTORM_DATABASE_METERID].ToString()));
				rowDate.Add(DateTime.Parse(read[Constants.POWERSTORM_DATABASE_DATETIME].ToString()));
			}
			
			read.Close();
		
			//determines if each unmarked value is an outlier (using 2 standard deviations)
			for (int i = 0; i < rowMeterId.Count; i++)
            {
                int meterId = rowMeterId[i];
                DateTime date = rowDate[i];
                MySqlCommand command2 = new MySqlCommand(@"SELECT id, date_time, meter_id, power " 
				                                        + "FROM electricity_readings "
														+ "WHERE power > ANY(SELECT (3 * STD(power) + AVG(power) + 2) AS UPPERLIMIT " 
														+ "FROM electricity_readings WHERE meter_id = @meter "
														+ "AND date_time BETWEEN @from AND  @to "
														+ "AND (validity = 'ACCEPTABLE' OR validity = 'CORRECTED')) "
														+ "AND date_time BETWEEN @from AND  @to "
														+ "AND (validity = '' OR validity IS NULL) "
														+ "AND  meter_id = @meter;", dbPowerstorm);
				
                command2.Parameters.Add(@"@meter", MySqlDbType.Int32);
                command2.Parameters.Add(@"@from", MySqlDbType.DateTime);
                command2.Parameters.Add(@"@to", MySqlDbType.DateTime);

                command2.Parameters["@meter"].Value = meterId;
                command2.Parameters["@to"].Value = date;
                command2.Parameters["@from"].Value = date.AddMinutes(Constants.LOOKBACK_TIME);
				
				MySqlDataReader reader = command2.ExecuteReader();
				//keeps track of the ids of the records that are outliers
				List<int> theIds = new List<int>();
				
				//stores a description of all the readings marked as alarmed
				List<string> description = new List<string>();
				
				while (reader.Read())
				{
					theIds.Add(Convert.ToInt32(reader[Constants.POWERSTORM_DATABASE_INDEX].ToString()));
					description.Add("Date: " + reader[Constants.POWERSTORM_DATABASE_DATETIME].ToString() + " Meter Id: " + reader[Constants.POWERSTORM_DATABASE_METERID].ToString() + " Power: " + reader[Constants.POWERSTORM_DATABASE_POWER].ToString());
				}
				reader.Close();
				
				MySqlCommand command3 = new MySqlCommand("",dbPowerstorm);
				command3.Parameters.Add(@"@theId", MySqlDbType.Int32);
				
                //marks all outliers as alarmed
				for (int k = 0; k < theIds.Count; k++)
				{
					command3.CommandText=@"UPDATE electricity_readings SET validity = 'ALARMED' "
										+"WHERE id = @theId;";
					
					command3.Parameters["@theId"].Value = theIds[k];

					command3.ExecuteNonQuery();  
					
					//sends email notifications for each outlier
					EmailNotification(description[k]);
					
				}
				
							
				//finds all values that are not standard deviations and marks them as acceptable
                command2.CommandText = @"SELECT id FROM electricity_readings "
										+"WHERE power <= ANY(SELECT (3 * STD(power) + AVG(power) + 2) AS UPPERLIMIT "
										+"FROM electricity_readings WHERE meter_id = @meter "
										+"AND date_time BETWEEN @from AND  @to "
										+"AND (validity = 'ACCEPTABLE' OR validity = 'CORRECTED')) "
										+"AND date_time BETWEEN @from AND  @to "
										+"AND (validity = '' OR validity IS NULL) "
										+"AND  meter_id = @meter;";
				
                MySqlDataReader reader2 = command2.ExecuteReader();
				
				theIds = new List<int>();
				
				//keeps track of the list of ids of records that are NOT outliers
				while (reader2.Read())
				{
					theIds.Add(Convert.ToInt32(reader2[Constants.POWERSTORM_DATABASE_INDEX].ToString()));
				}
				
				reader2.Close();
				
				MySqlCommand command4 = new MySqlCommand("",dbPowerstorm);
				command4.Parameters.Add(@"@theId", MySqlDbType.Int32);
				
				foreach (int theId in theIds)
				{
                    //mark non-outliers as acceptable
					command4.CommandText=@"UPDATE electricity_readings SET validity = 'ACCEPTABLE' "
											+ "WHERE id = @theId;";
					command4.Parameters["@theId"].Value = theId;

					command4.ExecuteNonQuery();   
				}

            }
			 
			//begins cleaning the data
			
			//gets all values marked as 'alarmed' in the database
            MySqlCommand command5 = new MySqlCommand("SELECT * FROM electricity_readings "
			                                         +"WHERE validity = 'ALARMED'", dbPowerstorm); 
            MySqlDataReader alarmedReader = command5.ExecuteReader();
			
			//keeps track of all ids that are alarmed
			List<int> idList = new List<int>();
			//keeps track of dates of all alarmed records
			List<DateTime> dates = new List<DateTime>();
			//keeps track of the meter id for each alarmed record
			List<int> meterIds = new List<int>();
			
			while(alarmedReader.Read())
			{
				meterIds.Add(Convert.ToInt32(alarmedReader[Constants.POWERSTORM_DATABASE_METERID].ToString()));
				idList.Add(Convert.ToInt32(alarmedReader[Constants.POWERSTORM_DATABASE_INDEX].ToString()));
				dates.Add(Convert.ToDateTime(alarmedReader[Constants.POWERSTORM_DATABASE_DATETIME].ToString()));
			}
			
			alarmedReader.Close();
			
			//cleans each alarmed value based on linear regression
			for (int i = 0; i < meterIds.Count; i++)
			{

                //gets the previous acceptable value
                command5 = new MySqlCommand("SELECT * FROM electricity_readings "
				                            +"WHERE date_time < @date AND meter_id = @meter AND validity = 'ACCEPTABLE' "
				                            +"ORDER BY date_time DESC LIMIT 0,1", dbPowerstorm);
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
                    datePrev = DateTime.Parse(alarmedReader[Constants.POWERSTORM_DATABASE_DATETIME].ToString()).Ticks;
                    powerPrev = Int32.Parse(alarmedReader[Constants.POWERSTORM_DATABASE_POWER].ToString());
                    hadPrev = true;
                }
				alarmedReader.Close();
				
                //gets the value closest to the alarmed value that is acceptable and most recent
                command5 = new MySqlCommand("SELECT * FROM electricity_readings "
				                            +"WHERE date_time > @date AND meter_id = @meter AND validity = 'ACCEPTABLE' "
				                            +"ORDER BY date_time ASC LIMIT 0,1", dbPowerstorm);
                command5.Parameters.Add(@"meter", MySqlDbType.Text);
				command5.Parameters.Add (@"date", MySqlDbType.DateTime);
                command5.Parameters["@meter"].Value = meterIds[i].ToString();
				command5.Parameters["@date"].Value = dates[i].ToString();

                alarmedReader = command5.ExecuteReader();

                double dateLast = 0;
                int powerLast = 0;
                bool hadLast = false;
                if (alarmedReader.Read())
                {
                    dateLast = DateTime.Parse(alarmedReader[Constants.POWERSTORM_DATABASE_DATETIME].ToString()).Ticks;

                    powerLast = Int32.Parse(alarmedReader[Constants.POWERSTORM_DATABASE_POWER].ToString());
                    hadLast = true;
                }
                alarmedReader.Close();

                if (hadLast && hadPrev)
                {
					//calculate what the value should be based on linear regression
                    	double currentTime = DateTime.Parse(dates[i].ToString()).Ticks;
                    	int id = Int32.Parse(idList[i].ToString());

                        double newPower = powerPrev + (powerLast - powerPrev) * ((currentTime - datePrev) / (dateLast - datePrev));

                        MySqlCommand updateCommand = new MySqlCommand(@"UPDATE electricity_readings "
					                                              +"SET power = @power, validity = 'CORRECTED' "
					                                              +"WHERE id = @id", dbPowerstorm);
                        updateCommand.Parameters.Add(@"@power", MySqlDbType.Int32);
                        updateCommand.Parameters.Add(@"@id", MySqlDbType.Int32);

                        updateCommand.Parameters["@power"].Value = newPower;
                        updateCommand.Parameters["@id"].Value = id;

                        updateCommand.ExecuteNonQuery();
                }
            }
        dbPowerstorm.Close ();   
        }

		/// <summary>
		/// Emails the notification that outliers have been detected.
		/// </summary>
		/// <param name='msg'>
		/// Information to include in the email
		/// </param>
		private void EmailNotification(string msg)
		{
			try
			{
				MailMessage mail = new MailMessage();
				mail.From = new MailAddress("whitworthpowerstorm@gmail.com");
				
				//set up gmail smtp client to send mail
				SmtpClient smtp = new SmtpClient();
				smtp.Port = 587;
				smtp.EnableSsl = true;
				smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
				smtp.UseDefaultCredentials = false;
				smtp.Credentials = new NetworkCredential(mail.From.ToString(), "p0werst0rm");
				smtp.Host = "smtp.gmail.com";
				
				mail.To.Add(new MailAddress("whitworthpowerstorm@gmail.com"));
				mail.IsBodyHtml = true;
				mail.Body = msg;
				mail.Subject = "Powerstorm Alert";
				smtp.Send(mail);
				            
			}
			catch (Exception ex)
			{
				Console.WriteLine("Email message could not be sent! {0}", ex.ToString());
			}
		}
	}
}

