using UnityEngine;
using UnityEngine.UI; // 引入 UI 命名空间
using DG.Tweening;

public class IdleAnimator : MonoBehaviour
{
    public enum AnimType
    {
        Float,      // 漂浮 (适合人物、面具)
        Swing,      // 摆动 (适合灯笼、对联) - UI模式下请修改RectTransform的Pivot
        Pulse,      // 呼吸缩放 (适合人物微调)
        Twinkle     // 闪烁 (适合星星)
    }

    [Header("动画设置")]
    public AnimType animationType = AnimType.Float;

    [Header("参数调整")]
    public float strength = 10f;
    public float duration = 2f;
    public bool useRandomDelay = true;

    // 内部变量，用于自动识别
    private SpriteRenderer _spriteRenderer;
    private Image _uiImage;
    private Tween _tween;

    void Start()
    {
        // 自动尝试获取两种组件
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _uiImage = GetComponent<Image>();

        float delay = useRandomDelay ? Random.Range(0f, 1f) : 0f;

        switch (animationType)
        {
            case AnimType.Float:
                // 无论是 UI 还是 World 物体，移动逻辑通用
                _tween = transform.DOLocalMoveY(transform.localPosition.y + strength, duration)
                    .SetEase(Ease.InOutSine)
                    .SetLoops(-1, LoopType.Yoyo)
                    .SetDelay(delay);
                break;

            case AnimType.Swing:
                // 旋转逻辑通用
                // ★注意：如果是 UI Image，请在 Inspector 里调整 RectTransform 的 Pivot (轴心)
                // y=1 代表顶部，y=0.5 代表中间
                _tween = transform.DOLocalRotate(new Vector3(0, 0, strength), duration)
                    .SetEase(Ease.InOutSine)
                    .SetLoops(-1, LoopType.Yoyo)
                    .SetDelay(delay);
                break;

            case AnimType.Pulse:
                // 缩放逻辑通用
                _tween = transform.DOScale(transform.localScale * (1 + strength / 100f), duration)
                    .SetEase(Ease.InOutSine)
                    .SetLoops(-1, LoopType.Yoyo)
                    .SetDelay(delay);
                break;

            case AnimType.Twinkle:
                // 闪烁逻辑需要区分组件
                HandleTwinkle(duration, delay);
                break;
        }
    }

    void HandleTwinkle(float time, float delayTime)
    {
        float targetAlpha = Mathf.Clamp01(1f - strength / 100f);

        if (_spriteRenderer != null)
        {
            // 处理 SpriteRenderer
            Color c = _spriteRenderer.color;
            c.a = targetAlpha;
            _spriteRenderer.color = c;
            _tween = _spriteRenderer.DOFade(1f, time).SetEase(Ease.InOutQuad).SetLoops(-1, LoopType.Yoyo).SetDelay(delayTime);
        }
        else if (_uiImage != null)
        {
            // 处理 UI Image
            Color c = _uiImage.color;
            c.a = targetAlpha;
            _uiImage.color = c;
            _tween = _uiImage.DOFade(1f, time).SetEase(Ease.InOutQuad).SetLoops(-1, LoopType.Yoyo).SetDelay(delayTime);
        }
    }

    void OnDestroy()
    {
        _tween?.Kill();
    }
}