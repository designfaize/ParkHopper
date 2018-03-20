using System;
using System.Collections.Generic;
using System.Linq;
using ParkHopper.Utils;
using ParkHopper.Events;
using UnityEngine;



namespace ParkHopper.Events
{
	/// Allows listeners to register for IEvent notifications
	public static class EventDispatcher
	{
		// TODO: ADT usage, are we using the best containers?
		//         ListenerCollection
		//             HashSet - appropriate since we don't want duplicates, but we can't control the order.
		//             List - what we have now, add ability to set initial capacity?
		//            
		//         Scopes
		//             Can we enforce the caller to use a dense enum?  If so, we can use an Array/List
		//         
		//         Event => Listeners map
		//             This can be a list/array if we can enforce the hash to a dense enumeration.  
		//             That might be possible if we have IEvent require the child class to define the hash
		//             as an enumeration, or give an Api for the EventDispatcher to tell an event class what
		//             its id is.           
		//
		// TODO: Should we keep the use of PendingList?  It might lead to unfulfilled expectations, where
		//         somebody adds/removes a listener but doesn't get the event they expect to get.
		//       
		// TODO: Add UnregisterEvent / RemoveScope functions?  This runs into the same problem that PendingList
		//         is trying to solve, though.  We'd to apply that solution or be careful to make sure Unreg/Remove is safe
		//         during Dispatch.
		// TODO: Can we avoid the listeners needing to cast to a derived type?

#region Internal Data

		// The collection we use to store listeners.  This extra layer is here to let us change
		// the underlying data type here if needed, it's effectively a typedef.
		private class ListenerCollection : PendingList<EventListener> {}

		private class Scope
		{
			// event => EventListeners registered for that event
			// The event is represented by the hash code of it's type, typeof(event).GetHashCode()
			//_eventListeners[e.GetType().GetHashCode()] is a list of functions to dispatch
			public Dictionary<int, ListenerCollection > listenerMap = new Dictionary<int, ListenerCollection>();
		}

		// scope ID => scope
		// NOTE: We use a dictionary to let the user use any id scheme they want.  An enum
		// is convenient and if we can enforce then we can use an array or list here.
		private static Dictionary<int, Scope> s_scopes = new Dictionary<int, Scope>();

#endregion


#region Internal Helpers

		// We often use the type and id together, this packages the two.
		private struct EventMetaData
		{
			public Type type;
			public int id;

			public EventMetaData( IEvent evt )
			{	
				type = evt.GetType();
				id = type.GetHashCode();
			}

			public EventMetaData( Type evtType )
			{
				type = evtType;
				id = type.GetHashCode();
			}

			public static EventMetaData Create<EVENT>() where EVENT : IEvent
			{
				return new EventMetaData(typeof(EVENT));
			}

			public override string ToString()
			{
				return type.ToString() + ":" + id;
			}
		}
		

		// returns a reference to the scope, logging an error if it's not found.
		private static Scope FindScope( int id, string operation)
		{
			Scope scope = null;
			s_scopes.TryGetValue( id, out scope );
			if( null == scope )
			{
				Debug.Log( "Scope id [" + id + "] doesn't exist during operation [" + operation + "]" );
				return null;
			}
			return scope;
		}

		// returns the ListenerCollection for a given event and scope, logging an error if not found.
		private static ListenerCollection FindListeners( int scopeId, EventMetaData evtMd, string operation )
		{
			Scope scope = FindScope( scopeId, operation );
			if( null == scope ) return null;

			ListenerCollection listeners = null;
			if( scope.listenerMap.TryGetValue( evtMd.id, out listeners ) )
			{
				return listeners;
			}
			else
			{
				Debug.Log( "Event [" + evtMd.ToString() + "] is not registered for scope [" + scopeId + "] during operation [" + operation + "]" );
				return null;
			}
		}

#endregion


#region Public Interface
		
		/// Other classes can register for events using this function signature
		public delegate void EventListener(IEvent e);

		/// The scope id used when no id is specified.  Pass this to CreateScope and RegisterEvents if you don't
		/// want to use multiple scopes.
		public const int DEFAULT_SCOPE_ID = 0;

		/// Create an event dispatcher scope.  A scope is a level of categorization
		/// to control which listeners get notified of an event.
		/// 
		/// \param id  a numerical identifier for this scope that you can use later when
		///            registering listeners and dispatching events.
		///            An enumeration can be handy to use for this value.  See EventDispatcher::DEFAULT_SCOPE_ID.
		/// \param eventsToRegister an optional array of events to register with this scope.
		public static void CreateScope( int scopeId, params Type[] eventsToRegister )
		{
			if( !s_scopes.ContainsKey( scopeId ) )
			{
				Debug.Log ("Creating scope for " + scopeId);
				s_scopes.Add( scopeId, new Scope() );
				RegisterEvents( scopeId, eventsToRegister );
			}
			else
			{
				Debug.LogError( "Event scope with id [" + scopeId + "] already exists.  " 
				                 + "This subsequent creation is ignored, and events will not be registered."
				                 );
			}
		}

		/// Give this EventDispatcher events to manage.
		/// For calls that require an event type, making those calls with an unregistered
		/// type is an error.
		// NOTE: There is no UnregisterEvents function.  Adding one would require coordinating
		// with the Dispatch pump since the intent would likely be to stop all future dispatches.
		public static void RegisterEvents(int scopeId, Type[] eventsToRegister)
		{
			Scope scope = FindScope( scopeId, "RegisterEvents" );
			if( null == scope ) return;


			for( int i = 0; i < eventsToRegister.Length; ++i )
			{
				var md = new EventMetaData(eventsToRegister[i]);
				
				if( !scope.listenerMap.ContainsKey( md.id ) )
				{
					scope.listenerMap.Add( md.id, new ListenerCollection() );
				}
				else
				{
					Debug.LogWarning( "Registering event type ["+ md + "] "
					                   + "that has already been registered for scope id [" + scopeId + "].  Ignoring." 
					                   );
					continue;
				}
			}
		}

		/// Send this event to every object that has registered to listen for evt's eventType
		public static void DispatchEvent(IEvent evt, int scopeId = DEFAULT_SCOPE_ID)
		{
			var md = new EventMetaData( evt );
			ListenerCollection listeners = FindListeners( scopeId, md, "DispatchEvent" );
			if( null == listeners ) return;


			// delay adds/removes during a dispatch in order to prevent modifying the list of event listeners while iterating over them
			listeners.Lock();
			{
				for (int i = 0; i < listeners.Count; ++i)
				{
					try
					{
						listeners[i](evt);
					}
					catch( System.Exception ex )
					{
						Debug.LogError("Exception: " + ex.ToString() + Environment.NewLine
						                 + "Exception raised when processing event type [" + md + "] in scope [" + scopeId + "]." + Environment.NewLine
										 + "Event data: " + evt.ToString()
										 );
					}
				}
			}
			// we're done dispatching everything, so we can release our lock now
			listeners.Unlock();
		}

		/**
		\brief Register a function to receive a callback every time an event of the given type is dispatched
		EVENT is the Type of the IEvent subclass you are listening for.

		\param listener The callback function
		*/
		public static void AddListener<EVENT>(EventListener listener, int scopeId = DEFAULT_SCOPE_ID ) where EVENT : IEvent
		{
			var md = EventMetaData.Create<EVENT>();
			PendingList<EventListener> listeners = FindListeners( scopeId, md, "AddEventListener<" + md.type + ">");
			if( null == listeners ) return;


			if (!listeners.Contains(listener))
			{
				listeners.Add(listener);
			}
			else
			{
				Debug.LogWarning( "Double add of listener ["+ listener.ToString() + "] for event [" + md + "], ignoring." );
			}
		}

		/**
		\brief Removes previously registered event listener

		\see addEventListener
		*/
		public static void RemoveListener<EVENT>(EventListener listener, int scopeId = DEFAULT_SCOPE_ID) where EVENT : IEvent
		{
			// REVIEW: If the scope/event wasn't registered, should we have FindListeners
			// issue a warning intead of an error?
			var md = EventMetaData.Create<EVENT>();
			PendingList<EventListener> listeners = FindListeners( scopeId, md, "RemoveEventListener<" + md.type + ">");
			if( null == listeners ) return;

			listeners.Remove(listener);
		}


		/**
		\brief See if any objects have requested to listen for events of type EVENT

		\see addEventListener
		*/
		public static bool HasListener<EVENT>(int scopeId = DEFAULT_SCOPE_ID) where EVENT : IEvent
		{
			var md = EventMetaData.Create<EVENT>();
			PendingList<EventListener> listeners = FindListeners( scopeId, md, "HasEventListener<" + md.type + ">");

			return null != listeners && listeners.Count > 0;
		}
	}
	#endregion
}
