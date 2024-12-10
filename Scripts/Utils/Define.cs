using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Define
{
    public enum StateName
    {
        MOVE,
        DASH,
        ATTACK
    }

    public enum WolrdObject
    {
        Unknown,
        Player,
        Monster,
    }

    public enum MoveMethod
    {
        None,
        CController,
        RColliderController
    }

    public enum State
    {
        Die,
        Moving,
        Run,
        Idle,
        NormalAttack,
        NormalAttacking,
        Skill,
        CC,
        Jumping
    }

    public enum Layer
    {
        Creature = 6,
        Ground = 7,
        MyCreature = 8,
        Wall = 9
    }

    public enum GameState
    {
        None,
        Choice,
        ReChoice,
        Loading,
        Game,
        Result_0,
        Result_1,
        Result_2,
        Result_3,
        End
    }

    public enum GoodsType
    {
        NONE = 0,
        COIN = 1,
        DIA = 2
    }

    public enum ItemType
    {
        NONE = 0,
        WEAPON = 1,
        ARMOR = 2,
        CONSUMABLE = 3,
        INSTALLABLE = 4,
        ICON = 5,
        STYLE =6
    }

    public enum WeaponType
    {
       NONE = 0,
       MELEE = 1,
       RANGED = 2,
       MAGIC = 3
    }

    public enum AttackAnim
    {

    }

    public enum ArmorType
    {
        NONE = 0,
        HELMET = 1,
        TOP = 2,
        BOTTOM = 3,
        BOOTS = 4,
        EARRING = 5,
        RIGN = 6,
        CLOAK = 7,
        Shield = 8
    }

    public enum ConsumableType
    {
        NONE = 0,
        HPPOTION = 1
    }

    public enum ItemRating
    {
        Common = 0,
        Normal = 1,
        Rare = 2,
        Unique = 3,
        Legend = 4,
        Epic = 5
    }

    public enum Team
    {
        None,
        RedTeam,
        BlueTeam,
        BotTeam
    }

    public enum Scene
    {
        Unknown,
        Login,
        Lobby,
        Loading,
        Game,
        GamePlay
    }

    public enum Sound2D
    {
        Bgm,
        Effect2D,
        MaxCount,
    }

    public enum Sound3D
    {
        Effect3D,
    }

    public enum UIEvent
    {
        Click,
        Enter,
        Down,
        Drag,
        Up,
        Exit
    }

    public enum MouseEvent
    {
        LPress,
        LPointerDown,
        LPointerUp,
        LPointerOut,
        LClick,
        RPress,
        RPointerDown,
        RPointerUp,
        RPointerOut,
        RClick,
    }

    public enum DimensionMode
    {
        _2D,
        _3D
    }
    public enum CameraMode
    {
        QuaterView,
        FirstPerson,
        ThirdPerson
    }

    public enum PhotonObjectType
    {
        PlayerObject,
        RoomObject,
    }

    public enum PlayerList
    {
        LocalPlayer,
        ForcePlayer,
        Size
    }
    
    public enum PhysicsType
    {
        Position,
        Rotation
    }

    public enum PhysicsDir
    {
        Push,
        Pull,
        Up,
        Down,
        Knockback
    }

    public enum NavMoveDest
    {
        None,
        Target,
        Point
    }
}
