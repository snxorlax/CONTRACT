using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class AnimateText : MonoBehaviour
{
    /// <summary>
    /// Start is called on the frame when a script is enabled just before
    /// any of the Update methods is called the first time.
    /// </summary>
    public TextMeshProUGUI text;
    public Animator textAnimator;
    private void Awake()
    {
        text = GetComponent<TextMeshProUGUI>();
        textAnimator = GetComponent<Animator>();
    }

    public void StartAnimation()
    {
        text.enabled = true;
        textAnimator.Play("Base Layer.DamageAnim", -1, 0);
    }
}
