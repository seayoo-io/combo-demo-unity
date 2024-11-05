using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;

public class MailListManager
{
    private static MailListManager instance;
    private string filePath;

    private MailListManager()
    {
        
        filePath = Path.Combine(Application.persistentDataPath, $"{PlayerController.GetPlayer().role.roleId}mails.json");
    }

    public static MailListManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new MailListManager();
            }
            return instance;
        }
    }

    public void SaveMail(MailInfo mail)
    {
        List<MailInfo> dataList = LoadMails();
        dataList.Add(mail);
        SaveMails(dataList);
    }

    public List<MailInfo> LoadMails()
    {
        if (!File.Exists(filePath))
        {
            return new List<MailInfo>(); // 返回空的列表
        }

        string json = File.ReadAllText(filePath);
        var mails = JsonConvert.DeserializeObject<List<MailInfo>>(json, new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Objects
        }) ?? new List<MailInfo>();

        return mails;
    }

    public void DeleteMail(string referenceId)
    {
        List<MailInfo> dataList = LoadMails();
        var mailToRemove = dataList.FirstOrDefault(mail => mail.referenceId == referenceId);

        if (mailToRemove != null)
        {
            dataList.Remove(mailToRemove);
            SaveMails(dataList);
            Debug.Log($"邮件 {referenceId} 已删除。");
        }
        else
        {
            Debug.Log($"未找到 ID 为 {referenceId} 的邮件。");
        }
    }

    public void DeleteAllMail()
    {
        List<MailInfo> dataList = new List<MailInfo>(); // 创建一个空列表
        SaveMails(dataList); // 保存空列表到文件
        Debug.Log("所有邮件已删除。");
    }

    private void SaveMails(List<MailInfo> mails)
    {
        string json = JsonConvert.SerializeObject(mails, Formatting.Indented, new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Objects
        });
        File.WriteAllText(filePath, json);
    }
}