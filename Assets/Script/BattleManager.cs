using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    private Queue<ReservedBattleAction> reservedCommands =
        new Queue<ReservedBattleAction>();
    private int reservedExtraActionCount = 0;

    private bool isPlayerCommandPhase = false;

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
        if (GameManager.Instance.CurrentState != GameState.BattleCommand &&
           GameManager.Instance.CurrentState != GameState.BattleExecute)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            UseSpecialSkill();
        }
        if (Input.GetKeyDown(KeyCode.Alpha1))
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
        if (p == null || enemyList == null || enemyList.Count == 0)
        {
            Debug.LogError("BattleUnit is null");
            return;
        }

        player = p;
        enemies = enemyList;
        escapeRequested = false;

        interruptCommands.Clear();
        reservedCommands.Clear();

        reservedInterruptCount = 0;
        reservedExtraActionCount = 0;

        selectedCommand = null;
        selectedTarget = null;

        skillUseCounts.Clear();

        currentSpecialGauge = maxSpecialGauge;

        if (SpecialGaugeUI.Instance != null)
        {
            SpecialGaugeUI.Instance.SetGauge(currentSpecialGauge, maxSpecialGauge);
        }

        for (int i = 0; i < 4; i++)
        {
            skillUseCounts[i] = 0;
        }

        if (SkillBarUI.Instance != null)
        {
            SkillBarUI.Instance.Init();
        }

        if (BattleUI.Instance != null)
        {
            BattleUI.Instance.Show();
        }

        if (BattleCommandUI.Instance != null)
        {
            BattleCommandUI.Instance.Hide();
        }

        if (BattleStatusUI.Instance != null)
        {
            BattleStatusUI.Instance.Init(player, enemies);
        }

        GameManager.Instance.ChangeState(GameState.BattleCommand);

        if (battleCoroutine != null)
        {
            StopCoroutine(battleCoroutine);
        }
        battleCoroutine = StartCoroutine(BattleLoop());
    }

    private IEnumerator BattleLoop()
    {
        string enemyNames = string.Join(
            "、 ",
            enemies.ConvertAll(e => e.GetBattleDisplayName()));

        yield return BattleLogUI.Instance.ShowLogAndWait(
            $"{enemyNames}が現れた！");

        canInterruptNow = true;
        yield return TryProcessInterrupt(player);

        if (AreAllEnemiesDead())
        {
            int preBattleExpGained = 0;

            foreach (var enemy in enemies)
            {
                if (enemy.IsDead() && !enemy.hasGivenExp)
                {
                    enemy.hasGivenExp = true;
                    preBattleExpGained += enemy.data.expReward;
                }
            }

            if (preBattleExpGained > 0)
            {
                int prevLevel = PlayerStatus.Instance.GetLevel();
                int levelUpCount = PlayerStatus.Instance.AddExperience(preBattleExpGained);
                int newLevel = PlayerStatus.Instance.GetLevel();

                yield return BattleLogUI.Instance.ShowLogAndWait(
                    $"敵を全て倒した！\n経験値を{preBattleExpGained}獲得した",
                    true
                );

                if(levelUpCount > 0)
                {
                    yield return BattleLogUI.Instance.ShowLogAndWait(
                        $"{player.GetUnitName()}はレベルアップした！\nLv.{prevLevel} -> {newLevel}",
                        true
                    );
                }
            }

            EndBattleRoutine();
            yield break;
        }

        while (true)
        {
            if (escapeRequested)
            {
                EndBattleRoutine();
                yield break;
            }

            isPlayerCommandPhase = true;

            GameManager.Instance.ChangeState(GameState.BattleCommand);

            if (BattleCommandUI.Instance != null)
            {
                BattleCommandUI.Instance.ResetSelection();
                BattleCommandUI.Instance.Show();
            }

            BattleHelpLog.Instance.SetMessage("コマンドを選択してください");

            yield return new WaitUntil(() => selectedCommand != null || escapeRequested);

            if (escapeRequested)
            {
                EndBattleRoutine();
                yield break;
            }

            reservedCommands.Enqueue(
                new ReservedBattleAction(
                    selectedCommand,
                    selectedTarget));

            selectedCommand = null;
            selectedTarget = null;

            while (HasReservedExtraAction())
            {
                yield return BattleLogUI.Instance.ShowLogAndWait("追加コマンド発動！");

                GameManager.Instance.ChangeState(GameState.BattleCommand);

                BattleCommandUI.Instance.ResetSelection();
                BattleCommandUI.Instance.Show();

                yield return new WaitUntil(() => selectedCommand != null);

                reservedCommands.Enqueue(
                    new ReservedBattleAction(
                        selectedCommand,
                        selectedTarget));

                selectedCommand = null;
                selectedTarget = null;

                ConsumeReservedExtraAction();
            }

            isPlayerCommandPhase = false;

            GameManager.Instance.ChangeState(GameState.BattleExecute);

            while (reservedCommands.Count > 0)
            {
                if (AreAllEnemiesDead())
                {
                    reservedCommands.Clear();
                    break;
                }

                var action = reservedCommands.Dequeue();

                yield return action.Command.Execute(
                    player,
                    action.Target);
            }

            if (!(selectedCommand is EscapeCommand))
            {
                canInterruptNow = true;
                yield return TryProcessInterrupt(player);

                bool allDeadAfterSkill = enemies.TrueForAll(e => e.IsDead());
                if (allDeadAfterSkill)
                {
                    int skillExpGained = 0;

                    foreach (var enemy in enemies)
                    {
                        if (enemy.IsDead() && !enemy.hasGivenExp)
                        {
                            enemy.hasGivenExp = true;
                            skillExpGained += enemy.data.expReward;
                        }
                    }

                    if (skillExpGained > 0)
                    {
                        int prevLevel = PlayerStatus.Instance.GetLevel();
                        int levelUpCount = PlayerStatus.Instance.AddExperience(skillExpGained);
                        int newLevel = PlayerStatus.Instance.GetLevel();

                        yield return BattleLogUI.Instance.ShowLogAndWait(
                            $"敵を全て倒した！\n経験値を{skillExpGained}獲得した",
                            true
                        );

                        if (levelUpCount > 0)
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

            foreach (var enemy in enemies)
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

                if (levelUpCount > 0)
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

            if (AreAllEnemiesDead())
            {
                int interruptExpGained = 0;

                foreach(var enemy in enemies)
                {
                    if(enemy != null &&
                       enemy.IsDead() &&
                       !enemy.hasGivenExp)
                    {
                        enemy.hasGivenExp = true;
                        interruptExpGained += enemy.data.expReward;
                    }
                }

                if(interruptExpGained > 0)
                {
                    int prevLevel = PlayerStatus.Instance.GetLevel();

                    int levelUpCount =
                        PlayerStatus.Instance.AddExperience(
                            interruptExpGained);

                    int newLevel = PlayerStatus.Instance.GetLevel();

                    yield return BattleLogUI.Instance.ShowLogAndWait(
                        $"敵を全て倒した！\n経験値を{interruptExpGained}獲得した",
                        true
                    );

                    if(levelUpCount > 0)
                    {
                        yield return BattleLogUI.Instance.ShowLogAndWait(
                            $"{player.GetUnitName()}はレベルアップした！\nLv.{prevLevel} -> {newLevel}",
                            true
                        );
                    }
                }

                EndBattleRoutine();
                yield break;
            }

            selectedCommand = null;
            isExecuting = false;

            if (currentSpecialGauge < maxSpecialGauge)
            {
                currentSpecialGauge++;

                if (SpecialGaugeUI.Instance != null)
                {
                    SpecialGaugeUI.Instance.SetGauge(currentSpecialGauge, maxSpecialGauge);
                }
            }

            BattleStatusUI.Instance.Init(player, enemies);

            yield return new WaitForSeconds(0.5f);

            foreach (var enemy in enemies)
            {
                if (enemy == null || enemy.IsDead())
                {
                    continue;
                }

                canInterruptNow = false;

                yield return ExecuteEnemyAction(enemy);

                canInterruptNow = true;

                yield return TryProcessInterrupt(player);

                if (player.IsDead())
                {
                    yield return BattleLogUI.Instance.ShowLogAndWait("敗北した...");
                    EndBattleRoutine();
                    yield break;
                }

                if (AreAllEnemiesDead())
                {
                    yield return ResolveEnemyDefeat();
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

        if (isExecuting && !(command is EscapeCommand)) return;

        if (command == null)
        {
            Debug.LogError("Command is null");
            return;
        }

        selectedCommand = command;

        if (BattleCommandUI.Instance != null)
        {
            BattleCommandUI.Instance.Hide();
        }
    }

    public void UseMagic(MagicData magic)
    {
        if (magic == null) return;

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

    public BattleUnit ResolveAttackTarget(BattleUnit reservedTarget)
    {
        if(reservedTarget != null && !reservedTarget.IsDead())
        {
            return reservedTarget;
        }

        foreach(var enemy in enemies)
        {
            if(enemy != null && !enemy.IsDead())
            {
                return enemy;
            }
        }

        return null;
    }

    public string GetEnemyDisplayName(BattleUnit enemy)
    {
        if(enemy == null)
        {
            return "";
        }

        if (enemy.isPlayer)
        {
            return enemy.GetUnitName();
        }

        string baseName = enemy.data.unitName;

        int sameEnemyCount = 0;
        int sameEnemyIndex = 0;

        foreach(var currentEnemy in enemies)
        {
            if(currentEnemy == null || currentEnemy.isPlayer)
            {
                continue;
            }

            if(currentEnemy.data == enemy.data)
            {
                sameEnemyCount++;

                if(currentEnemy == enemy)
                {
                    sameEnemyIndex = sameEnemyCount;
                }
            }
        }

        if(sameEnemyCount <= 1)
        {
            return baseName;
        }

        char suffix = (char)('A' + sameEnemyIndex - 1);

        return $"{baseName}{suffix}";
    }

    public void EnqueueReservedAction(
        IBattleCommand command,
        BattleUnit target)
    {
        if (command == null)
        {
            Debug.LogError("Reserved command is null");
            return;
        }

        reservedCommands.Enqueue(
            new ReservedBattleAction(
                command,
                target));
    }

    public void RequestInterrupt(IBattleCommand command)
    {
        if (command == null) return;

        interruptCommands.Enqueue(command);
    }

    private IEnumerator TryProcessInterrupt(BattleUnit user)
    {
        if (interruptCommands.Count == 0) yield break;

        if (!canInterruptNow && reservedInterruptCount == 0) yield break;

        reservedInterruptCount = 0;

        while (interruptCommands.Count > 0)
        {
            if (escapeRequested) yield break;

            if (AreAllEnemiesDead())
            {
                interruptCommands.Clear();
                reservedCommands.Clear();
                reservedExtraActionCount = 0;
                yield break;
            }

            canInterruptNow = false;

            var cmd = interruptCommands.Dequeue();

            yield return cmd.Execute(user, null);
        }
    }

    public void ReservedExtraAction()
    {
        reservedExtraActionCount++;
    }

    public bool HasReservedExtraAction()
    {
        return reservedExtraActionCount > 0;
    }

    public void ConsumeReservedExtraAction()
    {
        if (reservedExtraActionCount > 0)
        {
            reservedExtraActionCount--;
        }
    }

    void UseSkill(int index)
    {
        Debug.Log($"[UseSkill] 仲間スキル index:{index}");

        if (CompanionManager.Instance == null)
        {
            Debug.LogError("[UseSkill] CompanionManager.Instanceが存在しません");
            return;
        }

        int companionIndex = index + 1;

        CompanionStatus companion =
            CompanionManager.Instance.GetCompanion(companionIndex);

        if (companion == null)
        {
            Debug.Log($"[UseSkill] 仲間{index + 1}が存在しません");
            return;
        }

        SkillData skill = companion.EquippedSkill;

        if (skill == null)
        {
            Debug.Log(
                $"[UseSkill] 仲間{index + 1}に装備スキルがありません");

            return;
        }

        Debug.Log(
            $"[UseSkill] 仲間{index + 1} / " +
            $"スキル:{skill.skillName} / " +
            $"Type:{skill.type}");

        if (!skillUseCounts.ContainsKey(index))
        {
            return;
        }

        if (skillUseCounts[index] >= maxSkillUsesPerUnit)
        {
            Debug.Log("スキル使用回数上限");
            return;
        }

        skillUseCounts[index]++;

        SkillBarUI.Instance?.UseSkill(index);

        if (skill.type == SkillType.Utility)
        {
            if (isPlayerCommandPhase)
            {
                ReservedExtraAction();
            }
            else
            {
                RequestInterrupt(new SelectCommandInterrupt());
            }
        }
        else
        {
            if (isPlayerCommandPhase)
            {
                EnqueueReservedAction(
                    new SkillCommand(
                        skill,
                        companion.Data.companionName),
                    null);
            }
            else
            {
                RequestInterrupt(
                    new SkillCommand(
                        skill,
                        companion.Data.companionName));
            }
        }
    }

    void UseSpecialSkill()
    {
        if (currentSpecialGauge < maxSpecialGauge)
        {
            Debug.Log("ゲージ不足");
            return;
        }

        var skill = PlayerStatus.Instance.GetSkill(SkillSlotType.Special);

        currentSpecialGauge = 0;
        SpecialGaugeUI.Instance?.SetGauge(currentSpecialGauge, maxSpecialGauge);

        if (skill == null) return;

        if (isPlayerCommandPhase)
        {
            EnqueueReservedAction(
                new SkillCommand(
                    skill,
                    player.GetUnitName()),
                null);
        }
        else
        {
            RequestInterrupt(
                new SkillCommand(
                    skill,
                    player.GetUnitName()));
        }
    }

    public bool AreAllEnemiesDead()
    {
        return enemies.TrueForAll(e => e == null || e.IsDead());
    }

    private IEnumerator ExecuteEnemyAction(BattleUnit enemy)
    {
        EnemyActionData action = enemy.GetNextEnemyAction();

        if(action == null)
        {
            Debug.LogWarning(
                $"{enemy.GetBattleDisplayName()}に敵行動が設定されていません"
            );

            yield break;
        }

        if(action is EnemyIdleData idleData)
        {
            yield return ExecuteEnemyIdle(
                enemy,
                idleData.idleText);

            yield break;
        }

        if(enemy.data.idleCance > 0f &&
           Random.value < enemy.data.idleCance)
        {
            yield return ExecuteEnemyRandomIdle(enemy);
            yield break;
        }

        if(action is EnemyAttackData attackData)
        {
            yield return ExecuteEnemyAttack(enemy, attackData);
            yield break;
        }

        Debug.LogWarning(
            $"未対応の敵行動: {action.GetType().Name}"
        );
    }

    private IEnumerator ExecuteEnemyRandomIdle(BattleUnit enemy)
    {
        string idleText = enemy.data.randomIdleText;

        if (string.IsNullOrEmpty(idleText))
        {
            Debug.LogWarning(
                $"{enemy.GetBattleDisplayName()}の確率さぼりテキストが表示されません"
            );

            yield return BattleLogUI.Instance.ShowLogAndWait(
                $"{enemy.GetBattleDisplayName()}はさぼっている!"
            );
        }

        else
        {
            yield return BattleLogUI.Instance.ShowLogAndWait(
                $"{enemy.GetBattleDisplayName()}は{idleText}"
            );
        }

        yield return new WaitForSeconds(0.8f);
    }

    private IEnumerator ExecuteEnemyIdle(
        BattleUnit enemy,
        string idleText)
    {
        if (string.IsNullOrEmpty(idleText))
        {
            Debug.LogWarning(
                $"{enemy.GetBattleDisplayName()}の確定さぼりテキストが設定されていません。"
            );

            yield return BattleLogUI.Instance.ShowLogAndWait(
                $"{enemy.GetBattleDisplayName()}はさぼっている！"
            );
        }
        else
        {
            yield return BattleLogUI.Instance.ShowLogAndWait(
                $"{enemy.GetBattleDisplayName()}は{idleText}"
            );
        }

        yield return new WaitForSeconds(0.5f);
    }

    private IEnumerator ExecuteEnemyAttack(
        BattleUnit enemy,
        EnemyAttackData action)
    {
        yield return BattleLogUI.Instance.ShowLogAndWait(
            $"{enemy.GetBattleDisplayName()}の{action.actionName}"
        );

        yield return new WaitForSeconds(0.3f);

        int baseDamage =
            enemy.GetAttack() + action.damage;

        int damage = Random.Range(
            baseDamage + action.minRandomDamage,
            baseDamage + action.maxRandomDamage
        );

        if(damage < 1)
        {
            damage = 1;
        }

        player.TakeDamage(damage);

        yield return BattleLogUI.Instance.ShowLogAndWait(
            $"{player.GetUnitName()}は{damage}のダメージを受けた！"
        );
    }

    private IEnumerator ResolveEnemyDefeat()
    {
        int expGained = 0;

        foreach(var enemy in enemies)
        {
            if(enemy != null &&
               enemy.IsDead() &&
               !enemy.hasGivenExp)
            {
                enemy.hasGivenExp = true;
                expGained += enemy.data.expReward;
            }
        }

        if (expGained > 0)
        {
            int prevLevel = PlayerStatus.Instance.GetLevel();

            int levelUpCount =
                PlayerStatus.Instance.AddExperience(expGained);

            int newLevel = PlayerStatus.Instance.GetLevel();

            yield return BattleLogUI.Instance.ShowLogAndWait(
                $"敵を全て倒した！\n経験値を{expGained}獲得した",
                true
            );

            if (levelUpCount > 0)
            {
                yield return BattleLogUI.Instance.ShowLogAndWait(
                    $"{player.GetUnitName()}はレベルアップした！\n" +
                    $"Lv.{prevLevel} → {newLevel}",
                    true
                );
            }
        }
        else
        {
            yield return BattleLogUI.Instance.ShowLogAndWait(
                "敵を全て倒した！",
                true
            );
        }

        EndBattleRoutine();
    }

    private void EndBattleRoutine()
    {
        selectedCommand = null;
        selectedTarget = null;
        isExecuting = false;
        escapeRequested = false;

        skillUseCounts.Clear();

        interruptCommands.Clear();
        reservedCommands.Clear();

        reservedExtraActionCount = 0;
        reservedInterruptCount = 0;

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
