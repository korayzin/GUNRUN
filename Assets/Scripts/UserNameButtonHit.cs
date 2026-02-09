using UnityEngine;

/// <summary>
/// Ray bu objenin collider'ına çarptığında ve tetikleyiciye basıldığında
/// UserNameController'daki ilgili aksiyon çağrılır. Button/EventSystem gerekmez.
/// </summary>
[RequireComponent(typeof(Collider))]
public class UserNameButtonHit : MonoBehaviour
{
    public enum Action
    {
        Sample1,
        Sample2,
        Sample3,
        Confirm
    }

    public Action action = Action.Sample1;

    private void Awake()
    {
        if (GetComponent<Collider>() != null)
            GetComponent<Collider>().isTrigger = true;
    }
}
