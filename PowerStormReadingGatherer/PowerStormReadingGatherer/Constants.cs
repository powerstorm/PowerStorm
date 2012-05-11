using System;

namespace PowerStormReadingGatherer
{
	public static class Constants
	{
	 	public const int POWERSTORM_DATABASE_INDEX = 0;
		public const int POWERSTORM_DATABASE_DATETIME = 1;
		public const int POWERSTORM_DATABASE_METERID = 2;
		public const int POWERSTORM_DATABASE_POWER = 3;
		public const int POWERSTORM_DATABASE_CREATEDAT = 4;
		public const int POWERSTORM_DATABASE_UPDATEDAT = 5;
		public const int POWERSTORM_DATABASE_VALIDITY = 6;
		
		public const int LOOKBACK_TIME = -1 * 90; //Amount of time we look back for the Standard Deviation
		public const int READER_GATHERER_SLEEP = 1000 * 60 * 5;// 1000 miliseconds, 60 second, 5 minutes
	}
}

