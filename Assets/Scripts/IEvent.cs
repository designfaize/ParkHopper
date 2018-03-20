using System;
using System.Reflection;

namespace ParkHopper.Events
{
	/// An event to be broadcast through the EventDispatcher.
	/// Listeners register for a particular event using its type, so you differentiate events
	/// by creating a new type that inherits from IEvent.
	///
	/// You can add members to your derived class to serve as a payload for listeners to work with.
	public interface IEvent
	{
	}
		

	public static class EventExtenstions
	{
		/**
		\brief Uses reflection to print all public fields of an IEvent.

		Use for debugging only.  Reflection is too slow to use in game code.
		*/
		// TODO: I'm not sure this works, I think this might conflict with Object.ToString().
		public static string ToString(this IEvent self)
		{
			Type type = self.GetType();
			FieldInfo[] info = type.GetFields(BindingFlags.Public | BindingFlags.Instance);
			System.Text.StringBuilder str = new System.Text.StringBuilder();
			str.AppendFormat("{0}: ", type.Name);
			foreach (FieldInfo i in info)
			{
				//Concats all public values of this instance of IEvent.
				str.AppendFormat("{0}={1}, ", i.Name, i.GetValue(self));
			}
			return str.ToString();
		}
	}
}
