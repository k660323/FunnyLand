using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

[CreateAssetMenu(menuName ="Category",fileName ="Category_")]
public class Category : ScriptableObject, IEquatable<Category>
{
    [SerializeField]
    private string codeName;
    [SerializeField]
    private string displayName;

    public string CodeName => codeName;
    public string DisplayName => displayName;

    #region Operator
    public bool Equals(Category other)
    {
        if (other is null)
            return false;
        if (ReferenceEquals(other, this)) // 같은 값을 가리키는지 확인
            return true;
        if (GetType() != other.GetType())
            return false;

        return codeName == other.CodeName;
    }

    public override int GetHashCode() => (CodeName, DisplayName).GetHashCode();

    public override bool Equals(object other) => base.Equals(other);

    public static bool operator ==(Category Ihs, string rhs)
    {
        if (Ihs is null)
            return ReferenceEquals(rhs, null);
        return Ihs.CodeName == rhs || Ihs.DisplayName == rhs;
    }

    public static bool operator !=(Category Ihs, string rhs) => !(Ihs == rhs);

    #endregion
}
