using UnityEngine;

public class BattleUnit : MonoBehaviour
{
    public BattleData data;
    public bool isPlayer;

    private int currentHP;

    private int attackBuff = 0;

    private int awakenTurn = 0;
    private float awakenRate = 1.3f;

    [HideInInspector] public bool hasGivenExp = false;

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

    public void Init()
    {
        attackBuff = 0;
        awakenTurn = 0;
        hasGivenExp = false;

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

        if (IsAwaken())
        {
            atk = Mathf.RoundToInt(atk * awakenRate);
        }

        return atk;
    }

    public void AddAttackBuff(int amount)
    {
        attackBuff += amount;
    }

    public void Attack(BattleUnit target)
    {
        target.TakeDamage(GetAttack());
    }

    public bool IsDead()
    {
        return currentHP <= 0;
    }
}
