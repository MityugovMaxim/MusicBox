using System;
using System.Threading.Tasks;
using Firebase.Database;

public abstract class DataObject<TValue>
{
	public bool Loaded { get; private set; }

	protected abstract string Path { get; }

	public TValue Value
	{
		get => m_Value;
		protected set
		{
			if (Equals(m_Value, value))
				return;
			
			m_Value = value;
			
			m_ChangeAction?.Invoke(m_Value);
		}
	}

	TValue m_Value;

	DatabaseReference m_Data;

	readonly DynamicDelegate<TValue> m_ChangeAction = new DynamicDelegate<TValue>();

	public async Task Load()
	{
		if (Loaded)
			return;
		
		if (m_Data == null)
		{
			m_Data              =  FirebaseDatabase.DefaultInstance.RootReference.Child(Path);
			m_Data.ValueChanged += OnDataChange;
		}
		
		await Fetch();
		
		Loaded = true;
	}

	public Task Reload()
	{
		Unload();
		
		return Load();
	}

	public void Unload()
	{
		if (!Loaded)
			return;
		
		if (m_Data != null)
		{
			m_Data.ValueChanged -= OnDataChange;
			m_Data              =  null;
		}
		
		Loaded = false;
	}

	public void Subscribe(Action _Action) => m_ChangeAction.AddListener(_Action);

	public void Subscribe(Action<TValue> _Action) => m_ChangeAction.AddListener(_Action);

	public void Unsubscribe(Action _Action) => m_ChangeAction.RemoveListener(_Action);

	public void Unsubscribe(Action<TValue> _Action) => m_ChangeAction.RemoveListener(_Action);

	protected abstract TValue Create(DataSnapshot _Data);

	async Task Fetch()
	{
		Value = default;
		
		DataSnapshot snapshot = await m_Data.GetValueAsync(15000, 2);
		
		if (snapshot == null)
			return;
		
		Value = Create(snapshot);
	}

	void OnDataChange(object _Sender, ValueChangedEventArgs _Args)
	{
		Value = default;
		
		DataSnapshot snapshot = _Args?.Snapshot;
		
		if (snapshot == null)
			return;
		
		Value = Create(snapshot);
	}
}
