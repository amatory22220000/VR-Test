using System;
using UnityEngine;

namespace MeowStudio.Input
{
    public interface IPlayerInput
    {
        void StartListenMoveDirection(Action<Vector2> onChange);
        void FinishListenMoveDirection(Action<Vector2> onChange);

        void StartListenAimPointPosition(Action<Vector2> onChange);
        void StartFinishAimPointPosition(Action<Vector2> onChange);
    }
}
