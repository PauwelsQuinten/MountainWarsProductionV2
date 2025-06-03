using UnityEngine;

public class WalkBehaviour : StateMachineBehaviour
{
    private Vector2 _velocity;
    public Vector2 Velocity 
    { 
        get { return _velocity; }
        set { _velocity = value; }
    }


    
}
