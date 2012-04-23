using System;
using MySql.Data.MySqlClient;
using System.Net.Mail;

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
			ErrorSanitizer();
			EmailNotification();
		}
		
		/// <summary>
		/// Cleans errors in the data
		/// </summary>
		private void ErrorSanitizer()
		{
			//find each record marked as an outlier, and run linear regression to fix the outlier
			
			//call ErrorFrequency for each error
			
			
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

