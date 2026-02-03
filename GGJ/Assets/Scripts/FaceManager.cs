using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FaceManager : SingletonMono<FaceManager> 
{
    [SerializeField] private Image[] faces;


    private void Start()
    {
        faces[0].gameObject.SetActive(false);
        faces[1].gameObject.SetActive(true);
        faces[2].gameObject.SetActive(false);
    }
    //÷ª‘ –Ì ‰»Î123
    public void ChangeFace(int index)
    {
       for(int i =0; i < faces.Length; i++)
        {
            if(i == index)
            {
                faces[i].gameObject.SetActive(true);
            }
            else
            {
                faces[i].gameObject.SetActive(false);
            }
        }

 
    }
}
