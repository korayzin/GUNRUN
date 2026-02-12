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
        Random,
        Confirm
    }

    public Action action = Action.Random;

    private void Awake()
    {
        if (GetComponent<Collider>() != null)
            GetComponent<Collider>().isTrigger = true;
    }
}
