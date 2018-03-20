using System;
using System.Collections.Generic;

namespace ParkHopper
{
	public class Rides
	{
		public static Dictionary<Ride, string> rideLookupDictionary;
		public enum Ride
		{
			ThunderMountain,
			SplashMountain,
			SpaceMountain,
			Empty
		}
		static Rides()
		{
			rideLookupDictionary = new Dictionary<Ride, string> ();
			rideLookupDictionary.Add (Ride.ThunderMountain, "Big Thunder Mountain");
			rideLookupDictionary.Add (Ride.SplashMountain, "Splash Mountain");
			rideLookupDictionary.Add (Ride.SpaceMountain, "S[ace Mountain");
		}
	}
}

