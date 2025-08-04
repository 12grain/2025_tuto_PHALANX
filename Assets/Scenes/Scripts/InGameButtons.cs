using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class InGameButtons : MonoBehaviour
{

    public GameObject OptionPanel;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        OptionPanel.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    public void OptionButtonClicked() 
    {
        OptionPanel.SetActive(true);
    }

    public void OptionPanelExitButtonClicked()
    {
        OptionPanel.SetActive(false);
    }
}
