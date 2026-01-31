using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class IntervalBar : MonoBehaviour
{
    [SerializeField]private TextMeshProUGUI m_TextMeshPro;


    public void SetCapital(string cap)
    {
        m_TextMeshPro.text = cap;
    }
}
