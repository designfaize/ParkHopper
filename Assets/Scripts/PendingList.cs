using System.Collections.Generic;

namespace ParkHopper.Utils
{
	/// <summary>
	/// List implementation that allows delaying of modifications to the "master" list until ready.
	/// Ex: delay modification until after your done iterating through the list.
	/// The lock is not thread safe, this is not a concurrent data type.
	/// </summary>
	public class PendingList<T> : List<T>
	{
		private int _lockCounter = 0;
		private List<T> _pendingAdds = new List<T>();
		private List<T> _pendingRemoves = new List<T>();

		public new void Add(T item)
		{
			if (_lockCounter == 0)
			{ // we're not locked, so just add as usual
				base.Add(item);
				return;
			}

			// hide the normal add behavior by remembering what was requested to be added
			_pendingAdds.Add(item);
		}

		public new void Remove(T item)
		{
			if (_lockCounter == 0)
			{ // we're not locked, so just remove as usual
				base.Remove(item);
				return;
			}

			// hide the normal remove behavior by remembering what was requested to be removed
			_pendingRemoves.Add(item);
		}

		/// <summary>
		/// "Lock" the master list to hold off pending modifications until fully "unlocked".
		/// </summary>
		public void Lock()
		{
			++_lockCounter;
		}

		/// <summary>
		/// "Unlock" the master list to allow pending modifications if no longer "locked".
		/// </summary>
		public void Unlock()
		{
			if (--_lockCounter > 0)
				return; // still locked

			// fully unlocked, so resolve the pending adds/removes in the "master" list
			AddRange(_pendingAdds);
			RemoveAll(_pendingRemoves.Contains);

			_pendingAdds.Clear();
			_pendingRemoves.Clear();
		}
	}
}