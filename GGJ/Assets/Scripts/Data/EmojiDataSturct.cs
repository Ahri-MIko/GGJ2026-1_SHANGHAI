using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EmojiDataStruct
{
    [Serializable]
    public class EmojiData
    {
        public string id;       // 表情包ID，例如 "Angry"
        public Sprite sprite;   // 对应的图片
    }
}
