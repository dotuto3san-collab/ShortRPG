using System.Collections;
using UnityEngine;

public class StoryCharacterMover : MonoBehaviour
{
    [Header("識別ID")]
    [SerializeField] private string characterId;

    public string CharacterId => characterId;

    [Header("移動設定")]
    [SerializeField] private float moveSpeed = 5f;

    [Header("消える演出設定")]
    [SerializeField] private float despawnFadeDuration = 0.5f;

    private bool isMoving;

    private void Start()
    {
        if(StoryCharacterPositionManager.Instance != null &&
           !string.IsNullOrEmpty(characterId) &&
           StoryCharacterPositionManager.Instance.TryGetPosition(characterId, out Vector3 savedPos))
        {
            transform.position = savedPos;
        }

        RegisterCharacter();
    }

    private void OnDisable()
    {
        if (StoryCharacterRegistry.Instance == null)
        {
            return;
        }

        if (string.IsNullOrEmpty(characterId))
        {
            return;
        }

        StoryCharacterRegistry.Instance.Unregister(
            characterId
        );
    }

    private void RegisterCharacter()
    {
        if (StoryCharacterRegistry.Instance == null)
        {
            Debug.LogWarning(
                $"StoryCharacterMover: StoryCharacterRegistry.Instanceが存在しません。" +
                $"Object = {gameObject.name}"
            );

            return;
        }

        if (string.IsNullOrEmpty(characterId))
        {
            Debug.LogWarning(
                $"StoryCharacterMover: characterIdが設定されていません。" +
                $"Object = {gameObject.name}"
            );

            return;
        }

        StoryCharacterRegistry.Instance.Register(
            characterId,
            this
        );

        Debug.Log(
            $"StoryCharacterMover: キャラクターを登録しました。" +
            $"ID = {characterId}, Object = {gameObject.name}"
        );
    }

    public IEnumerator Move(Vector2Int direction, int tileCount)
    {
        if (isMoving)
        {
            yield break;
        }

        if(tileCount <= 0)
        {
            yield break;
        }

        if(direction == Vector2.zero)
        {
            yield break;
        }

        if(direction.x != 0)
        {
            direction.y = 0;
        }
        else if(direction.y != 0)
        {
            direction.x = 0;
        }

        isMoving = true;

        for(int i = 0;i < tileCount; i++)
        {
            yield return StartCoroutine(
                MoveOneTile(direction)
            );
        }

        isMoving = false;

        if(StoryCharacterPositionManager.Instance != null && !string.IsNullOrEmpty(characterId))
        {
            StoryCharacterPositionManager.Instance.RecordPosition(characterId, transform.position);
        }
    }

    public IEnumerator Despawn(bool useFade)
    {
        if (useFade)
        {
            SpriteRenderer sr = GetComponent<SpriteRenderer>();

            if(sr != null)
            {
                Color startColor = sr.color;
                float t = 0f;

                while(t < despawnFadeDuration)
                {
                    t += Time.deltaTime;
                    float alpha = Mathf.Lerp(1f, 0f, t / despawnFadeDuration);
                    sr.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
                    yield return null;
                }
            }
        }

        gameObject.SetActive(false);
    }

    private IEnumerator MoveOneTile(Vector2Int direction)
    {
        Vector3 startPosition = transform.position;

        Vector3 moveDirection = new Vector3(
            direction.x,
            direction.y,
            0f
        );

        Vector3 targetPosition = startPosition + moveDirection;

        while(Vector3.Distance(transform.position, targetPosition) > 0.01f)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                targetPosition,
                moveSpeed * Time.deltaTime
            );

            yield return null;
        }

        transform.position = targetPosition;
    }

    public static Vector2Int GetDirection(string direction)
    {
        switch (direction)
        {
            case "Up":
                return Vector2Int.up;

            case "Down":
                return Vector2Int.down;

            case "Left":
                return Vector2Int.left;

            case "Right":
                return Vector2Int.right;

            default:
                Debug.LogWarning(
                    $"StoryCharacterMover: 不正な方向です。Direction = {direction}"
                );

                return Vector2Int.zero;
        }
    }
}
