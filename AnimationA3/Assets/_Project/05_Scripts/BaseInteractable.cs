using UnityEngine;

public class BaseInteractable : MonoBehaviour, IInteractable
{
    public virtual void Interact()
    {
        
    }

    public virtual bool CanInteract()
    {
        return true;
    }
    
    
}