using UnityEngine;

namespace MeowStudio.Submarine
{
    public class SubmarineMove : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField, Min(0f)] private float maxForwardSpeed = 5f;
        [SerializeField, Min(0f)] private float maxReverseSpeed = 2f;
        [SerializeField, Min(0f)] private float maxDepthSpeed = 1.5f;
        [SerializeField, Min(0f)] private float maxTurnSpeed = 30f;
        [SerializeField, Min(0f)] private float maxDeptTurnSpeed = 30f;
        [SerializeField, Range(0f, 20f)] private float maxDepthAngle = 20f;

        [Header("Inertia")]
        [SerializeField, Min(0.01f)] private float movementAcceleration = 2f;
        [SerializeField, Min(0.01f)] private float movementDeceleration = 1f;
        [SerializeField, Min(0.01f)] private float depthAcceleration = 1f;
        [SerializeField, Min(0.01f)] private float depthDeceleration = 1f;
        [SerializeField, Min(0.01f)] private float turnAcceleration = 90f;
        [SerializeField, Min(0.01f)] private float turnDeceleration = 120f;
        [SerializeField, Min(0.01f)] private float depthTurnAcceleration = 30f;
        [SerializeField, Min(0.01f)] private float depthTurnDeceleration = 45f;

        private float _trottle;
        private float _steering;
        private float _depthControl;
        private float _currentSpeed;
        private float _currentDepthSpeed;
        private float _currentTurnSpeed;
        private float _depthSteering;
        private float _currentDepthAngle;
        private float _currentDepthTurnSpeed;

        public void SetTrottle(float value)
        {
            _trottle = Mathf.Clamp(value, -1f, 1f);
        }

        public void SetSteering(float value)
        {
            _steering = Mathf.Clamp(value, -90f, 90f);
        }

        public void SetDepthControl(float value)
        {
            _depthControl = Mathf.Clamp(value, -1f, 1f);
        }

        public void SetDepthSteering(float value)
        {
            _depthSteering = Mathf.Clamp(value, -1f, 1f);
        }

        private void Update()
        {
            float targetSpeed = _trottle >= 0f
                ? _trottle * maxForwardSpeed
                : _trottle * maxReverseSpeed;
            float targetDepthSpeed = _depthControl * maxDepthSpeed;
            float speedForTurning = _currentSpeed >= 0f
                ? maxForwardSpeed
                : maxReverseSpeed;
            float turnBySpeed = speedForTurning > Mathf.Epsilon
                ? Mathf.Clamp01(Mathf.Abs(_currentSpeed) / speedForTurning)
                : 0f;
            float targetTurnSpeed = Mathf.Abs(_currentSpeed) > Mathf.Epsilon
                ? _steering / 90f * maxTurnSpeed * turnBySpeed * Mathf.Sign(_currentSpeed)
                : 0f;
            float targetDepthAngle = Mathf.Lerp(
                -maxDepthAngle,
                maxDepthAngle,
                (_depthSteering + 1f) * 0.5f) * turnBySpeed;

            _currentSpeed = MoveWithInertia(
                _currentSpeed,
                targetSpeed,
                movementAcceleration,
                movementDeceleration);
            _currentDepthSpeed = MoveWithInertia(
                _currentDepthSpeed,
                targetDepthSpeed,
                depthAcceleration,
                depthDeceleration);
            _currentTurnSpeed = MoveWithInertia(
                _currentTurnSpeed,
                targetTurnSpeed,
                turnAcceleration,
                turnDeceleration);
            float previousDepthAngle = _currentDepthAngle;
            float targetDepthTurnSpeed = Mathf.Abs(targetDepthAngle - _currentDepthAngle) > Mathf.Epsilon
                ? Mathf.Sign(targetDepthAngle - _currentDepthAngle) * maxDeptTurnSpeed
                : 0f;
            _currentDepthTurnSpeed = MoveWithInertia(
                _currentDepthTurnSpeed,
                targetDepthTurnSpeed,
                depthTurnAcceleration,
                depthTurnDeceleration);
            float depthAngleStep = _currentDepthTurnSpeed * Time.deltaTime;
            float nextDepthAngle = _currentDepthAngle + depthAngleStep;
            bool reachedTarget = Mathf.Abs(targetDepthAngle - _currentDepthAngle) <= Mathf.Epsilon
                || (targetDepthAngle - _currentDepthAngle) * (targetDepthAngle - nextDepthAngle) <= 0f;
            if (reachedTarget)
            {
                _currentDepthAngle = targetDepthAngle;
                _currentDepthTurnSpeed = 0f;
            }
            else
            {
                _currentDepthAngle += depthAngleStep;
            }

            transform.Rotate(0f, _currentTurnSpeed * Time.deltaTime, 0f, Space.Self);
            transform.Rotate(0f, 0f, _currentDepthAngle - previousDepthAngle, Space.Self);
            transform.Translate(_currentSpeed * Time.deltaTime, 0f, 0f, Space.Self);
            transform.position += Vector3.up * _currentDepthSpeed * Time.deltaTime;
        }

        private float MoveWithInertia(
            float current,
            float target,
            float acceleration,
            float deceleration)
        {
            float rate = Mathf.Abs(target) > Mathf.Abs(current)
                ? acceleration
                : deceleration;

            return Mathf.MoveTowards(current, target, rate * Time.deltaTime);
        }
    }
}
