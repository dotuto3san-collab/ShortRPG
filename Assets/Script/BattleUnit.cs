using UnityEngine;

public class BattleUnit : MonoBehaviour
{
    public BattleData data;
    public bool isPlayer;

    private int currentHP;

    private int attackBuff = 0;

    private float attackDebuffRate = 0f;
    private float defenseDebuffRate = 0f;

    private int awakenTurn = 0;
    private float awakenRate = 1.3f;

    [HideInInspector] public bool hasGivenExp = false;

    private int enemyActionIndex = 0;

    public int GetHP()
    {
        return currentHP;
    }

    public int GetMaxHP()
    {
        if(isPlayer && PlayerStatus.Instance != null)
        {
            return PlayerStatus.Instance.maxHP;
        }

        return data.MaxHP;
    }

    public string GetUnitName()
    {
        if(isPlayer && PlayerStatus.Instance != null)
        {
            return PlayerStatus.Instance.GetPlayerName();
        }

        return data.unitName;
    }

    public string GetBattleDisplayName()
    {
        if (isPlayer)
        {
            return GetUnitName();
        }

        if(BattleManager.Instance != null)
        {
            return BattleManager.Instance.GetEnemyDisplayName(this);
        }

        return GetUnitName();
    }

    public void Init()
    {
        attackBuff = 0;
        attackDebuffRate = 0f;
        defenseDebuffRate = 0f;

        awakenTurn = 0;
        hasGivenExp = false;

        enemyActionIndex = 0;

        if(data == null)
        {
            Debug.LogError("BattleData is null");
            return;
        }

        if (isPlayer)
        {
            if(PlayerStatus.Instance == null)
            {
                Debug.LogError("PlayerStatus.Instance Ç™ë∂ç›ÇµÇ‹ÇπÇÒ");
                currentHP = data.MaxHP;
                return;
            }
            Debug.Log("Load HP: " + PlayerStatus.Instance.currentHP);
            currentHP = PlayerStatus.Instance.currentHP;
        }
        else
        {
            currentHP = data.MaxHP;
        }   
    }

    public void TakeDamage(int dmg)
    {
        currentHP -= dmg;
        if (currentHP < 0) currentHP = 0;
        Debug.Log("Save HP:" + currentHP);

        if (isPlayer && PlayerStatus.Instance != null)
        {
            PlayerStatus.Instance.currentHP = currentHP;
        }

        if(BattleStatusUI.Instance != null)
        {
            BattleStatusUI.Instance.UpdateHP(this);
        }
    }

    public void Heal(int amount)
    {
        currentHP += amount;

        if (currentHP > GetMaxHP())
            currentHP = GetMaxHP();

        if (isPlayer && PlayerStatus.Instance != null)
        {
            PlayerStatus.Instance.currentHP = currentHP;
        }

        if (BattleStatusUI.Instance != null)
        {
            BattleStatusUI.Instance.UpdateHP(this);
        }
    }
    public void StartAwaken(int turn)
    {
        awakenTurn = turn;
    }

    public bool IsAwaken()
    {
        return awakenTurn > 0;
    }

    public bool EndAwaken()
    {
        if(awakenTurn > 0)
        {
            awakenTurn = 0;
            return true;
        }

        return false;
    }

    public bool UpdateTurn()
    {
        if(awakenTurn > 0)
        {
            awakenTurn--;

            if(awakenTurn == 0)
            {
                return true;
            }
        }
        return false;
    }

    public int GetAttack()
    {
        int atk = data.attack + attackBuff;

        if(attackDebuffRate > 0f)
        {
            atk = Mathf.RoundToInt(
                atk * (1f - attackDebuffRate));
        }

        if (IsAwaken())
        {
            atk = Mathf.RoundToInt(atk * awakenRate);
        }

        return Mathf.Max(0, atk);
    }

    public int GetDefense()
    {
        int defense = data.defense;

        if(defenseDebuffRate > 0f)
        {
            defense = Mathf.RoundToInt(
                defense * (1f - defenseDebuffRate));
        }

        return Mathf.Max(0, defense);
    }

    public void AddAttackBuff(int amount)
    {
        attackBuff += amount;
    }

    public void ApplyAttackDebuff(float rate)
    {
        attackDebuffRate = Mathf.Clamp01(
            attackDebuffRate + rate);
    }

    public void ApplyDefenseDebuff(float rate)
    {
        defenseDebuffRate = Mathf.Clamp01(
            defenseDebuffRate + rate);
    }

    public void Attack(BattleUnit target)
    {
        target.TakeDamage(GetAttack());
    }

    public EnemyActionData GetNextEnemyAction()
    {
        if(data.actionRotation == null ||
           data.actionRotation.Length == 0)
        {
            return null;
        }

        EnemyActionData action =
            data.actionRotation[enemyActionIndex];

        enemyActionIndex++;

        if(enemyActionIndex >= data.actionRotation.Length)
        {
            enemyActionIndex = 0;
        }

        return action;
    }

    public bool IsDead()
    {
        return currentHP <= 0;
    }
}
