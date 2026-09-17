using UnityEngine;

public class CategoryManager : MonoBehaviour
{
    public Transform parentTransform;
    // 发行平台前缀，与 ProductManager 里按 productName 做前缀匹配的规则对应（忽略大小写）
    private static readonly string[] Categories = { "iOS", "Android", "HarmonyOS", "WebGL", "Windows" };

    void Start()
    {
        foreach (var category in Categories)
        {
            AppendCategoryItem(category, category);
        }
        AppendCategoryItem("全部商品", null);
    }

    private void AppendCategoryItem(string title, string categoryPrefix)
    {
        var view = CategoryItemView.Instantiate();
        view.SetTitle(title);
        view.SetClickCallback(() => UIController.ShowShopView(categoryPrefix, title));
        view.gameObject.transform.SetParent(parentTransform, false);
        view.Show();
    }
}
