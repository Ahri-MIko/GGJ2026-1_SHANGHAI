using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class SoundDataBatchTools
{
    // 这个特性会让选项出现在 SoundData 资源的右键菜单里
    [MenuItem("Assets/Quick Audio/填充 2001-2035 占位符")]
    public static void BatchAddSounds()
    {
        // 获取当前右键选中的对象
        SoundData data = Selection.activeObject as SoundData;

        if (data == null)
        {
            EditorUtility.DisplayDialog("提示", "请先选中一个 SoundData 资源文件", "OK");
            return;
        }

        Undo.RecordObject(data, "Batch Add Sound Names");

        // 逻辑：生成 2001 到 2035 的名字
        List<SoundEffectData> newList = new List<SoundEffectData>();

        // 如果你想保留之前的音效，取消下面这一行的注释：
        // if(data.soundEffectList != null) newList.AddRange(data.soundEffectList);

        for (int i = 2001; i <= 2035; i++)
        {
            SoundEffectData newEntry = new SoundEffectData();
            newEntry.soundName = i.ToString();
            newEntry.volume = 1.0f;
            newList.Add(newEntry);
        }

        data.soundEffectList = newList.ToArray();

        // 强制保存
        EditorUtility.SetDirty(data);
        AssetDatabase.SaveAssets();

        Debug.Log($"<color=cyan>成功为 {data.name} 生成了 2001-2035 的配置槽位！</color>");
    }
}