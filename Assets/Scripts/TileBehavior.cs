using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using ParkHopper;


namespace ParkHopper
{
	public class TileBehavior : MonoBehaviour {
		public static Dictionary<Ride, string> rideLookupDictionary;
		public enum Ride
		{
			ThunderMountain,
			SplashMountain,
			SpaceMountain,
			SpaceMountain1,
			SpaceMountain2,
			Empty
		}
		static TileBehavior()
		{
			rideLookupDictionary = new Dictionary<Ride, string> ();
			rideLookupDictionary.Add (Ride.ThunderMountain, "Big Thunder Mountain");
			rideLookupDictionary.Add (Ride.SplashMountain, "Splash Mountain");
			rideLookupDictionary.Add (Ride.SpaceMountain, "Space Mountain");
			rideLookupDictionary.Add (Ride.SpaceMountain1, "Space Mountain");
			rideLookupDictionary.Add (Ride.SpaceMountain2, "Space Mountain");
		}
		public enum TileType
		{
			Pay,
			FastPass,
			Get,
			ForceMove,
			Lose,
			RollAgain,
			BuySorcererCard,
			VilliansMove,
			VilliansLoseCash,
			Empty
		}
		public TileType tile;
		public int value;
		public Ride ride = Ride.Empty;
		public TextMeshPro textMesh;
		void Start()
		{
			if (textMesh != null)
				textMesh.SetText(value.ToString ());
		}
	}
}