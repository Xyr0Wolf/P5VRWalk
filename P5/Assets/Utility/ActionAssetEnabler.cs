using UnityEngine;
using UnityEngine.InputSystem;

public class ActionAssetEnabler : MonoBehaviour
{
    [SerializeField] InputActionAsset actionAsset;

    void OnEnable()
    {
        if(actionAsset != null) 
            actionAsset.Enable();
    }
}
