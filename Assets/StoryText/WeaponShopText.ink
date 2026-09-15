VAR shop_name = "武器屋"

-> main

=== main ===
# name: {shop_name}
いらっしゃいませ!

+ [買いものをする]
    -> buy
+ [アイテムを売る]
    -> sell
+ [なにもしない]
    -> exit

=== buy ===
# shop
-> END

=== sell ===
# sell
-> END

=== exit ===
# dialogue
# name: {shop_name}
ありがとうございました。
-> END