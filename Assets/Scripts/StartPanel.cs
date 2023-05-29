using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StartPanel : MonoBehaviour
{
    public Image myImage;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    //public void SetMyImage(Sprite sprite)
    //{
    //    myImage.sprite=
    //}
    public void OnStartGameBtnFunction()
    {
        this.gameObject.SetActive(false);
        GPTManager.Instance.StartGPT();
    }
}
