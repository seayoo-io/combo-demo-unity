using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InputFieldWithHistory : MonoBehaviour
{
    public InputField inputField;
    public Dropdown dropdown;

    public string playerPrefs;

    public List<string> historyRecords = new List<string>();
    private int dropdownSelectedIndex = -1;

    void Start()
    {
        LoadHistoryRecords();
        dropdown.onValueChanged.AddListener(OnDropdownItemSelected);
        inputField.onEndEdit.AddListener(OnSaveInput);
        InitDropdown();
    }
    
    void OnDestroy()
    {
        SaveHistoryRecords();
        dropdown.onValueChanged.RemoveListener(OnDropdownItemSelected);
        inputField.onEndEdit.RemoveListener(OnSaveInput);
    }

    private void InitDropdown()
    {
        dropdown.options.Insert(0, new Dropdown.OptionData(""));
        foreach (string item in historyRecords)
        {
            dropdown.options.Add(new Dropdown.OptionData(item));
        }
    }

    private void OnDropdownItemSelected(int index)
    {
        inputField.text = dropdown.options[index].text;
        dropdownSelectedIndex = index;
        dropdown.Hide();
    }

    private void UpdateDropdownOptions(List<string> items)
    {
        dropdown.ClearOptions();
        var options = new List<Dropdown.OptionData>();
        options.Insert(0, new Dropdown.OptionData(""));
        foreach (string item in items)
        {
            options.Add(new Dropdown.OptionData(item));
        }
        dropdown.AddOptions(options);
        dropdown.Show();
    }

    private void LoadHistoryRecords()
    {
        if (PlayerPrefs.HasKey(playerPrefs))
        {
            string historyString = PlayerPrefs.GetString(playerPrefs);
            historyRecords = new List<string>(historyString.Split(';'));
        }
    }

    private void SaveHistoryRecords()
    {
        string historyString = string.Join(";", historyRecords.ToArray());
        PlayerPrefs.SetString(playerPrefs, historyString);
        PlayerPrefs.Save();
    }

    private void OnSaveInput(string newText)
    {
        if(string.IsNullOrEmpty(newText))
        {
            if(dropdownSelectedIndex != -1 && dropdownSelectedIndex <= historyRecords.Count)
            {
                if(dropdownSelectedIndex - 1 < 0) return;
                historyRecords.RemoveAt(dropdownSelectedIndex - 1);
                dropdownSelectedIndex = -1;
                SaveHistoryRecords();
                UpdateDropdownOptions(historyRecords);
            }
            return;
        }
        if (!historyRecords.Contains(newText))
        {
            historyRecords.Add(newText);
            SaveHistoryRecords();
            UpdateDropdownOptions(historyRecords);
        }
    }
}
