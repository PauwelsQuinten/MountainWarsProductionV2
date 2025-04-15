
using UnityEngine;
public struct Opening
{
    public Direction OpeningDirection;
    public Size OpeningSize;

    public Opening(Direction openingDirection, Size openingSize)
    {
        OpeningDirection = openingDirection;
        OpeningSize = openingSize;
    }
    public static bool operator ==(Opening opening1, Opening opening2)
    {
        return opening1.OpeningDirection.Equals(opening2.OpeningDirection) && opening1.OpeningSize.Equals(opening2.OpeningSize);
    }
    public static bool operator !=(Opening opening1, Opening opening2)
    {
        return !(opening1 == opening2);
    }
    public override bool Equals(object obj)
    {
        if (obj == null || GetType() != obj.GetType())
        {
            return false;
        }

        Opening other = (Opening)obj;
        return OpeningDirection == other.OpeningDirection && OpeningSize == other.OpeningSize;
    }
    public override int GetHashCode()
    {
        return OpeningDirection.GetHashCode() ^ OpeningSize.GetHashCode();
    }


}
