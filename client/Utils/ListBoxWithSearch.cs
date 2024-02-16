using client.Utils;
using ImGuiNET;

namespace client.Views;

public unsafe class ListBoxWithSearch : IDisposable
{
    private int _itemIndex = 0;
    private string _searchText = "";
    private string[] _items = Array.Empty<string>();
    private byte*[] _itemsUtf8 = new byte*[0];
    private byte* [] _filteredItems;
    private int _filteredItemsCount;
    private readonly List<int> _itemsIndexLookup = new();
    private readonly uint maxLength;
    private readonly int _heightInItems;
    private CString _label;
    private string _persistentLabel;

    public int CurrentItem => _itemIndex < _itemsIndexLookup.Count && _itemIndex >= 0 ? _itemsIndexLookup[_itemIndex] : -1;

    public ListBoxWithSearch(string persistentLabel, string[] items, uint maxLength, int heightInItems = -1)
    {
        _label = new CString(persistentLabel);
        _persistentLabel = persistentLabel;
        SetItems(items);
        this.maxLength = maxLength;
        _heightInItems = heightInItems;
    }

    public void SetItems(string[] items)
    {
        for (var i = 0; i < _itemsUtf8.Length; i++)
            F.FreeUtf8String(_itemsUtf8[i]);
            
        _items = (string[])items.Clone() ?? throw new ArgumentNullException(nameof(items));
        _itemsUtf8 = new byte*[_items.Length];
        _filteredItems = new byte*[_items.Length];
        _itemsIndexLookup.Clear();
            
        for (var i = 0; i < _items.Length; i++)
        {
            _itemsUtf8[i] = F.AllocUtf8String(_items[i]);
        }
            
        Filter();
    }

    private void Filter()
    {
        _filteredItemsCount = 0;
        _itemsIndexLookup.Clear();

        var searchEmpty = string.IsNullOrEmpty(_searchText.Trim());
            
        for (var i = 0; i < _items.Length; i++)
        {
            if (searchEmpty || _items[i].Contains(_searchText, StringComparison.OrdinalIgnoreCase))
            {
                _filteredItems[_filteredItemsCount++] = _itemsUtf8[i];
                _itemsIndexLookup.Add(i);
            }
        }
    }

    public bool Render()
    {
        ImGui.BeginGroup();
        if (ImGui.InputText("##Search" + _persistentLabel, ref _searchText, maxLength))
            Filter();


        bool changed;
        fixed (byte ** itemsPtr = _filteredItems)
        {
            fixed (int* currentItemPtr = &_itemIndex)
            {
                changed = ImGuiNative.igListBox_Str_arr(_label.Ptr, currentItemPtr, itemsPtr, _filteredItemsCount, _heightInItems) > 0;
            }
        }
            
        ImGui.EndGroup();
        return changed;
    }

    private void ReleaseUnmanagedResources()
    {
        _label.Dispose();
        for (var i = 0; i < _itemsUtf8.Length; i++)
            F.FreeUtf8String(_itemsUtf8[i]);
    }

    public void Dispose()
    {
        ReleaseUnmanagedResources();
        GC.SuppressFinalize(this);
    }

    ~ListBoxWithSearch()
    {
        ReleaseUnmanagedResources();
    }
}
