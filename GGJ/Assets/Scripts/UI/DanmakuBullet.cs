using UnityEngine;
using TMPro;
using DG.Tweening; // 必用

public class DanmakuBullet : MonoBehaviour
{
    public TextMeshProUGUI textComp;
    public RectTransform rectTrans;

    private void Awake()
    {
        if (textComp == null) textComp = GetComponent<TextMeshProUGUI>();
        if (rectTrans == null) rectTrans = GetComponent<RectTransform>();
    }

    /// <summary>
    /// 发射弹幕
    /// </summary>
    /// <param name="content">文字内容</param>
    /// <param name="color">文字颜色（区分Boss和玩家）</param>
    /// <param name="duration">飞过去需要几秒（越小越快）</param>
    /// <param name="yPos">高度（Y轴坐标）</param>
    public void Fire(string content, Color color, float duration, float yPos)
    {
        // 1. 初始化状态
        textComp.text = content;
        textComp.color = color;

        // 2. 设定起点 (屏幕右侧外)
        // 假设 Canvas 是 1920 宽，那么 1100 肯定在屏幕外了
        // 更严谨的做法是获取父物体的 rect.width，但在 Jam 里直接写死或者给个大数也行
        float startX = 1200f;
        float endX = -1200f;

        rectTrans.anchoredPosition = new Vector2(startX, yPos);

        // 3. 开始飞行 (线性运动 Linear 最像弹幕)
        rectTrans.DOAnchorPosX(endX, duration)
            .SetEase(Ease.Linear)
            .OnComplete(() =>
            {
                // 飞出屏幕后销毁自己
                Destroy(gameObject);
            });
    }
}