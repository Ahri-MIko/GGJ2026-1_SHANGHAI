using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class RoundController : MonoBehaviour
{
    private TextMeshProUGUI _textMeshPro;

    private void Awake()
    {
        // 在Awake中初始化，确保其他脚本在Start中调用SetRound时已经准备好
        _textMeshPro = GetComponent<TextMeshProUGUI>();
        
        if (_textMeshPro == null)
        {
            Debug.LogError($"RoundController: 在 {gameObject.name} 上找不到 TextMeshProUGUI 组件！");
            return;
        }
    }

    private void Start()
    {
        if (_textMeshPro != null)
        {
            _textMeshPro.text = $"Round:1";
        }
    }

    public void SetRound(int roundCount)
    {
        if (_textMeshPro != null)
        {
            _textMeshPro.text = $"Round:{roundCount}";
        }
        else
        {
            Debug.LogWarning($"RoundController: TextMeshProUGUI组件未初始化，无法设置Round:{roundCount}");
        }
    }
}
