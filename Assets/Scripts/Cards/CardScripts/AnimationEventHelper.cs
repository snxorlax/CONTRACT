using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationEventHelper : MonoBehaviour
{
    public AnimateCard animateCard;
    public void CompleteDestroy()
    {
        animateCard.CompleteDestroyCard();
    }
}
