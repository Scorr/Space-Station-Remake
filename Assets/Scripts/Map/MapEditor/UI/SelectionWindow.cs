using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.IO;

internal class SelectionWindow : MonoBehaviour {

    public CustomAutoCompleteComboBox comboBox;

    private Dictionary<string, Atom> selectionDict = new Dictionary<string, Atom>();
    
    public string CurrentSelection {
        get {
            return comboBox.SelectedItem;
        }
    }

	void Start () {
        string[] data = ResourceManager.Instance.GetAllDataNames();

        for (int i = 0; i < data.Length; i++) {
            comboBox.AvailableOptions.Add(Path.GetFileNameWithoutExtension(data[i]));
        }
        
        comboBox.RebuildPanel();
    }
}
