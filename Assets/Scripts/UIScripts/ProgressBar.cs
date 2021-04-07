using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode()]
public class ProgressBar : MonoBehaviour
{
    public int max;
    public int curr;
    public Image mask;


    void Update()
    {
        GetCurrentFill();
    }

    void GetCurrentFill()
    {
        float fillAmount = (float)curr / (float)max;
        mask.fillAmount = fillAmount;
    }
}
