using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening; // 必用

[RequireComponent(typeof(CanvasGroup))]
public class BasePanel : MonoBehaviour
{
    protected CanvasGroup canvasGroup;

    protected virtual void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
    }

    // 打开界面（带渐变动画）
    public virtual void Show()
    {
        if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
        gameObject.SetActive(true);
        canvasGroup.alpha = 0f;
        canvasGroup.DOFade(1f, 0.3f).SetUpdate(true); // SetUpdate(true) 保证暂停时也能播放
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
    }

    // 关闭界面
    public virtual void Hide()
    {
        if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
        canvasGroup.DOFade(0f, 0.3f).SetUpdate(true).OnComplete(() =>
        {
            gameObject.SetActive(false);
        });
    }
}
