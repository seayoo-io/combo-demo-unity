using UnityEngine;
using UnityEngine.UI;

public class TrackPanel : MonoBehaviour
{
    //public Dropdown evtSelector;
    //public Button submitBtn;
    //public Button cancelBtn;

    //private string selectedEvtName;

    //void Start()
    //{
    //    submitBtn.onClick.AddListener(OnSubmit);
    //    cancelBtn.onClick.AddListener(OnClose);
    //    evtSelector.onValueChanged.AddListener(OnSelectorValueChange);
    //    selectedEvtName = evtSelector.options[0].text;
    //}

    //public void Show()
    //{
    //    this.gameObject.SetActive(true);
    //}

    //void OnSelectorValueChange(int value)
    //{
    //    selectedEvtName = evtSelector.options[value].text;
    //}

    //void OnSubmit()
    //{
    //    switch (selectedEvtName)
    //    {
    //        case "OmniSDKCreateRoleEvent":
    //            OmniSDK.Track(
    //                new OmniSDKCreateRoleEvent
    //                {
    //                    roleInfo = new OmniSDKRoleInfo
    //                    {
    //                        userId = "mockUserID_CreateRole",
    //                        roleId = "mockRoleID_CreateRole",
    //                        roleLevel = "mockRoleLevel_CreateRole",
    //                        roleName = "mockRoleName_CreateRole",
    //                        roleVipLevel = "mockRoleVipLevel_CreateRole",
    //                        serverId = "mockServerID_CreateRole",
    //                        serverName = "mockServerName_CreateRole",
    //                    }
    //                }
    //            );
    //            break;
    //        case "OmniSDKEnterGameEvent":
    //            OmniSDK.Track(
    //                new OmniSDKEnterGameEvent
    //                {
    //                    roleInfo = new OmniSDKRoleInfo
    //                    {
    //                        userId = "mockUserID_EnterGame",
    //                        roleId = "mockRoleID_EnterGame",
    //                        roleLevel = "mockRoleLevel_EnterGame",
    //                        roleName = "mockRoleName_EnterGame",
    //                        roleVipLevel = "mockRoleVipLevel_EnterGame",
    //                        serverId = "mockServerID_EnterGame",
    //                        serverName = "mockServerName_EnterGame",
    //                    }
    //                }
    //            );
    //            break;
    //        case "OmniSDKRoleLevelUpEvent":
    //            OmniSDK.Track(
    //                new OmniSDKRoleLevelUpEvent
    //                {
    //                    roleInfo = new OmniSDKRoleInfo
    //                    {
    //                        userId = "mockUserID_LevelUp",
    //                        roleId = "mockRoleID_LevelUp",
    //                        roleLevel = "mockRoleLevel_LevelUp",
    //                        roleName = "mockRoleName_LevelUp",
    //                        roleVipLevel = "mockRoleVipLevel_LevelUp",
    //                        serverId = "mockServerID_LevelUp",
    //                        serverName = "mockServerName_LevelUp",
    //                    }
    //                }
    //            );
    //            break;
    //        case "OmniSDKPurchaseEvent":
    //            OmniSDK.Track(
    //                new OmniSDKPurchaseEvent
    //                {
    //                    userId = "mockUserID_Purchase",
    //                    orderId = "mockOrderID_Purchase",
    //                    productId = "1",
    //                    productName = "mockProductName_Purchase",
    //                    productDesc = "mockProductDesc_Purchase",
    //                    productUnitPrice = 1,
    //                    purchaseAmount = 1,
    //                    purchaseQuantity = 1,
    //                    gameOrderId = "mockGameOrderID_Purchase",
    //                    gameRoleId = "mockGameRoleID_Purchase",
    //                    gameRoleName = "mockGameRoleName_Purchase",
    //                    gameRoleLevel = "mockGameRoleLevel_Purchase",
    //                    gameRoleVipLevel = "mockGameRoleVipLevel_Purchase",
    //                    gameServerId = "mockGameServerID_Purchase",
    //                    currency = "mockCurrency_Purchase",
    //                    extJson = "{}"
    //                }
    //            );
    //            break;
    //        case "OmniSDKRevenueEvent":
    //            OmniSDK.Track(
    //                new OmniSDKRevenueEvent
    //                {
    //                    roleInfo = new OmniSDKRoleInfo
    //                    {
    //                        userId = "mockUserID_Revenue",
    //                        roleId = "mockRoleID_Revenue",
    //                        roleLevel = "mockRoleLevel_Revenue",
    //                        roleName = "mockRoleName_Revenue",
    //                        roleVipLevel = "mockRoleVipLevel_Revenue",
    //                        serverId = "mockServerID_Revenue",
    //                        serverName = "mockServerName_Revenue",
    //                    },
    //                    consumeNum = 1
    //                }
    //            );
    //            break;
    //        default:
    //            Toast.Show("Unsupported Event Type");
    //            break;
    //    }
    //    Toast.Show("事件完成上报");
    //    this.gameObject.SetActive(false);
    //}

    //void OnClose()
    //{
    //    this.gameObject.SetActive(false);
    //}
}
