module BotArena.BlottoView
open BotArena.Types

let stringBlottoView blottoGame =
    let totalMatchCount = blottoGame.results |> List.length

    let winnerResults = 
        blottoGame.results
        |> List.groupBy(fun x -> x.winnerBotId)
        |> List.map(fun (key, coll) -> (blottoGame.bots |> List.find(fun x -> x.id = key), float (coll |> List.length) / float totalMatchCount))
        |> List.map(fun (bot, winRate) -> (bot.name, winRate))

    let stringWinrateBoard winnerResults =
        winnerResults 
        |> List.sortBy(fun (bot, winRate) -> winRate)
        |> List.map(fun (bot, winRate) -> sprintf "%s %f" bot winRate)
        |> List.reduce(fun x y -> sprintf "%s\n%s" x y)
    
    stringWinrateBoard winnerResults