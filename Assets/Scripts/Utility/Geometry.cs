using UnityEngine;
using UnityEngine.AI;
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
        public static Vector3 CalculateVectorFromfOrientation(float orientationRad)
        {
            return new Vector3(Mathf.Cos(orientationRad), 0f, Mathf.Sin(orientationRad));
        }

        public static Vector2 Calculate2DVectorFromfOrientation(float orientation)
        {
            float angle = (int)orientation * Mathf.Deg2Rad;
            return new Vector3(Mathf.Cos(angle), Mathf.Sin(angle));
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
            diff = diff > 180 ? diff - 360 : diff;


            if (diff > -centerAngle && diff < centerAngle)
                return Direction.ToCenter;
            else if (diff > 30 && diff < outerAngle || (isHoldingShield && diff > 30))
                return Direction.ToLeft;
            else if (diff < -30 && diff > -outerAngle || (isHoldingShield && diff < -30))
                return Direction.ToRight;
            return Direction.Wrong;
        }

        public static Direction CalculateFeintDirection(float orientation, Vector2 analogInput, float maxAngleToCenter)
        {
            //int orient = (int)orientation;
            int orient = (int)orientation;
            float input = CalculateAngleRadOfInput(analogInput) * Mathf.Rad2Deg;
            int diff = (int)input - orient;
            diff = diff < -180 ? 360 + diff : diff;
            diff = diff > 180 ? diff -360 : diff;

            //if (diff < 0 || diff > 180)                
            if (diff < 0 && diff > -maxAngleToCenter)                
                return Direction.ToLeft;
            if (diff > 0 && diff <  maxAngleToCenter)                
                return Direction.ToRight;
            else 
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

        public static Vector3 GetRandomPointOnNavMesh(Vector3 center, float radius, float minRadius)
        {
            for (int i = 0; i < 5; i++) // Try up to 5 times
            {
                Vector3 randomDirection = Random.insideUnitSphere * Random.Range(minRadius, radius);
                randomDirection += center;

                if (NavMesh.SamplePosition(randomDirection, out NavMeshHit hit, radius, NavMesh.AllAreas))
                {
                    return hit.position;
                }
            }

        // If no valid point found, return center
        return center;
        }

        public static Vector3 GetPointInDirectionOnNavMesh(Vector3 center, Vector3 direction, float radius)
        {
            int sign = 1;
            for (int i = 0; i < 10; i++) // Try up to 10 times
            {
                sign *= -1;
                Vector3 rotatedAngle = Quaternion.Euler(0, 5 * i * sign, 0) * direction;
                Vector3 target = center + rotatedAngle * radius;
                if (NavMesh.SamplePosition(target, out NavMeshHit hit, radius, NavMesh.AllAreas))
                {
                    if (Vector3.Distance(hit.position, center) > 1f)
                        return hit.position;
                }
            }
            // If no valid point found, return center
            return center;
        }


        public static Vector3 GetPointAtAngle(float angleDegrees)
        {
            float angleRadians = angleDegrees * Mathf.Deg2Rad;
            Vector3 direction = new Vector3(Mathf.Cos(angleRadians), 0, Mathf.Sin(angleRadians));
            return direction;
        }


        public static float CalculatefOrientationToTarget(Vector3 target, Vector3 self)
        {
            Vector3 direction = target - self;
            return Mathf.Atan2(direction.z, direction.x) * Mathf.Rad2Deg;
        }

        public static Orientation FindOrientationFromAngle(float fOrientation)
        {
            const int angleInterval = 45;
            int newOrientation = Mathf.RoundToInt(fOrientation / angleInterval);
            newOrientation *= angleInterval;

            if (newOrientation == -180)
                newOrientation = 180;
            return (Orientation)newOrientation;
        }

        //One way of deciding if input is a feint
        //This way it is decided by where your analog input starts compared to your orientation
        //Side is attack, below is feint
        private static bool IsFeintMovement(Direction direction, float feintAngle, Vector2 inputStart, float fOrientation)
        {
            var orientAngleRad = fOrientation * Mathf.Deg2Rad;

            var startAngleRad = CalculateAngleRadOfInput(inputStart) - orientAngleRad;
            startAngleRad = ClampAngle(startAngleRad);


            if (direction == Direction.ToLeft && startAngleRad > -feintAngle * Mathf.Deg2Rad && startAngleRad < 0f)
                return false;
            else if (direction == Direction.ToRight && startAngleRad < feintAngle * Mathf.Deg2Rad && startAngleRad > 0)
                return false;

            return true;
        }

        public static float BodyRotationAngleFromOrientation(Orientation orientation)
        {
            float angle = (float)orientation;
            angle = -angle + 90f;
            if (angle > 180)
                angle -= 360;
            return angle;
        }


        public static float CalculateSpearAngle(float inputAngle, float maxAngle)
        {
            float angle = inputAngle;
            float sign = Mathf.Sign(inputAngle);
            float absAngle = Mathf.Abs(angle);
            float deadAngle = 90f - maxAngle;

            if (absAngle > 90 + deadAngle)
            {
                //float newAngle = _maxAngle - (absAngle - _maxAngle);
                //angle = (newAngle >= 0f)? sign * newAngle : 0f;
                angle = sign * (180 - absAngle);
            }
            else if (absAngle > maxAngle)
            {
                angle = sign * maxAngle;
            }
            return angle;
        }

        public static bool InFrontOfAttacker(float OrientationAttacker, float orientationDefender, float margin)
        {
            float diff = OrientationAttacker - orientationDefender;
            float sign = Mathf.Sign(diff);
            return Mathf.Abs(diff * sign - 180) < margin;
        }

    }
}