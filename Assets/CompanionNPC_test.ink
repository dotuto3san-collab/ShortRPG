VAR npc_name = "ステリー"
EXTERNAL JoinCompanion()

# name: {npc_name}
こんにちは

# name: {npc_name}
仲間になってもいいですか？

* [いいよ]
    # name: {npc_name}
    よろしくね。
    ~ JoinCompanion()
    {npc_name}が仲間になった！

* [やだ]
# name: {npc_name}
    嫌い。

- # name: {npc_name}
    選択したあとの共通テキストです。

-> END