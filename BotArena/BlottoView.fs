module BotArena.BlottoView
open BotArena.Types

let stringBlottoView (blottoGame: BlottoGame) =
     let totalMatchCount = 
        blottoGame.completedMatches
        |> List.map(fun x -> x.results) 
        |> List.collect(fun x -> x)
        |> List.length

     let winnerResults = 
        blottoGame.completedMatches
        |> List.map(fun x -> x.results) 
        |> List.collect(fun x -> x)
        |> List.groupBy(fun x -> x.winnerBotId)
        |> List.map(fun (key, coll) -> (blottoGame.bots |> List.find(fun x -> x.id = key), float (coll |> List.length) / float totalMatchCount))
        |> List.map(fun (bot, winRate) -> (bot.name, winRate))

     let stringWinrateBoard winnerResults =
        winnerResults 
        |> List.sortByDescending(fun (bot, winRate) -> winRate)
        |> List.map(fun (bot, winRate) -> sprintf "%s %f" bot winRate)
        |> List.reduce(fun x y -> sprintf "%s\n%s" x y)
    
     stringWinrateBoard winnerResults
