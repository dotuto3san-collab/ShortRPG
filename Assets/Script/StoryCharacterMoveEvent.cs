using UnityEngine;

public class StoryCharacterMoveEvent : MonoBehaviour
{
    [Header("ˆÚ“®‘ÎÛ")]
    [SerializeField] private StoryCharacterMover characterMover;

    [Header("ˆÚ“®İ’è")]
    [SerializeField] private string direction = "Right";

    [SerializeField] private int tileCount = 1;

    public void Move()
    {
        if(characterMover == null)
        {
            Debug.LogWarning(
                $"StoryCharacterMoveEvent: StoryCharacterMover‚ªİ’è‚³‚ê‚Ä‚¢‚Ü‚¹‚ñB" +
                $"Object = {gameObject.name}"
            );

            return;
        }

        Vector2Int moveDirection =
            StoryCharacterMover.GetDirection(direction);

        if(moveDirection == Vector2Int.zero)
        {
            return;
        }

        StartCoroutine(
            characterMover.Move(
                moveDirection,
                tileCount
            )
        );
    }
}
