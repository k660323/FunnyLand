using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class GaugeUI : MonoBehaviour
{
    Slider mySlider;
    
    public Image fill;
    public Color highRate;
    public Color midRate;
    public Color lowRate;

    void Start()
    {
        mySlider = GetComponent<Slider>();    
    }

    public void GaugeUpdate(float rate)
    {
        mySlider.value = rate;
        if(rate > 0.7f)
        {
            fill.color = highRate;
        }
        else if(rate > 0.4f)
        {
            fill.color = midRate;
        }
        else
        {
            fill.color = lowRate;
        }
    }
}
