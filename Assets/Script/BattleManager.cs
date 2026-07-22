using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BattleManager : MonoBehaviour
{
    public static BattleManager Instance;

    [SerializeField] private MagicData[] magicSlots;

    public BattleUnit player;
    public List<BattleUnit> enemies = new List<BattleUnit>();

    private IBattleCommand selectedCommand;
    private BattleUnit selectedTarget;

    private bool isExecuting = false;
    private bool escapeRequested = false;
    private Coroutine battleCoroutine;

    private Queue<IBattleCommand> interruptCommands = new Queue<IBattleCommand>();
    private bool canInterruptNow = false;
    private int reservedInterruptCount = 0;

    private int maxSkillUsesPerUnit = 3;
    private Dictionary<int, int> skillUseCounts = new Dictionary<int, int>();

    private int maxSpecialGauge = 3;
    private int currentSpecialGauge = 3;

    public bool HasSelectedCommand()
    {
        return selectedCommand != null;
    }

    public IBattleCommand ConsumeSelectedCommand()
    {
        var cmd = selectedCommand;
        selectedCommand = null;
        return cmd;
    }

    public BattleUnit GetSelectedTarget()
    {
        return selectedTarget;
    }

    public void ClearSelectedCommand()
    {
        selectedCommand = null;
        selectedTarget = null;
    }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Update()
    {
        if(GameManager.Instance.CurrentState != GameState.BattleCommand &&
           GameManager.Instance.CurrentState != GameState.BattleExecute)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            UseSpecialSkill();
        }
        if(Input.GetKeyDown(KeyCode.Alpha1))
        {
            UseSkill(0);
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            UseSkill(1);
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            UseSkill(2);
        }
        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            UseSkill(3);
        }
    }

    public void StartBattle(BattleUnit p, List<BattleUnit> enemyList)
    {
        if(p == null || enemyList == null || enemyList.Count == 0)
        {
            Debug.LogError("BattleUnit is null");
            return;
        }

        player = p;
        enemies = enemyList;
        escapeRequested = false;

        interruptCommands.Clear();
        reservedInterruptCount = 0;
        selectedCommand = null;
        selectedTarget = null;

        skillUseCounts.Clear();

        currentSpecialGauge = maxSpecialGauge;

        if(SpecialGaugeUI.Instance != null)
        {
            SpecialGaugeUI.Instance.SetGauge(currentSpecialGauge,maxSpecialGauge);
        }

        var skills = PlayerStatus.Instance.GetLearnedSkills();

        for(int i = 0; i < 4; i++)
        {
            skillUseCounts[i] = 0;
        }

        if(SkillBarUI.Instance != null)
        {
            SkillBarUI.Instance.Init();
        }

        if(BattleUI.Instance != null)
        {
            BattleUI.Instance.Show();
        }

        if(BattleCommandUI.Instance != null)
        {
            BattleCommandUI.Instance.Hide();
        }

        if (BattleStatusUI.Instance != null)
        {
            BattleStatusUI.Instance.Init(player, enemies);
        }

        GameManager.Instance.ChangeState(GameState.BattleCommand);

        if(battleCoroutine != null)
        {
            StopCoroutine(battleCoroutine);
        }
        battleCoroutine = StartCoroutine(BattleLoop());
    }

    private IEnumerator BattleLoop()
    {
        string enemyNames = string.Join("、 ",enemies.ConvertAll(e => e.data.unitName));
        yield return BattleLogUI.Instance.ShowLogAndWait($"{enemyNames}が現れた！");

        while (true)
        {
            if (escapeRequested)
            {
                EndBattleRoutine();
                yield break;
            }

            GameManager.Instance.ChangeState(GameState.BattleCommand);

            if (BattleCommandUI.Instance != null)
            {
                BattleCommandUI.Instance.ResetSelection();
                BattleCommandUI.Instance.Show();
            }

            BattleHelpLog.Instance.SetMessage("コマンドを選択してください");

            yield return new WaitUntil(() => selectedCommand != null || escapeRequested);
            
            if(!(selectedCommand is EscapeCommand))
            {
                canInterruptNow = true;
                yield return TryProcessInterrupt(player);

                bool allDeadAfterSkill = enemies.TrueForAll(e => e.IsDead());
                if (allDeadAfterSkill)
                {
                    int skillExpGained = 0;

                    foreach(var enemy in enemies)
                    {
                        if(enemy.IsDead() && !enemy.hasGivenExp)
                        {
                            enemy.hasGivenExp = true;
                            skillExpGained += enemy.data.expReward;
                        }
                    }

                    if(skillExpGained > 0)
                    {
                        int prevLevel = PlayerStatus.Instance.GetLevel();
                        int levelUpCount = PlayerStatus.Instance.AddExperience(skillExpGained);
                        int newLevel = PlayerStatus.Instance.GetLevel();

                        yield return BattleLogUI.Instance.ShowLogAndWait(
                            $"敵を全て倒した！\n経験値を{skillExpGained}獲得した",
                            true
                        );

                        if(levelUpCount > 0)
                        {
                            yield return BattleLogUI.Instance.ShowLogAndWait(
                                $"{player.GetUnitName()}はレベルアップした！\nLv.{prevLevel} → {newLevel}",
                                true
                            );
                        }
                    }

                    EndBattleRoutine();
                    yield break;
                }
            }

            isExecuting = true;

            GameManager.Instance.ChangeState(GameState.BattleExecute);

            if(selectedTarget == null && selectedCommand is AttackCommand)
            {
                Debug.LogError("Target is null");
                selectedCommand = null;
                isExecuting = false;
                continue;
            }

            yield return selectedCommand.Execute(player, selectedTarget);

            bool allDead = enemies.TrueForAll(e => e.IsDead());

            if (player.IsAwaken())
            {
                bool showMessage = allDead ? player.EndAwaken() : player.UpdateTurn();
                if (showMessage)
                {
                    yield return BattleLogUI.Instance.ShowLogAndWait(
                        $"{player.GetUnitName()}は平常心を取り戻した");
                }
            }

            int totalExpGained = 0;

            foreach(var enemy in enemies)
            {
                if (enemy.IsDead())
                {
                    if (!enemy.hasGivenExp)
                    {
                        enemy.hasGivenExp = true;
                        totalExpGained += enemy.data.expReward;
                    }
                }
                else
                {
                    allDead = false;
                }
            }

            if (totalExpGained > 0)
            {
                int prevLevel = PlayerStatus.Instance.GetLevel();
                int levelUpCount = PlayerStatus.Instance.AddExperience(totalExpGained);
                int newLevel = PlayerStatus.Instance.GetLevel();

                if (allDead)
                {
                    yield return BattleLogUI.Instance.ShowLogAndWait(
                        $"敵を全て倒した！\n経験値を{totalExpGained}獲得した",
                        true
                    );
                }
                else
                {
                    yield return BattleLogUI.Instance.ShowLogAndWait(
                        $"経験値を{totalExpGained}獲得した"
                    );
                }

                if(levelUpCount > 0)
                {
                    Debug.Log($"レベルアップ表示するはず: {prevLevel} → {newLevel}");
                    yield return BattleLogUI.Instance.ShowLogAndWait(
                        $"{player.GetUnitName()}はレベルアップした！\nLv.{prevLevel} → {newLevel}",
                        true
                    );
                    Debug.Log("レベルアップ表示完了！");
                } 
            }

            if (allDead)
            {
                EndBattleRoutine();
                yield break;
            }

            if (escapeRequested)
            {
                EndBattleRoutine();
                yield break;
            }

            canInterruptNow = true;
            yield return TryProcessInterrupt(player);

            selectedCommand = null;
            isExecuting = false;

            if(currentSpecialGauge < maxSpecialGauge)
            {
                currentSpecialGauge++;

                if(SpecialGaugeUI.Instance != null)
                {
                    SpecialGaugeUI.Instance.SetGauge(currentSpecialGauge, maxSpecialGauge);
                }
            }

            BattleStatusUI.Instance.Init(player, enemies);

            yield return new WaitForSeconds(0.5f);

            foreach(var enemy in enemies)
            {
                if(enemy.IsDead()) continue;

                canInterruptNow = false;

                yield return BattleLogUI.Instance.ShowLogAndWait($"{enemy.data.unitName}の攻撃！");

                int baseDamage = enemy.GetAttack();
                int damage = Random.Range(baseDamage - 8, baseDamage + 16 + 1);

                if (damage < 1)
                {
                    damage = 1;
                }

                player.TakeDamage(damage);
            
                yield return BattleLogUI.Instance.ShowLogAndWait(
                    $"{player.GetUnitName()}は{damage}のダメージを受けた！"
                );

                canInterruptNow = true;

                yield return TryProcessInterrupt(player);

                if (player.IsDead())
                {
                    yield return BattleLogUI.Instance.ShowLogAndWait("敗北した...");
                    EndBattleRoutine();
                    yield break;
                }
            }
        }
    }

    public void SetCommand(IBattleCommand command)
    {
        if (GameManager.Instance.CurrentState != GameState.BattleCommand)
        {
            return;
        }

        if(isExecuting && !(command is EscapeCommand))return;

        if (command == null)
        {
            Debug.LogError("Command is null");
            return;
        }

        selectedCommand = command;

        if(BattleCommandUI.Instance != null)
        {
            BattleCommandUI.Instance.Hide();
        }
    }

    public void UseMagic(MagicData magic)
    {
        if(magic == null) return;

        switch (magic.targetType)
        {
            case MagicTargetType.EnemySingle:
                BattleTargetUI.Instance.SetMagic(magic);
                break;

            case MagicTargetType.EnemyAll:
            case MagicTargetType.Self:
                break;
        }
    }

    public void RequestEscape()
    {
        escapeRequested = true;

        interruptCommands.Clear();

        reservedInterruptCount = 0;
    }

    public void ExecuteAttackCommand()
    {
        SetCommand(new AttackCommand());
    }

    public void SetTarget(BattleUnit target)
    {
        selectedTarget = target;
    }

    public void RequestInterrupt(IBattleCommand command)
    {
        if (command == null)  return;

        interruptCommands.Enqueue(command);

        if (!canInterruptNow)
        {
            reservedInterruptCount++;
        }
    }

    private IEnumerator TryProcessInterrupt(BattleUnit user)
    {
        if(interruptCommands.Count == 0) yield break;

        if (!canInterruptNow && reservedInterruptCount == 0) yield break;

        reservedInterruptCount = 0;

        while (interruptCommands.Count > 0)
        {
            if (escapeRequested) yield break;

            canInterruptNow = false;

            var cmd = interruptCommands.Dequeue();

            yield return cmd.Execute(user, null);
        }
    }

    void UseSkill(int index)
    {
        Debug.Log($"UseSkill index:{index}, skillUseCount.ContainsKey:{skillUseCounts.ContainsKey(index)}, Count:{skillUseCounts.Count}");
        SkillSlotType slot;

        switch (index)
        {
            case 0: slot = SkillSlotType.Heal; break;
            case 1: slot = SkillSlotType.Debuff; break;
            case 2: slot = SkillSlotType.Buff; break;
            case 3: slot = SkillSlotType.Utility; break;
            default: return;
        }

        var skill = PlayerStatus.Instance.GetSkill(slot);
        if (skill == null)
        {
            Debug.Log($"[UseSkill] slot:{slot}にスキルがない");
            return;
        }

        Debug.Log($"[UseSkill 発動: {skill.name}");

        if (!skillUseCounts.ContainsKey(index)) return;

        if (skillUseCounts[index] >= maxSkillUsesPerUnit)
        {
            Debug.Log("スキル使用回数上限");
            return;
        }

        skillUseCounts[index]++;

        SkillBarUI.Instance?.UseSkill(index);

        if (skill.type == SkillType.Utility)
        {
            RequestInterrupt(new SelectCommandInterrupt());
        }
        else
        {
            RequestInterrupt(new SkillCommand(skill));
        }
    }

    void UseSpecialSkill()
    {
        if(currentSpecialGauge < maxSpecialGauge)
        {
            Debug.Log("ゲージ不足");
            return;
        }

        currentSpecialGauge = 0;
        SpecialGaugeUI.Instance?.SetGauge(currentSpecialGauge, maxSpecialGauge);

        var skill = PlayerStatus.Instance.GetSkill(SkillSlotType.Special);
        if(skill == null) return;

        RequestInterrupt(new SkillCommand(skill));
    }

    private void EndBattleRoutine()
    {
        selectedCommand = null;
        selectedTarget = null;
        isExecuting = false;
        escapeRequested = false;

        skillUseCounts.Clear();

        if (SkillBarUI.Instance != null)
        {
            SkillBarUI.Instance.ResetAll();
        }

        if (BattleUI.Instance != null)
        {
            BattleUI.Instance.Hide();
        }

        GameManager.Instance.ChangeState(GameState.Exploring);

        battleCoroutine = null;
    }
}
