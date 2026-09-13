using UnityEngine;

namespace MeowStudio.Data
{
    [CreateAssetMenu(menuName = "DATA/Game settings", fileName = "GameSettings")]
	public class GameSettings: ScriptableObject
	{
		[field: SerializeField] public int maxLives { get; private set; }
		[field: SerializeField] public Vector2Int lifeRestoreTime { get; private set; } = new Vector2Int(0, 30);
    }
}

