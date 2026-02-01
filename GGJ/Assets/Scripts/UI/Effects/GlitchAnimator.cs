using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class GlitchAnimator : MonoBehaviour
{
    [Header("故障强度")]
    public float positionShake = 5f; // UI模式下单位是像素，建议比 World 模式大一点 (例如 5-10)

    [Header("间歇设置")]
    public float minInterval = 2f;
    public float maxInterval = 5f;

    private SpriteRenderer _spriteRenderer;
    private Image _uiImage;
    private Vector3 _originalPos;

    void Start()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _uiImage = GetComponent<Image>();
        _originalPos = transform.localPosition;

        Invoke("TriggerGlitch", Random.Range(minInterval, maxInterval));
    }

    void TriggerGlitch()
    {
        // 1. 位置震动 (逻辑通用)
        transform.DOShakePosition(0.2f, new Vector3(positionShake, positionShake, 0), 20, 90, false, true)
            .OnComplete(() => transform.localPosition = _originalPos);

        // 2. 透明度闪烁 (区分组件)
        if (_spriteRenderer != null) DoGlitchFade(_spriteRenderer);
        else if (_uiImage != null) DoGlitchFade(_uiImage);

        Invoke("TriggerGlitch", Random.Range(minInterval, maxInterval));
    }

    // 使用泛型或者重载来处理不同类型的 Fade，这里用最简单的重载
    void DoGlitchFade(SpriteRenderer target)
    {
        Sequence flickerSeq = DOTween.Sequence();
        flickerSeq.Append(target.DOFade(0.2f, 0.05f));
        flickerSeq.Append(target.DOFade(1f, 0.05f));
        flickerSeq.Append(target.DOFade(0.4f, 0.05f));
        flickerSeq.Append(target.DOFade(1f, 0.05f));
    }

    void DoGlitchFade(Image target)
    {
        Sequence flickerSeq = DOTween.Sequence();
        flickerSeq.Append(target.DOFade(0.2f, 0.05f));
        flickerSeq.Append(target.DOFade(1f, 0.05f));
        flickerSeq.Append(target.DOFade(0.4f, 0.05f));
        flickerSeq.Append(target.DOFade(1f, 0.05f));
    }

    void OnDestroy()
    {
        transform.DOKill();
    }
}