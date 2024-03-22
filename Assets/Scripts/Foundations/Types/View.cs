using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

internal abstract class View<T> : MonoBehaviour
    where T : View<T>
{
    public ViewCallbacks callbacks = new ViewCallbacks();
    public Action<T> HideCompleted;
    protected abstract IEnumerator OnShow();
    protected abstract IEnumerator OnHide();

    private static readonly Dictionary<Type, List<T>> instances = new Dictionary<Type, List<T>>();

    public static T Instantiate()
    {
        var viewPrefabAttr = (ViewPrefabAttribute)
            Attribute.GetCustomAttribute(typeof(T), typeof(ViewPrefabAttribute));
        if (viewPrefabAttr == null)
        {
            Log.E("Cannot find prefab attribute for view " + typeof(T).Name);
            return null;
        }
        var prefabRes = viewPrefabAttr.Instantiate();
        if (prefabRes == null)
        {
            Log.E("Cannot instantiate prefab for view " + typeof(T).Name);
            return null;
        }
        var goInstance = GameObject.Instantiate(prefabRes) as GameObject;
        if (goInstance == null)
        {
            Log.E("Cannot instantiate gameObject for view " + typeof(T).Name);
            return null;
        }
        var compInstance = goInstance.GetComponent<T>();
        if (compInstance == null)
        {
            Log.E("Cannot find component for view " + typeof(T).Name);
            return null;
        }

        if (!instances.TryGetValue(typeof(T), out var instanceList))
        {
            instanceList = new List<T>();
            instances[typeof(T)] = instanceList;
        }
        instanceList.Add(compInstance);
        return compInstance;
    }

    public void Show()
    {
        gameObject.SetActive(true);
        callbacks.BeforeShow?.Invoke();
        callbacks.BeforeShow = null;
        StartCoroutine(
            CoroutineWithCallback(
                OnShow(),
                () =>
                {
                    callbacks.AfterShow?.Invoke();
                    callbacks.AfterShow = null;
                }
            )
        );
    }

    public void Hide()
    {
        callbacks.BeforeHide?.Invoke();
        callbacks.BeforeHide = null;
        StartCoroutine(
            CoroutineWithCallback(
                OnHide(),
                () =>
                {
                    gameObject.SetActive(false);
                    callbacks.AfterHide?.Invoke();
                    callbacks.AfterHide = null;
                }
            )
        );
    }

    public void Show(ViewCallbacks viewCallbacks)
    {
        SetCallbacks(viewCallbacks);
        Show();
    }

    public void Hide(ViewCallbacks viewCallbacks)
    {
        SetCallbacks(viewCallbacks);
        Hide();
    }

    public void Destroy()
    {
        if (instances.TryGetValue(typeof(T), out var instanceList))
        {
            instanceList.Remove((T)this);
        }
        GameObject.Destroy(gameObject);
    }

    public static void DestroyAll() 
    {
        if (instances.TryGetValue(typeof(T), out var instanceList))
        {
            var instanceListCopy = new List<T>(instanceList);
            foreach(var instance in instanceListCopy) {
                instance.Destroy();
            }
        }
    }

    void SetCallbacks(ViewCallbacks viewCallbacks)
    {
        callbacks.BeforeShow = viewCallbacks?.BeforeShow;
        callbacks.BeforeHide = viewCallbacks?.BeforeHide;
        callbacks.AfterShow = viewCallbacks?.AfterShow;
        callbacks.AfterHide = viewCallbacks?.AfterHide;
    }

    private IEnumerator CoroutineWithCallback(IEnumerator enumerator, Action onFinished)
    {
        yield return StartCoroutine(enumerator);
        onFinished?.Invoke();
    }
}

internal class ViewCallbacks
{
    public Action BeforeShow;
    public Action BeforeHide;
    public Action AfterShow;
    public Action AfterHide;
}

public class ViewPrefabAttribute : Attribute
{
    private string Path { get; }

    public ViewPrefabAttribute(string path)
    {
        Path = path;
    }

    public UnityEngine.Object Instantiate() => Resources.Load(Path);
}
