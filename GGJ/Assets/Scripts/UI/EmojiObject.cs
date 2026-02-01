using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class EmojiObject : MonoBehaviour
{
    public Image targetImage;

    private void Awake()
    {
        if (targetImage == null) targetImage = GetComponent<Image>();
    }

    public void Init(Sprite sprite, float duration = 2f)
    {
        targetImage.sprite = sprite;

        // --- 动画设计 (Juice) ---

        // 1. 初始状态：缩小为0，稍微旋转一点
        transform.localScale = Vector3.zero;
        transform.localRotation = Quaternion.Euler(0, 0, Random.Range(-15f, 15f));

        // 2. 进场：弹性弹出 (OutElastic)
        transform.DOScale(1f, 0.5f).SetEase(Ease.OutElastic);

        // 3. 停留一段时间后消失
        // 这里使用 Sequence 队列来管理一连串动作
        Sequence seq = DOTween.Sequence();

        // 等待 (duration - 0.5 - 0.3) 秒
        seq.AppendInterval(duration - 0.8f);

        // 退场：变成透明 + 向上飘 + 缩小
        seq.Append(targetImage.DOFade(0f, 0.3f));
        seq.Join(transform.DOLocalMoveY(100f, 0.3f).SetRelative(true)); // 向上飘
        seq.Join(transform.DOScale(0f, 0.3f).SetEase(Ease.InBack));

        // 4. 销毁
        seq.OnComplete(() => Destroy(gameObject));
    }
}