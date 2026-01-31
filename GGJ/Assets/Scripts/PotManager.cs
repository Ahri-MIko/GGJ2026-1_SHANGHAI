using UnityEngine;

/// <summary>
/// 锅管理器 - 管理7个节拍点的锅图片显隐
/// </summary>
public class PotManager : MonoBehaviour
{
    [Header("节拍点锅图片（共7个）")]
    [Tooltip("手动拖入7个锅图片，对应节拍1-7")]
    [SerializeField] private GameObject[] pots = new GameObject[7];
    
    [Header("箭头设置")]
    [SerializeField] private GameObject Arrow;
    [SerializeField] private int ArrowDistance = 70;
    
    [Header("默认箭头位置")]
    [Tooltip("非1-7拍时箭头的默认位置")]
    [SerializeField] private Transform defaultArrowPosition;

    private void Awake()
    {
        Arrow.SetActive(false);
    }

    public void ShowPot(int index)
    {
        if (index >= 1 && index <= 7)
        {
            pots[index - 1].SetActive(true);
        }
    }

    /// <summary>
    /// 隐藏指定编号的锅（编号从1开始）
    /// </summary>
    public void HidePot(int index)
    {
        if (index >= 1 && index <= 7)
        {
            pots[index - 1].SetActive(false);
        }
    }

    /// <summary>
    /// 显示箭头在指定锅盖位置（编号1-7）
    /// </summary>
    public void ShowArrowInIndex(int index)
    {
        if (index >= 1 && index <= 7 && Arrow != null)
        {
            // 激活箭头
            Arrow.SetActive(true);
            
            // 获取目标锅盖的位置
            GameObject targetPot = pots[index - 1];
            if (targetPot != null)
            {
                // 设置箭头位置为锅盖位置 + ArrowDistance（向上偏移）
                Vector3 newPosition = targetPot.transform.position;
                newPosition.y += ArrowDistance;
                Arrow.transform.position = newPosition;
            }
        }
    }

    /// <summary>
    /// 显示箭头在默认位置（用于非1-7拍）
    /// </summary>
    public void ShowArrowAtDefaultPosition()
    {
        if (Arrow != null)
        {
            // 激活箭头
            Arrow.SetActive(true);
            
            // 设置箭头到默认位置
            if (defaultArrowPosition != null)
            {
                Arrow.transform.position = defaultArrowPosition.position;
            }
        }
    }
}
