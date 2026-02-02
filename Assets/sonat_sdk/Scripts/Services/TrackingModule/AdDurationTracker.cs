using System;

namespace Sonat.TrackingModule
{
	public class AdDurationTracker
	{
		// Singleton instance 
		private static readonly AdDurationTracker instance = new
			AdDurationTracker();
		// Private constructor to prevent multiple instances 
		private AdDurationTracker() { }
		// Method to get the singleton instance 
		public static AdDurationTracker Instance
		{
			get { return instance; }
		}
		// Variables to store various time points 
		private double leaveDuration = 0;
		private int numberOfClicks = 0;
		private DateTime? adShowedTime;
		private DateTime? adDismissedTime;
		private int? adClickedTime;
		private int? adBounceBackTime;
		private DateTime? userLeaveTime;
		private DateTime? userReturnTime;
		// Method to be called when the ad is shown 
		public void OnAdShowedFullScreenContent()
		{
			// Reset all values 
			leaveDuration = 0;
			numberOfClicks = 0;
			adShowedTime = DateTime.Now;
			adDismissedTime = null;
			adClickedTime = null;
			adBounceBackTime = null;
			userLeaveTime = null;
			userReturnTime = null;
		}
		// Method to be called when the user clicks the ad
		public void OnUserClickedAd()
		{
			numberOfClicks = numberOfClicks + 1;
			if (adClickedTime > 0) return;
			if (adShowedTime == null) return;
			DateTime userClickedTime = DateTime.Now;
			TimeSpan duration = userClickedTime - adShowedTime.Value; adClickedTime = (int)Math.Ceiling(duration.TotalSeconds);
		}
		// Method to be called when the user leaves the app
		public void OnUserLeavesApp()
		{
			userLeaveTime = DateTime.Now;
		}
		// Method to be called when the user returns to the app
		public void OnUserReturnsToApp()
		{
			if (userLeaveTime == null) return;
			userReturnTime = DateTime.Now;
			TimeSpan duration = userReturnTime.Value - userLeaveTime.Value; leaveDuration += duration.TotalSeconds;
			adBounceBackTime = (int)duration.TotalSeconds;
		}
		// Method to be called when the ad is dismissed 
		public AdMetrics OnAdDismissedFullScreenContent()
		{
			if (adShowedTime == null) return new AdMetrics(0, 0, null, null); adDismissedTime = DateTime.Now;
			TimeSpan duration = adDismissedTime.Value - adShowedTime.Value; int adDuration = (int)Math.Round(duration.TotalSeconds -
				leaveDuration);
			return new AdMetrics(adDuration, numberOfClicks, adClickedTime, adBounceBackTime);
		}
		// Inner class to hold the metrics 
		public class AdMetrics
		{
			public int AdDuration { get; private set; }
			public int AdClickCount { get; private set; }
			public int? AdClickTime { get; private set; }
			public int? AdBounceBackTime { get; private set; }
			public AdMetrics(int adDuration, int adClickCount, int? adClickTime, int? adBounceBackTime)
			{
				AdDuration = adDuration;
				AdClickCount = adClickCount;
				AdClickTime = adClickTime;
				AdBounceBackTime = adBounceBackTime;
			}
		}
	}
}
