namespace UnityEngine 
{
    public enum Orientation
    {
        North = 90,
        NorthEast = 45,
        East = 0,
        SouthEast = -45,
        South = -90,
        SouthWest = -135,
        West = 180,
        NorthWest = 135
    }

    public enum AttackHeight
    {
        Head = 0,
        Torso = 1
    }


    public enum CharacterState
    {
        Idle,
        Knocked,
        Dodging
    }

    public enum AttackState
    {
        Idle,
        Attack,
        Stun,
        ShieldDefence,
        SwordDefence,
        BlockAttack,
    }

    public enum AimingInputState
    {
        Idle, 
        Moving, 
        Hold, 
        Cooldown
    }

    public enum Direction
    {
        Idle = 0, //no specific direction
        ToRight = 1,
        ToCenter = 2,
        ToLeft = 3, 
        Wrong = 4,//aimed to a bad direction, like for an invallid block
        Default // used for sending signal, on default dont change current direction in animator 
    }

    public enum AttackSignal
    {
        Idle,
        Stab,
        //Feint,
        Swing,
        Charge
    }

    public enum AttackType
    {
        Stab,
        HorizontalSlashToLeft,
        HorizontalSlashToRight,
        ShieldBash,
        None,
        Charge
    }

    public enum BodyParts 
    { 
        Head,
        Torso,
        LeftArm,
        RightArm,
        LeftLeg,
        RightLeg,
    }


    public enum BlockResult
    {
        
        //When no defence and clean hit
        //Take full damage and stamina loss
        Hit,
        //This will happen when you block with sword in right direction
        //This will half the damage taken.
        SwordBlock,
        //This will happen when you block with sword a stab from center position
        //Take 3/4 of the damage
        SwordHalfBlock,
        
        //This will happen when you are holding the shield up in center position while the attack comes either from left or right.
        //this will not cause damage but will cut down your Stamina more. 
        HalfBlocked, 
        //This will happen when you are holding the shield up in position of the attack.
        //this will take less Stamina from you and cause a small knockback to the opponent. 
        FullyBlocked,
        //This will take the least amount of Stamina and create the biggest opening to attack the opponent afterwards
        Parried
    }
    
    public enum BlockMedium
    {
        Shield = 0,
        Sword = 1,
        Nothing = 2
    }

    public enum EquipmentType
    {
        Melee,
        Ranged,
        Shield,
        Fist,
        Wood
    }
    
    public enum EquipmentHand
    {
       LeftHand = 0,
       RightHand = 1,
       TwoHanded = 2
    }


    public enum WorldStateType
    {
        Desired,
        Satisfying,
        Current
    }
    
    public enum EWorldState
    {
        Behaviour,
        Health,
        Stamina,
        RHEquipment,
        LHEquipment,
        ShieldState,
        HasTarget,
        HasRHEquipment,
        HasLHEquipment,

        TargetBehaviour,
        TargetHealth,
        TargetStamina,
        TargetRHEquipment,
        TargetLHEquipment,
        TargetShieldState,

        TargetOpening,
        TargetAttackRange,
        AttackRange

    }

    public enum EWorldStateValue
    {
        Full,
        High,
        Mid,
        Low,
        Zero,
        Default
    }
    
    public enum EWorldStatePossesion
    {       
        InPossesion,
        NotInPossesion,
        Default
    }

    public enum EBehaviourValue
    {
        Recovering,
        Attacking,
        Defending,
        Knock,
        Default,
        Idle,
        Searching,
        ChargeUp
    }
    
    public enum EWorldStateRange
    {
       InRange,
       OutOfRange,
       FarAway,
       Default
    }
      
    public enum EWorldStateShield
    {
       Centered,
       Right,
       Left,
       AttackDirection,
       Default
    }
     
    public enum CharacterMentality
    {
       Basic,
       Agressive,
       Coward
    }

    public enum AnimationState 
    {
        Empty,
        Idle,
        Walk,
        Run,
        SlashLeft,
        SlashRight,
        Stab,
        ShieldEquip,
        SwordEquip,
        ParryShieldLeft,
        ParryShieldRight,
        ParrySwordLeft,
        ParrySwordRight,
        SheathWeapon,
        DrawWeapon,
        DragShieldDown,
        Charge,
        PatchUp,
        PickUp,
        Stun,
        Hit,
        AttackFeedFackLow,
        AttackFeedFackHigh,
        BlockedHit,
        //values above 100 are used for in scene interactions
        Lean = 100,
        DipWater = 101,
        LookOver = 102,
        AlmostFalling = 103,
        Confused = 104,
        Angry = 105

    }

    public enum ObjectTarget
    {
        Player,
        Weapon,
        Shield,
        Forward,
        Backward,
        Side,
        PatrolPoint,
        InRadius
    }
    public enum AIInputAction
    {
        PatchUp,
        Dash,
        StopDash,
        PickUp,
        LockShield,
        GrabShield
    }

    public enum Size
    {
        None,
        Small,
        Medium,
        Large
    }

    public enum Biome 
    { 
        village,
        Forest,
        Mountain
    }
    
    public enum SpecialInput
    { 
        Default,
        ShieldGrab,
        PatchUp,
        PickUp,
        SheatSword,
        UnSheatSword,
        // values above 100 ar used to activate animations for scene interactions
        //Make sure that these are equal to the ones from the AnimationState
        Lean = 100,
        DipWater = 101,
        LookOver = 102,
        AlmostFalling = 103,
        Confused = 104,
        Angry = 105

    }



}