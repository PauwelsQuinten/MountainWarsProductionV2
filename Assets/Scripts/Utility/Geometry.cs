using UnityEngine;
namespace Geometry
{
    public static class Geometry
    {
        public static float ClampAngle(float startAngleRad)
        {
            if (startAngleRad < -Mathf.PI)
                startAngleRad += Mathf.PI * 2f;
            else if (startAngleRad > Mathf.PI)
                startAngleRad -= Mathf.PI * 2f;
            return startAngleRad;
        }

        public static bool AreVectorWithinAngle(Vector2 one, Vector2 two, float angleDegree)
        {
            Vector2 nOne = one.normalized;
            Vector2 nTwo = two.normalized;
            float dot = Vector2.Dot(nOne, nTwo);

            return Mathf.Acos(dot) < angleDegree * Mathf.Deg2Rad;
        }

        public static float CalculateAngleRadOfInput(Vector2 direction)
        {
            return Mathf.Atan2(direction.y, direction.x);
        }

        public static bool IsInputInFrontOfOrientation(Vector2 direction, float acceptedRange, float orientation)
        {
            float angle = CalculateAngleRadOfInput(direction) * Mathf.Rad2Deg;
            float angleDiff = angle - orientation;
            return Mathf.Abs(angleDiff) < acceptedRange || Mathf.Abs(angleDiff) > 360 - acceptedRange;
        }

        public static Vector2 CalculateVectorFromOrientation(Orientation orientation)
        {
            float angle = (int)orientation * Mathf.Deg2Rad;
            return new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
        }

        public static float CalculateSwingSpeed(float length, float currentTime, float minResult, float maxResult)
        {
            if (currentTime == 0)
                return minResult;
            float speed = (length * 1 / currentTime) * 0.01f;
            speed = speed < minResult ? minResult : speed;
            return speed > maxResult ? maxResult : speed;
        }

        public static Direction CalculateBlockDirection(float orientation, Vector2 analogInput, bool isHoldingShield, float centerAngle, float outerAngle)
        {
            float length = analogInput.magnitude;
            if (length < 0.5f && !isHoldingShield)
                return Direction.Idle;

            //int orient = (int)orientation;
            int orient = (int)orientation;
            float input = CalculateAngleRadOfInput(analogInput) * Mathf.Rad2Deg;
            int diff = (int)input - orient;
            diff = diff < -180 ? 360 + diff : diff;


            if (diff > -centerAngle && diff < centerAngle)
                return Direction.ToCenter;
            else if (diff > 30 && diff < outerAngle || (isHoldingShield && diff > 30))
                return Direction.ToLeft;
            else if (diff < -30 && diff > -outerAngle || (isHoldingShield && diff < -30))
                return Direction.ToRight;
            return Direction.Wrong;
        }

        public static Direction CalculateSwingDirection(float angleDegree, Vector2 analogInput, Vector2 previousInput, Vector2 StorredStartInput)
        {
            Vector2 inputVec = Vector2.zero;
            if (analogInput == Vector2.zero)
                inputVec = analogInput;
            else
                inputVec = previousInput;//Debug switching--------------------------------------------------

            float cross = StorredStartInput.x * inputVec.y - StorredStartInput.y * inputVec.x;
            if (cross == 0f)
                return Direction.ToCenter;

            var startAngle = CalculateAngleRadOfInput(StorredStartInput);
            var endAngle = CalculateAngleRadOfInput(inputVec);

            if (angleDegree < 180)
                //if (Mathf.Abs(startAngle) + Mathf.Abs(endAngle) < Mathf.PI)
                return cross > 0 ? Direction.ToLeft : Direction.ToRight;

            return cross < 0 ? Direction.ToLeft : Direction.ToRight;
        }
    }
}