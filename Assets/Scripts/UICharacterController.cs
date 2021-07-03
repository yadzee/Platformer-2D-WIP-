using UnityEngine.UI;
using UnityEngine;

public class UICharacterController : MonoBehaviour
{
    [SerializeField] private PressedButton left;
    [SerializeField] private PressedButton right;
    [SerializeField] private Button jump;
    [SerializeField] private Button fire;


    public PressedButton Left
    {
        get { return left; }
    }
    public PressedButton Right
    {
        get { return right; }
    }
    public Button Jump
    {
        get { return jump; }
    }
    public Button Fire
    {
        get { return fire; }
    }

    public void Start()
    {
        Player.Instance.InitController(this);
    }
}
