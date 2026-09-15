VAR n_name = "兵士"
VAR p_name = "ブレイバー"
VAR m_name = "謎の声"

EXTERNAL SetFlag(flagName)
EXTERNAL HasFlag(flagName)
EXTERNAL MoveCharacter(id, direction, tiles)
EXTERNAL HideTextPanel()
EXTERNAL DespawnCharacter(id, fade)
EXTERNAL Wait(seconds)

{HasFlag("firstSoldier"):
- else:
~ SetFlag("firstSoldier")
#name: {n_name}
おはよう!ついにこの日が来たな！同期として誇らしいぜ
}

{not HasFlag("chestOpened"):
	{not HasFlag("soldierToldChest"):
		~ SetFlag("soldierToldChest")
		#name: {n_name}
		そこの宝箱に俺からの贈り物を入れておいた。喜んでくれると嬉しいぜ
	- else:
		#name: {n_name}
		まだ宝箱開けてないのか？早く開けてくれよ
		#name: {m_name}
		宝箱を開けるには、宝箱の前でEnterキーかZキーを押すと開けることが出来ますよ。
	}
	-> END
}

{not HasFlag("soldierToldChest"):
~ SetFlag("soldierToldChest")
#name: {n_name}
そこの宝箱に俺からの贈り物を入れておいた。喜んでくれると嬉しいぜ
#name: {n_name}
って、もう開けてたのか?!もう冒険者の心構えを持っているみたいだな
}

#name: {n_name}
贈り物はどうだ？うれしいか？

*[うれしいよ、ありがとう]
	#name: {n_name}
	そうか!喜んでくれて俺もうれしいぜ!

*[ドヤ顔すんな]
	#name: {n_name}
	別にいいだろ!ドヤ顔くらいしたっていいじゃないか

- #name: {n_name}
そうだ、宝箱が好きそうなお前にいいこと教えてあげるぜ

#name: {n_name}
実は、この世界のあちこちに宝箱が置いてあるという噂があるんだ。

#name: {n_name}
昔の冒険者たちが使わなくなった道具をそこに入れてるとか...

#name: {p_name}
ほう...ロマンがあるな

#name: {n_name}
だからって、欲張って中にある道具を何個も取っていくんじゃないぞ!地獄の魔王が罰を与えてくるぞ!

#name: {p_name}
そ、そうか。

#name: {n_name}
そろそろ雑談も終わりだ。2階の謁見の間に向かおうか
~ HideTextPanel()
~ MoveCharacter("Soldier", "Down", 1)
~ DespawnCharacter("Soldier", false)
~ Wait(0.3)
-> END