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

    public void SaveMail(MailBaseInfo mail)
    {
        List<MailBaseInfo> dataList = LoadMails();
        dataList.Add(mail);
        SaveMails(dataList);
    }

    public List<MailBaseInfo> LoadMails()
    {
        if (!File.Exists(filePath))
        {
            return new List<MailBaseInfo>(); // 返回空的列表
        }

        string json = File.ReadAllText(filePath);
        var mails = JsonConvert.DeserializeObject<List<MailBaseInfo>>(json, new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Objects
        }) ?? new List<MailBaseInfo>();

        // 按 isRead 属性排序，未读放前面
        return mails.OrderBy(mail => mail.isRead).ToList();
    }

    public void DeleteMail(Guid guid)
    {
        List<MailBaseInfo> dataList = LoadMails();
        var mailToRemove = dataList.FirstOrDefault(mail => mail.mailId == guid);

        if (mailToRemove != null)
        {
            dataList.Remove(mailToRemove);
            SaveMails(dataList);
            Debug.Log($"邮件 {guid} 已删除。");
        }
        else
        {
            Debug.Log($"未找到 ID 为 {guid} 的邮件。");
        }
    }

    public void DeleteAllMail()
    {
        List<MailBaseInfo> dataList = new List<MailBaseInfo>(); // 创建一个空列表
        SaveMails(dataList); // 保存空列表到文件
        Debug.Log("所有邮件已删除。");
    }

    private void SaveMails(List<MailBaseInfo> mails)
    {
        string json = JsonConvert.SerializeObject(mails, Formatting.Indented, new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Objects
        });
        File.WriteAllText(filePath, json);
    }
}