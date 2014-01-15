using System;
using System.ComponentModel;
using System.Diagnostics;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.Controls;

namespace CDuke.LastWorkItem.Base
{
	/// <summary> 
	/// Team Explorer plugin common base class. 
	/// </summary> 
	public class TeamExplorerBase : IDisposable, INotifyPropertyChanged
	{
		#region Members

		private bool _contextSubscribed = false;

		#endregion

		/// <summary> 
		/// Get/set the service provider. 
		/// </summary> 
		public IServiceProvider ServiceProvider
		{
			get { return _serviceProvider; }
			set
			{
				// Unsubscribe from Team Foundation context changes 
				if (_serviceProvider != null)
				{
					UnsubscribeContextChanges();
				}

				_serviceProvider = value;

				// Subscribe to Team Foundation context changes 
				if (_serviceProvider != null)
				{
					SubscribeContextChanges();
				}
			}
		}
		private IServiceProvider _serviceProvider = null;

		/// <summary> 
		/// Get the requested service from the service provider. 
		/// </summary> 
		public T GetService<T>()
		{
			Debug.Assert(this.ServiceProvider != null, "GetService<T> called before service provider is set");
			if (this.ServiceProvider != null)
			{
				return (T)this.ServiceProvider.GetService(typeof(T));
			}

			return default(T);
		}

		/// <summary> 
		/// Show a notification in the Team Explorer window. 
		/// </summary> 
		protected Guid ShowNotification(string message, NotificationType type)
		{
			var teamExplorer = GetService<ITeamExplorer>();
			if (teamExplorer != null)
			{
				var guid = Guid.NewGuid();
				teamExplorer.ShowNotification(message, type, NotificationFlags.None, null, guid);
				return guid;
			}

			return Guid.Empty;
		}

		#region IDisposable

		/// <summary> 
		/// Dispose. 
		/// </summary> 
		public virtual void Dispose()
		{
			UnsubscribeContextChanges();
		}

		#endregion

		#region INotifyPropertyChanged

		public event PropertyChangedEventHandler PropertyChanged;

		/// <summary> 
		/// Raise the PropertyChanged event for the specified property. 
		/// </summary> 
		/// <param name="propertyName">Property name</param> 
		protected void RaisePropertyChanged(string propertyName)
		{
			if (this.PropertyChanged != null)
			{
				this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}

		#endregion

		#region Team Foundation Context

		/// <summary> 
		/// Subscribe to context changes. 
		/// </summary> 
		protected void SubscribeContextChanges()
		{
			Debug.Assert(this.ServiceProvider != null, "ServiceProvider must be set before subscribing to context changes");
			if (this.ServiceProvider == null || _contextSubscribed)
			{
				return;
			}

			var tfContextManager = GetService<ITeamFoundationContextManager>();
			if (tfContextManager != null)
			{
				tfContextManager.ContextChanged += ContextChanged;
				_contextSubscribed = true;
			}
		}

		/// <summary> 
		/// Unsubscribe from context changes. 
		/// </summary> 
		protected void UnsubscribeContextChanges()
		{
			if (this.ServiceProvider == null || !_contextSubscribed)
			{
				return;
			}

			var tfContextManager = GetService<ITeamFoundationContextManager>();
			if (tfContextManager != null)
			{
				tfContextManager.ContextChanged -= ContextChanged;
			}
		}

		/// <summary> 
		/// ContextChanged event handler. 
		/// </summary> 
		protected virtual void ContextChanged(object sender, ContextChangedEventArgs e)
		{
		}

		/// <summary> 
		/// Get the current Team Foundation context. 
		/// </summary> 
		protected ITeamFoundationContext CurrentContext
		{
			get
			{
				var tfContextManager = GetService<ITeamFoundationContextManager>();
				if (tfContextManager != null)
				{
					return tfContextManager.CurrentContext;
				}

				return null;
			}
		}

		#endregion
	}
}