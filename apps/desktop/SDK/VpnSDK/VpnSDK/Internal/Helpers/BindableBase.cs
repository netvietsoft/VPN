using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace VpnSDK.Internal.Helpers;

internal abstract class BindableBase : INotifyPropertyChanged
{
	private bool _selected;

	public bool IsSelected
	{
		get
		{
			return _selected;
		}
		set
		{
			SetProperty(ref _selected, value, "IsSelected");
		}
	}

	public event PropertyChangedEventHandler PropertyChanged;

	protected virtual bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
	{
		if (object.Equals(storage, value))
		{
			return false;
		}
		storage = value;
		OnPropertyChanged(propertyName);
		return true;
	}

	protected void OnPropertyChanged(string propertyName)
	{
		SDKCore.SyncContext.Post(delegate(object state)
		{
			PropertyChanged?.Invoke(state, new PropertyChangedEventArgs(propertyName));
		}, this);
	}
}
